using ProducerClient;
using ProducerClient.Models;

var producerBuilder = new ProducerBuilder<Ignore, string>("http://api:5163");

var producer = producerBuilder.Build();
await producer.RegisterAsync();
var i = 0;
while (true)
{
    var message = new Message<Ignore, string>()
    {
        Payload = i++.ToString()
    };
    await producer.SendAsync("topic1", message);
    await Task.Delay(3000);
}
