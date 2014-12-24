using System;
using System.Configuration;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.EventHub
{
    public class Reader : IReader
    {
        private const string PartitionId = "0";
        private EventHubClient _client;
        private string _eventHubName;
        private EventHubConsumerGroup _group;
        private string _offset;
        private EventHubReceiver _receiver;

        public bool Initialize(params string[] configs)
        {
            try
            {
                _eventHubName = ConfigurationManager.AppSettings[
                    "QueToDb.Queues.EventHub.ServiceBus.EnentHubName"];
                _client =
                    EventHubClient.CreateFromConnectionString(
                        ConfigurationManager.AppSettings[
                            "QueToDb.Queues.EventHub.ServiceBus.ListenConnectionString"],
                        _eventHubName);
                _group = _client.GetDefaultConsumerGroup();
                _offset = OffsetProvider.Offset.ToString();
                _receiver = _group.CreateReceiver(PartitionId, _offset);
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

        public Message Receive()
        {
            _offset = OffsetProvider.Offset.ToString();
            EventData message = _receiver.Receive(new TimeSpan(0, 0, 1));
            _offset = message.Offset;
            OffsetProvider.Offset = Convert.ToInt64(_offset);
            return
                JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(message.GetBytes()));
        }
    }
}