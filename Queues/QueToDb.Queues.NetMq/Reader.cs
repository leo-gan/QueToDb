using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.NetMq
{
    public class Reader : IReader
    {
        private string _address = System.Configuration.ConfigurationManager.AppSettings["QueToDb.Queues.NetMq.Address"];
        private NetMQContext _ctx;
        private SubscriberSocket _sock;

        public bool Initialize(params string[] configs)
        {
            try
            {
                _address = configs[0];
                _ctx = NetMQContext.Create();
                _sock = _ctx.CreateSubscriberSocket();
                _sock.Subscribe(""); // subscribe for all topics

                _sock.Connect(_address);
                Thread.Sleep(1000);
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
            if (_sock != null) _sock.Dispose();
            if (_ctx != null) _ctx.Dispose();
        }

        public Message Receive()
        {
            return Receive<Message>();
        }

        public T Receive<T>()
        {
            var byteArray = _sock.Receive();
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(byteArray));
        }

    }
}