using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using QueToDb.Dber;
using QueToDb.Dbs.MongoDB;

namespace QueToDb.Tests.MongoDb
{
    [TestFixture]
    public class WriterToReader
    {
        private readonly Writer _w = new Writer();
        private readonly Reader _r = new Reader();

        [SetUp]
        public void Init()
        {
            _w.Initialize();
            _r.Initialize();
        }

        private static Message CreateMessage()
        {
            var msgBody = Encoding.UTF8.GetBytes("Test message body:" + Guid.NewGuid());
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

            var id = _w.Write(msg);

            var msgRead = _r.ReadOne(id);

            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body);
            Assert.AreEqual(msg.Properties, msgRead.Properties);
            Assert.AreEqual(msg.DateTimeStamp.ToString(), msgRead.DateTimeStamp.ToString());
            Assert.AreEqual(msg.Type, msgRead.Type);
        }

        [Test]
        public void ReadWriteManyMsgInBatch()
        {
            const int max = 2000;

            var sw = new Stopwatch();
            sw.Start();

            var msgWrittenList = new List<Message>();
            for (var i = 0; i < max; i++)
                msgWrittenList.Add(CreateMessage());

            var idList = _w.Write(msgWrittenList);

            var msgReadList = _r.Read(idList);
            Assert.NotNull(msgReadList);
            Assert.AreEqual(msgWrittenList.Count, msgReadList.Count);

            sw.Stop();
            Console.WriteLine("Messages written/read In Batch: {0}, in {1}:  msg/sec: {2}", max, sw.Elapsed, (max * 1000) / sw.Elapsed.Milliseconds);

            Assert.IsTrue( CompareLists(msgWrittenList, msgReadList) );
         }

        private static bool CompareLists(IEnumerable<Message> msgWrittenList, List<Message> msgReadList)
        {
            foreach (var wMsg in msgWrittenList)
            {
                var foundEqual = msgReadList.Any(rMsg => wMsg.Body.ToString() == rMsg.Body.ToString());
                if (!foundEqual)
                    return false;
            }
            return true;
        }

        [Test]
        public void ReadWriteManyMsgInSequence()
        {
            const int max = 2000;

            var msgWrittenList = new List<Message>();
            for (var i = 0; i < max; i++)
                msgWrittenList.Add(CreateMessage());

            var sw = new Stopwatch();
            sw.Start();

            var idList = msgWrittenList.Select(msg => _w.Write(msg)).ToList();

            var msgReadList = idList.Select(id => _r.ReadOne(id)).ToList();

            sw.Stop();
            Console.WriteLine("Messages sent/received In Sequence: {0}, in {1}:  msg/sec: {2}", max, sw.Elapsed, max * 1000 / sw.Elapsed.Milliseconds);

            Assert.IsTrue(CompareLists(msgWrittenList, msgReadList));

        }
    }
}
