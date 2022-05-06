using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Rechnungsverwaltung.Model;

namespace Rechnungsverwaltung.MQTT
{
    class MQTTClient
    {
        private IMqttFactory factory = new MqttFactory();
        private IMqttClient mqttClient;

        public async void Init()
        {
            mqttClient = factory.CreateMqttClient();
            // Create TCP based options using the builder.
            var options = new MqttClientOptionsBuilder()
                .WithClientId("Client1")
                .WithTcpServer("localhost")
                //.WithCredentials("bud", "%spencer%")
                //.WithTls()
                .WithCleanSession()
                .Build();

           
            await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken

        }

        public async Task<bool> SendInvoicePosition(PositionEntity Position)
        {
            if (mqttClient.IsConnected)
            {
                var Message = new MqttApplicationMessageBuilder()
                    .WithTopic("invoice/position")
                    .WithPayload($"ID: {Position.Id}; ItemNr: {Position.ItemNr} Price: {Position.Price} Qty: {Position.Qty}")
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();

                await mqttClient.PublishAsync(Message, CancellationToken.None); // Since 3.0.5 with CancellationToken
                return true;
            }
            return false;
        }

        public async Task<bool> SendInvoice(Invoice Invoice)
        {
            if (mqttClient.IsConnected)
            {
                int i = 0;
                var Message = new MqttApplicationMessageBuilder()
                    .WithTopic("invoice/rechnung")
                    .WithPayload($"ID: {Invoice.ID} Date: {Invoice.InvoiceDate} Amount: {Invoice.Amount} Name: {Invoice.CustomerName} Adress: {Invoice.CustomerAdress}")
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();

                await mqttClient.PublishAsync(Message, CancellationToken.None); // Since 3.0.5 with CancellationToken

                foreach (var Position in Invoice.Position)
                {
                    await SendInvoicePosition(Position);
                }
                return true;
            }
            return false;
        }
    }
}

