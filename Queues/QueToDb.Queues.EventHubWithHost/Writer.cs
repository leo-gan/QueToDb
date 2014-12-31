using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.EventHubWithHost
{
    public class Writer : IWriter
    {
        private EventHubClient _client;
        private string _eventHubName;
        private const string PartitionId = "0";

        public bool Initialize(params string[] configs)
        {
            try
            {
                _eventHubName = ConfigurationManager.AppSettings[
                    "QueToDb.Queues.EventHub.ServiceBus.EnentHubName"];
                _client =
                    EventHubClient.CreateFromConnectionString(
                        ConfigurationManager.AppSettings["QueToDb.Queues.EventHub.ServiceBus.SendConnectionString"], _eventHubName);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if (_client != null) _client.Close();
        }

        public void Send(Message msg)
        {
            var eventData = new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
            _client.Send(eventData);
        }
    }
}
