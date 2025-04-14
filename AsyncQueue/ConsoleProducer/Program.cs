using ProducerClient;
using ProducerClient.Models;

var producerBuilder = new ProducerBuilder<Ignore, string>("");

var producer = producerBuilder.Build();
await producer.RegisterAsync();
var i = 0;
while (true)
{
    var message = new Message<Ignore, string>()
    {
        Payload = i++.ToString()
    };
    await producer.SendAsync("topic", message);
}
