using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
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
            
            var Message = $"ID: {Position.Id}; ItemNr: {Position.ItemNr} Price: {Position.Price} Qty: {Position.Qty}";

            var Status = await SendInvoice(Message, "invoice/position");

            if (Status != "successful") return Status;

            return "successful";
           
        }

        public async Task<String> SendInvoice(Invoice Invoice)
        {

            var Message = $"ID: {Invoice.ID} Date: {Invoice.InvoiceDate} Amount: {Invoice.Amount} Name: {Invoice.CustomerName} Adress: {Invoice.CustomerAdress}";


            var Status = await SendInvoice(Message, "invoice/rechnung");

            if (Status != "successful") return Status;

            foreach (var Position in Invoice.Position)
            {
                String isSuccessful = await SendInvoicePosition(Position);
                if (isSuccessful != "successful") return isSuccessful;
                
            }

            return "successful";
  
        }

        public async Task<String> SendInvoiceJson(Invoice Invoice)
        {
            //JSON konvertieren
            string json = JsonConvert.SerializeObject(Invoice, Formatting.Indented,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            });
            //via MQTT senden
            var Status = await SendInvoice(json, "invoice_json/rechnung");

            if (Status != "successful") return Status;

            foreach (var Position in Invoice.Position)
            {
                String isSuccessful = await SendInvoicePositionJson(Position);
                if (isSuccessful != "successful") return isSuccessful;

            }

            return "successful";
        }

        public async Task<String> SendInvoicePositionJson(PositionEntity Position)
        {
            //JSON konvertieren
            string json = JsonConvert.SerializeObject(Position, Formatting.Indented,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            });
            //via MQTT senden
            var Status = await SendInvoice(json, "invoice_json/position");

            if (Status != "successful") return Status;

            return "successful";
        }



        private async Task<String> SendInvoice(string message, string topic)
        {
            //isConnected
            if (mqttClient.IsConnected == false) return "Connection failed";

            var Message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .Build();

           
            try
            {
                await mqttClient.PublishAsync(Message, CancellationToken.None); // Since 3.0.5 with CancellationToken
            }
            catch (Exception e)
            {

                return e.ToString();
            }
            return "successful";
        }
    }
}

