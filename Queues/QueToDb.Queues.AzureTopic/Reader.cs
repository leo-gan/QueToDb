using System;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.AzureTopic
{
    public class Reader : IReader
    {
        private SubscriptionClient _client;
        private string _topicName;

        #region IReader Members

        public bool Initialize(params string[] configs)
        {
            try
            {
                _topicName = ConfigurationManager.AppSettings[
                    "QueToDb.Queues.AzureTopic.ServiceBus.TopicName"];
                string subscriptionName = ConfigurationManager.AppSettings[
                    "QueToDb.Queues.AzureTopic.ServiceBus.SubscriptionName"];
                _client = SubscriptionClient.CreateFromConnectionString(
                    ConfigurationManager.AppSettings[
                        "QueToDb.Queues.AzureTopic.ServiceBus.ListenConnectionString"], _topicName,
                    subscriptionName, ReceiveMode.ReceiveAndDelete);
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
            Message msg = null;
            BrokeredMessage brokeredMsg = null;

            //try
            //{
                brokeredMsg = _client.Receive(new TimeSpan(0, 0, 1));
                if (brokeredMsg == null) return null;
                msg = JsonConvert.DeserializeObject<Message>(brokeredMsg.GetBody<string>());
                // brokeredMsg.Complete(); // it is for the ReceiveMode.ReekLock mode
            //}
            //catch (Exception ex)
            //{
            //    if (brokeredMsg != null)
            //        brokeredMsg.Abandon(); // it is for the ReceiveMode.ReekLock mode
            //    throw;
            //}
            return msg;
        }

        #endregion

        public void CleanUp()
        {
            try
            {
                int i = 0;
                while (true)
                {
                    Message brokeredMsg = Receive();
                    if (brokeredMsg == null) break;
                    i++;
                    Console.WriteLine("QueToDb.Queues.AzureTopic.Reader.CleanUp(): cleaned i: " + i);
                }
                Console.WriteLine("QueToDb.Queues.AzureTopic.Reader.CleanUp(): Finished: all " + i);
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine("QueToDb.Queues.AzureTopic.Reader.CleanUp(): TimeoutException" +
                                  ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("QueToDb.Queues.AzureTopic.Reader.CleanUp(): Exception" +
                                  ex.Message);
            }
        }
    }
}