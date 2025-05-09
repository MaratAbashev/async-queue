using ProducerClient;
using ProducerClient.Models;

var brokerUrl = Environment.GetEnvironmentVariable("MESSAGEBROKER__HOST") ?? throw new ArgumentNullException("Broker host not found");
var producerBuilder = new ProducerBuilder<Ignore, string>(brokerUrl);

var producer = producerBuilder.Build();
await producer.RegisterAsync();
var i = 0;
var topic = Environment.GetEnvironmentVariable("MESSAGEBROKER__TOPIC") ?? throw new ArgumentNullException("Topic not found");
while (true)
{
    var message = new Message<Ignore, string>()
    {
        Payload = i++.ToString()
    };
    await producer.SendAsync(topic, message);
    await Task.Delay(3000);
}
