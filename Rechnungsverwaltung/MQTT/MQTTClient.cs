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

        public async void Init(string Client, string TcpServer)
        {
            mqttClient = factory.CreateMqttClient();
            // Create TCP based options using the builder.
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Client)
                .WithTcpServer(TcpServer)
                .WithCleanSession()
                .Build();
            
                await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
            
            

        }

        public async Task<String> SendInvoicePosition(PositionEntity Position)
        {
            //if (mqttClient.IsConnected==false) return "Connection failed";
            
            var Message = new MqttApplicationMessageBuilder()
                .WithTopic("invoice/position")
                .WithPayload($"ID: {Position.Id}; ItemNr: {Position.ItemNr} Price: {Position.Price} Qty: {Position.Qty}")
                .Build();
            try
            {
                await mqttClient.PublishAsync(Message, CancellationToken.None);
            }
            catch(Exception e)
            {
                return e.ToString();
            }
            return "successful";
           
        }

        public async Task<String> SendInvoice(Invoice Invoice)
        {
            //if (mqttClient.IsConnected==false) return "Connection failed";
            
            var Message = new MqttApplicationMessageBuilder()
                .WithTopic("invoice/rechnung")
                .WithPayload($"ID: {Invoice.ID} Date: {Invoice.InvoiceDate} Amount: {Invoice.Amount} Name: {Invoice.CustomerName} Adress: {Invoice.CustomerAdress}")
                .Build();

            try
            {
                await mqttClient.PublishAsync(Message, CancellationToken.None); // Since 3.0.5 with CancellationToken
            }
            catch(Exception e)
            {

                return e.ToString();
            }
            
            foreach (var Position in Invoice.Position)
            {
                String isSuccessful = await SendInvoicePosition(Position);
                if (isSuccessful != "successful") return isSuccessful;
                
            }
            
            return "successful";
            
  
        }
    }
}

