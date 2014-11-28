using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using QueToDb.Quer;
using StackExchange.Redis;

namespace QueToDb.Queues.Redis
{
    public class Reader : IReader
    {
        private readonly string _address = ConfigurationManager.AppSettings["QueToDb.Queues.Redis.Address"];
        private readonly string _queueName = ConfigurationManager.AppSettings["QueToDb.Queues.Redis.QueueName"];

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
            var byteArray = (byte[]) _db.ListRightPop(_queueName, CommandFlags.None);
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(byteArray));
        }
    }
}