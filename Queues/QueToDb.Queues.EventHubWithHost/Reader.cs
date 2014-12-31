using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.EventHubWithHost
{
    public class Reader : IReader
    {
        private const string PartitionId = "0";
        private EventHubClient _client;
        private string _eventHubName;
        private string _eventHubConnectionString;
        private string _blobConnectionString;
        private EventHubConsumerGroup _group;
        //private EventHubReceiver _receiver;
        private Microsoft.ServiceBus.Messaging.EventProcessorHost _eventProcessorHost;

        public bool Initialize(params string[] configs)
        {
            try
            {
               _blobConnectionString =
      ConfigurationManager.AppSettings["QueToDb.Queues.EventHub.ServiceBus.AzureStorageConnectionString"];
                 _eventHubName = ConfigurationManager.AppSettings[
                     "QueToDb.Queues.EventHub.ServiceBus.EnentHubName"];
                 _eventHubConnectionString = ConfigurationManager.AppSettings[
                      "QueToDb.Queues.EventHub.ServiceBus.ListenConnectionString"];

                _client =
                    EventHubClient.CreateFromConnectionString(_eventHubConnectionString, _eventHubName);
                _group = _client.GetDefaultConsumerGroup();
                _eventProcessorHost = new EventProcessorHost("eventhuboffset", _client.Path,
                    _group.GroupName, _eventHubConnectionString, _blobConnectionString);

                _eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>().Wait();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if (_eventProcessorHost != null) _eventProcessorHost.UnregisterEventProcessorAsync().Wait();
            if (_group != null) _group.Close();
            if (_client != null) _client.Close();
        }

        public Message Receive()
        {
            throw new NotImplementedException();
        }

        //public Message Receive()
        //{
        //    //_offset = OffsetProvider.Offset.ToString();
        //    EventData message = _receiver.Receive(new TimeSpan(0, 0, 1));
        //    //_offset = message.Offset;
        //    //OffsetProvider.Offset = Convert.ToInt64(_offset);
        //    return
        //        JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(message.GetBytes()));
        //}
    }
}
