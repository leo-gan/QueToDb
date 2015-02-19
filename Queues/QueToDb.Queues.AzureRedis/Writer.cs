using System;
using System.Configuration;
using System.Diagnostics;
using Newtonsoft.Json;
using QueToDb.Quer;
using StackExchange.Redis;

namespace QueToDb.Queues.AzureRedis
{
    public class Writer : IWriter
    {
        private readonly string _address =
            ConfigurationManager.AppSettings["QueToDb.Queues.AzureRedis.ConnectionString"];

        private readonly string _queueName =
            ConfigurationManager.AppSettings["QueToDb.Queues.AzureRedis.QueueName"];

        private IDatabase _db;
        private ConnectionMultiplexer _redis;

        #region IWriter Members

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

        public void Send(Message msg)
        {
            Send<Message>(msg);
        }

        #endregion

        public void Send<T>(T msg)
        {
            var redisMsg = (RedisValue) JsonConvert.SerializeObject(msg);
            _db.ListLeftPush(_queueName, redisMsg);
        }
    }
}