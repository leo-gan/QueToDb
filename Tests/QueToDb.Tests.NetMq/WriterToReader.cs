using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using QueToDb.Quer;
using QueToDb.Queues.NetMq;

namespace QueToDb.Tests.NetMq
{
    [TestFixture]
    public class WriterToReader
    {
        private readonly string _address = ConfigurationManager.AppSettings["QueToDb.Queues.NetMq.Address"];
        private readonly Writer _w = new Writer();
        private readonly Reader _r = new Reader();

        [TestFixtureSetUp]
        public void Init()
        {
            _w.Initialize(_address);
            _r.Initialize(_address);
        }

        private static Message CreateMessage()
        {
            byte[] msgBody = Encoding.UTF8.GetBytes("Test message body:" + Guid.NewGuid());
            var msg = new Message
            {
                Body = msgBody,
                Properties = new[]
                {
                    "Property1: Property1", "PropertiyGuid: " + Guid.NewGuid()
                },
                DateTimeStamp = DateTime.UtcNow,
                Type = "MyMsgType"
            };
            return msg;
        }

        [Test]
        public void ReadWrite1Msg()
        {
            var msg = CreateMessage();

            _w.Send(msg);

            var msgRead = _r.Receive();

            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body);
            Assert.AreEqual(msg.Properties, msgRead.Properties);
            Assert.AreEqual(msg.DateTimeStamp, msgRead.DateTimeStamp);
            Assert.AreEqual(msg.Type, msgRead.Type);
            Console.WriteLine(JsonConvert.SerializeObject(msgRead));
        }

        [Test]
        public void ReadWriteManyMsgInBatch()
        {
            const int max = 2000;

            var msg = CreateMessage();

            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < max; i++)
                _w.Send(msg);

            var msgRead = new Message();
            var loopCounter = 0;
            for (var i = 0; i < max - 1; i++, loopCounter++)
                msgRead = _r.Receive();

            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body);
            //Assert.AreEqual(max, loopCounter);

            Console.WriteLine("ReadWriteManyMsgInBatch(): " + JsonConvert.SerializeObject(msgRead));
            sw.Stop();
            Console.WriteLine("Messages sent/received: {0}, in {1}:  msg/sec: {2}", loopCounter, sw.Elapsed, (loopCounter * 1000) / sw.Elapsed.Milliseconds);
        }

        [Test]
        public void ReadWriteManyMsgInSequence()
        {
            const int max = 10000;

            var msg = CreateMessage();
            var sw = new Stopwatch();
            sw.Start();

            var msgRead = new Message();
            var loopCounter = 0;
            for (var i = 0; i < max; i++, loopCounter++)
            {
                _w.Send(msg);
                msgRead = _r.Receive();
            }

            Assert.AreEqual(max, loopCounter);
            Console.WriteLine("ReadWriteManyMsgInSequence(): " + JsonConvert.SerializeObject(msgRead));
            sw.Stop();
            Console.WriteLine("Messages sent/received: {0}, in {1}:  msg/sec: {2}", max, sw.Elapsed, max * 1000 / sw.Elapsed.Milliseconds);
        }
    }
}