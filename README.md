# Архитектура

* __Модель Pull__.
* __Брокер на ASP.NET управляет бд__
* *Producer = Consumer


## Струтура БД( базовая)
* Таблица топиков
* Таблица партиций
* Таблица сообщений
* Таблица консьюмеров
* Таблица продюсеров
* Таблица консьюмер групп
* Таблица оффсетов консьюмер групп

## Структура сообщений
* Полезная нагрузка(какая то моделька, которую будет обрабатывать консьюмер)
* Ключ
* Название топика
* Заголовки(опционально)
* В бд есть статус(pending, processing, processed) для каждой консьюмер группы для реализаций стратегии доставки

## Структура и конфигурирование проекта
1. Брокер на ASP + Docker file(описывается какие топики создать и сколько в них партиций для бд при поднятии контейнера и инициализации, какие консьюмер группы создать и на какие топики подписать)
2. Docker image postgres (создание кластера и бд для брокера)
3. Библиотека клиент для продюсера (интерфейс - регистрировать продюсера в брокер, отправлять сообщения, получать результат)
4. Библиотека клиент для консьюмера (интерфейс -регистрировать консьюмера, поллить сообщения и коммитить оффсет)
5. Консьюмер(ы) - ASP приложение(я), использует библиотеку клиент консьюмера
6. Продюсер(ы) - ASP приложения(я), используют библиотеку клиент продюсера 
7. Docker compose - объединение как минимум 1 экземпляра каждого из пунктов(1,2,5,6)

#### Преимущества
1. Динамическое добавление и удаление продюсеров и коньсюмеров (поднимаем приложение ASP и юзаем библиотечку) -> масштабирование
2. Динамическое добавление топиков и партиций (запрос на создание в бд) -> масштабирование

#### Нюансы
1. Брокер должен отслеживать состояние запомнивших консьюмеров и периодоически пинговать их.
2. Каждый раз когда присоединяется консьюмер или умирает, брокер должен по какому то алгоритму распределять партиции между консьюмерами и запоминать их. Для каждой партиции будет свой оффсет в рамках одной консьюмер группы, у другой группы он будет уже другой

#### Случай репликации брокеров
1. nginx будет распределять нагрузку между продюсерами и консьюмерами на брокеры
2. Нужен будет скорее всего Redis для сохранения консистентности между брокерами(например sequence number продюсеров, оффсеты консьюмер групп и тд)


## Как соообщение проходит от начала до конца
1. Продюсер отправляет сообщение c основной структурой брокеру + guid продюсера + sequence number(uint) сообщения  -> реализуется идемпотентность:
- Брокер положил сообщение и отправил ОК -> ничего не делаем
- Ответ не приходит по таймауту -> ретраи n раз 
- Ответ пришел с ошибкой 5** -> ретраи n раз
2. Брокер хранит предыдущие last_sequnce_number и получает сообщение:
    1. new_sequence_number == last_sequence_number + 1 -> OK продюсеру
    2. new_sequence_number == last_sequence_number -> ответ продюсеру, что сообщение отправлено повторно
    3. new_sequence_number > last_sequence_number + 1 -> ответ продюсеру, что какое то сообщение затерялось, но все равно кладет в бд
3. Смотрит на присланные ключ сообщения и название топика:
    1. ключ == null -> сам распределяет в партиции по какому то алгоритму, который учитывает баланс партиций
    2. ключ != null -> определяет по ключу в какую партицию нужно положить, также нужен алгоритм, который должен минимизировать дисбаланс сообщений в партициях 
4. Консьюмер поллит сообщеньку
    1. Брокер смотрит какая партиция принадлежит консьюмеру, смотрит оффсет его группы и отправляет ему нужное количество сообщений(батч), оффсет и количество сообщений в батче и присваивает им статус "в обработке" для конкретной консьюмер группы
    2. После того как консьюмер обработал сообщение(я), он должен отправить ответ:
        1. Ответ пришел с ошибкой 5** - отправляем в отдельный топик для плохих сообщений
        2. Ответ пришел нормальный - помечаем сообщения как обработанные, но если пришедший коммит меньше чем сохраненный, то не сохраняем пришедший
    3. Если консьюмер просит сообщения, а они находятся в обработке текущей группы (значит произошло перераспределение до того как консьюмер обработал сообщения), то:
        1. Брокер сохраняет предыдущее состояние ассоциаций партиций и консьюмеров, и если перераспределение произошло, потому что умер консьюмер, обрабатывающий данную партцию, то происходит откат статуса сообщений
        2. Если же это не так, то консьюмер берет сообщение после тех записей которые сейчас в обработке
    4. Если приходит коммит, меньший чем сохранен сейчас -> ШАГ 2 с пришедшим коммитом , пришедший коммит никак не сохраняем, потому что другой консьюмер уже коммитит большие значения и сохранен коммит уже актуального консьюмера

#### Преимущества
1. Увеличивается пропускная способность за счет паралеллизации получения сообщений между консьюмерами, каждый из низ читает из своей партиции, но отказоустойчивости можно достигнуть только засчет реплик бд
2. Достигается exactly-once - сообщение обрабатывается только один раз
3. Сообщение может быть быть обработано повторно, если мы того пожелаем за счет подписки консьюмер групп на топики и использования оффсетов
4. Отправка сообщений батчами также для увеличения пропускной способности
5. Сохранение порядка сообщений, если нужно обрабатывать сообщения в нужном порядке, то они будут добавляться в одну партицию за счет одинаковых ключей
6. Если нужен ответ продюсеру, то он может класть его в отдельный топик, откуда консьюмеры будут брать сообщения, обрабатывать их и класть обратно, обработалось или нет
