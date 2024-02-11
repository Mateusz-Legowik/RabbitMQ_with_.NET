using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.ComponentModel;
using System.Reflection;
using System.Text;

var factory = new ConnectionFactory() { HostName = "localhost" };
using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    //Key logic - ExchangeType.Direct
    channel.ExchangeDeclare(exchange: "MATT_TEST", type:
        ExchangeType.Direct);
    //Automatic queue declaration
    var queueName = channel.QueueDeclare().QueueName;

    //Operation on the "Mail" key
    RoutingKey key1 = RoutingKey.Mail;
    //Automatic Bind declaration - key declaration for the queue
    channel.QueueBind(queue: queueName, exchange: "MATT_TEST",
        routingKey: key1.GetDescription<RoutingKey>());

    //Operation on the "Test" key
    RoutingKey key2 = RoutingKey.Test;
    //Automatic Bind declaration - key declaration for the queue
    channel.QueueBind(queue: queueName, exchange: "MATT_TEST",
        routingKey: key2.GetDescription<RoutingKey>());

    Console.WriteLine(" [*] Waiting for messages.");

    var consumer = new EventingBasicConsumer(channel);
    //Event handling based on the above declarations - handling messages with a specific routingKey
    consumer.Received += (model, ea) =>
    {
        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
        var routingKey = ea.RoutingKey;
        Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);
    };
    
    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}



public enum RoutingKey
{
    [Description("SMS")]
    SMS = 1,
    [Description("Mail")]
    Mail = 2,
    [Description("Test")]
    Test = 3
}

//Casting an enumerated type to a string
public static class Helper
{
    public static string GetDescription<T>(this T enumerationValue)
    where T : struct
    {
        Type type = enumerationValue.GetType();
        if (!type.IsEnum)
        {
            throw new ArgumentException
                ("EnumerationValue must be of Enum type", "enumerationValue");
        }

        //Tries to find a DescriptionAttribute for a potential friendly name
        //for the enum
        MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attrs != null && attrs.Length > 0)
            {
                //Pull out the description value
                return ((DescriptionAttribute)attrs[0]).Description;
            }
        }
        //If we have no description attribute, just return the ToString of the enum
        return enumerationValue.ToString();
    }
}