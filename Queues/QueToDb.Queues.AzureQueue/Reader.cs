using System;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.AzureQueue
{
    public class Reader : IReader
    {
        private QueueClient _client;
        private string _queueName;

        public bool Initialize(params string[] configs)
        {
            try
            {
                _queueName = ConfigurationManager.AppSettings[
                    "QueToDb.Queues.AzureQueue.ServiceBus.QueueName"];
                _client = QueueClient.CreateFromConnectionString(
                    ConfigurationManager.AppSettings[
                        "QueToDb.Queues.AzureQueue.ServiceBus.ListenConnectionString"], _queueName);
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

            try
            {
                brokeredMsg = _client.Receive(new TimeSpan(0,0,1));
                if (brokeredMsg == null) return null;
                msg = JsonConvert.DeserializeObject<Message>(brokeredMsg.GetBody<string>());
                brokeredMsg.Complete();
            }
            catch (Exception ex)
            {
                if (brokeredMsg != null)
                    brokeredMsg.Abandon();
                throw;
            }
            return msg;
        }

        public void CleanUp()
        {

            try
            {
                int i = 0;
                while (true)
                {
                    var  brokeredMsg = Receive();
                    if (brokeredMsg == null) break;
                    i++;
                    Console.WriteLine("QueToDb.Queues.AzureQueue.Reader.CleanUp(): cleaned i: " + i);
                }
                Console.WriteLine("QueToDb.Queues.AzureQueue.Reader.CleanUp(): Finished: all " + i);
            }
            catch (System.TimeoutException ex)
            {
                Console.WriteLine("QueToDb.Queues.AzureQueue.Reader.CleanUp(): TimeoutException" + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("QueToDb.Queues.AzureQueue.Reader.CleanUp(): Exception" + ex.Message);
            }

        }
    }
}