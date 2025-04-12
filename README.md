# Архитектура

* __Модель Pull__.
* __Брокер на ASP.NET управляет бд__
* *Producer = Consumer


## Струтура БД( базовая)
* Таблица топиков
* Таблица партиций
* Таблица сообщений
* Таблица оффсетов консьюмер групп

## Структура сообщений
* Полезная нагрузка
* Ключ
* Заголовки(опционально)
* В бд есть статус(pending, processing, processed) для реализаций стратегии доставки

## Как происходит взаимодействие(случай без репликаций брокера)
1. Поднимается брокер и бд
2. Продюсеры обращаются по апишке к брокеру и кладут сообщение в очередь по топику. Продюсеры отправляют свой айди и монотонно возрастающее значение для сообщения, таким образом реализуется идемпотентность дабы не допустить повторной отправки сообщения на брокер
2. Брокер записывает в бд и присваивает статус pending для сообщения
3. Несколько консьюмеров объединяются в группы и подписываются на топики, таким образом брокер будет знать про коньсюмеров.
4. Брокер:
    * Добавляет запись в ConsumerGroupSubscriptions
    * Обновляет ConsumerGroupMembers
    * Запускает перебалансировку
5. Таким образом партиции будут распределяться между консьюмерами по какой-то стратегии(Range, Round Robin)
6. Консьюмеры из разных групп могут иметь разный оффсет на партицию и читать сообщения несколько раз для своей обработки.
7. Консьюмеры должны будут коммитить свои оффсеты на брокер, чтобы в случае падения запросить оффсет для чтения
8. Реализация стратегий доставок:
    * At-most-once - консьюмер перед тем как забрать сообщение коммитит оффсет и таким образом он только один раз обрабатывает сообщение, но сообщение может быть не обработано, если консьюмер упадет
    * At-least-once - консьюмер после того как обработал сообщение, закоммитил оффсет, таким образом сообщение обработается минимум один раз
    * Exactly-once - для этого проверям статус сообщения
9. Если мы получили сообщение с ошибкой обработки или лимит прошел, то после определенного количества попыток перемещаем сообщение куда нибудь для анализа, возможно альтернатива использовать что то по типу экспоненциального откладывания.

## Случай репликации брокеров
1. nginx будет распределять нагрузку между продюсерами и консьюмерами на брокеры

Продумать 
* как конфигурировать и запускать брокер вместе с консьюмерами и продюсерами
* как тестировать работоспособность

## Как соообщение проходит от начала до конца
1. Продюсер отправляет сообщение(вместе с айдишником своим из за того что может быть много продюсеров и айдишником сообщения) брокеру:
- ОК -> шаг 2
- Ответ не приходит по таймауту -> повторная отправка сколько то попыток 
- Ответ пришел с ошибкой 5** -> повторная отправка сколько то попыток
2. Брокер получает сообщение от продюсера и кладет в бд
- Айдишник -> ключ партиции
3. Консьюмер поллит сообщеньку
- Брокер 




