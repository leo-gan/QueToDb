using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using QueToDb.Quer;

namespace QueToDb.Queues.NetMq
{
    public class Writer : IWriter
    {
        private string _address = System.Configuration.ConfigurationManager.AppSettings["QueToDb.Queues.NetMq.Address"];
        private NetMQContext _ctx;
        private PublisherSocket _sock;

        public bool Initialize(params string[] configs)
        {
            try
            {
                _address = configs[0];
                _ctx = NetMQContext.Create();
                _sock = _ctx.CreatePublisherSocket();

                _sock.Bind(_address);
                Thread.Sleep(1000);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return false;
            }
        }

        public void Send(Message msg)
        {
            Send<Message>(msg);
        }

        public void Send(Stream stream)
        {
            Send(stream, StreamAsByteArray);
        }

        public void Send<T>(T msg)
        {
            Send(msg, TypeAsByteArray);
        }


        private void Send<T>(T msg, Func<T, byte[]> byteArrayFunc)
        {
            _sock.Send(byteArrayFunc(msg));
        }

        private static byte[] StreamAsByteArray(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static byte[] TypeAsByteArray<T>(T msg)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
        }
    }
}