using System.Configuration;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.AzureTopic
{
    public class Writer : IWriter
    {
        private TopicClient _client;
        private string _topicName;

        #region IWriter Members

        public bool Initialize(params string[] configs)
        {
            try
            {
                _topicName = ConfigurationManager.AppSettings[
                    "QueToDb.Queues.AzureTopic.ServiceBus.TopicName"];
                //var namespaceManager = NamespaceManager.CreateFromConnectionString(
                //        ConfigurationManager.AppSettings["QueToDb.Queues.AzureTopic.ServiceBus.ManageConnectionString"]);
                //if (!namespaceManager.TopicExists(_topicName))
                //    namespaceManager.CreateTopic(_topicName);
                _client = TopicClient.CreateFromConnectionString(
                    ConfigurationManager.AppSettings[
                        "QueToDb.Queues.AzureTopic.ServiceBus.SendConnectionString"], _topicName);
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

        #endregion
    }
}