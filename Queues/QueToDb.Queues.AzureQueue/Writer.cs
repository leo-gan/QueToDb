using System.Configuration;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.AzureQueue
{
    public class Writer : IWriter
    {
        private string _queueName;
        private QueueClient _client;

        public bool Initialize(params string[] configs)
        {
            try
            {
                _queueName = ConfigurationManager.AppSettings[
                    "QueToDb.Queues.AzureQueue.ServiceBus.QueueName"];
                //var namespaceManager = NamespaceManager.CreateFromConnectionString(
                //        ConfigurationManager.AppSettings["QueToDb.Queues.AzureQueue.ServiceBus.ManageConnectionString"]);
                //if (!namespaceManager.QueueExists(_queueName))
                //    namespaceManager.CreateQueue(_queueName);
                _client = QueueClient.CreateFromConnectionString(
                        ConfigurationManager.AppSettings["QueToDb.Queues.AzureQueue.ServiceBus.SendConnectionString"], _queueName);
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
            var brokeredMsg = new BrokeredMessage(JsonConvert.SerializeObject(msg));
            _client.Send(brokeredMsg);
        }
    }
}