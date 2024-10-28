using System;
using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Implementations;
using MQTTnet.Extensions.TopicTemplate;
using MQTTnet.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Authentication;

namespace BambuLabBasicMQTTClient
{
    class Program
    {
        static string printerIP = "192.168.0.136";
        static int printerPort = 8883;
        static string printerAccessCode = "0ad0517d";
        static readonly MqttTopicTemplate sampleTemplate = new("device/+/report");
        IMqttClientAdapterFactory channelFactory;
        IMqttNetLogger logger;
        MqttClient mqCli;
        static async Task msg_recv(MqttApplicationMessageReceivedEventArgs e)
        {
            var pl = System.Text.Encoding.Default.GetString(e.ApplicationMessage.Payload);
            Console.WriteLine(pl);
        }
        static async Task Subscribe_Topic()
        {
            IMqttClientAdapterFactory channelFactory = new MqttClientAdapterFactory();
            IMqttNetLogger logger = new MqttNetEventLogger();
            MqttClient mqCli = new MqttClient(channelFactory, logger);
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(printerIP, printerPort).WithTlsOptions(
                                    o =>
                                    {
                                            o.WithCertificateValidationHandler(_ => true);
                                            o.WithSslProtocols(SslProtocols.Tls12);
                                    }).WithCredentials("bblp", printerAccessCode).Build();

            mqCli.ApplicationMessageReceivedAsync += msg_recv;
            await mqCli.ConnectAsync(mqttClientOptions, CancellationToken.None);
            MqttClientSubscribeOptions mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder().WithTopicTemplate(sampleTemplate).Build();
            var response = await mqCli.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
            Console.WriteLine("MQTT client subscribed to topic.");
        }
        static async Task Main(string[] args)
        {
            await Subscribe_Topic();
            Console.WriteLine("Waiting for messages!");
            Console.ReadLine();
        }
    }
}