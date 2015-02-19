using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using QueToDb.Quer;
using StackExchange.Redis;

namespace QueToDb.Queues.AzureRedis
{
    public class Reader : IReader
    {
        private readonly string _address =
            ConfigurationManager.AppSettings["QueToDb.Queues.AzureRedis.ConnectionString"];

        private readonly string _queueName =
            ConfigurationManager.AppSettings["QueToDb.Queues.AzureRedis.QueueName"];

        private IDatabase _db;
        private ConnectionMultiplexer _redis;

        #region IReader Members

        public bool Initialize(params string[] configs)
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(_address);
                _db = _redis.GetDatabase();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return false;
            }
        }

        public void Dispose()
        {
            if (_redis != null) _redis.Dispose();
        }

        public Message Receive()
        {
            return Receive<Message>();
        }

        #endregion

        public T Receive<T>()
        {
            var byteArray = (byte[]) _db.ListRightPop(_queueName);
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(byteArray));
        }

        public void CleanUpAllMessages()
        {
            _db.ListTrim(_queueName, 0, 1); // the (.., 0, 0) doesn't work for some unknown reason.
            _db.ListRightPop(_queueName);
        }
    }
}