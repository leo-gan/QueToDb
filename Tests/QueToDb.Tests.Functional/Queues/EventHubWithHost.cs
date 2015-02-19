using System;
using System.Text;
using System.Threading;
using NUnit.Framework;
using QueToDb.Quer;
using QueToDb.Queues.EventHubWithHost;

namespace QueToDb.Tests.Functional.Queues
{
    [TestFixture]
    internal class EventHubWithHost
    {
        public EventHubWithHost()
        {
            bool isInitialized = _r.Initialize();
            Assert.IsTrue(isInitialized);
            isInitialized = _w.Initialize();
            Assert.IsTrue(isInitialized);
        }

        ~EventHubWithHost()
        {
            _w.Dispose();
            _r.Dispose();
        }


        //[SetUp]
        //public void Init()
        //{
        //    bool isInitialized = _r.Initialize();
        //    Assert.IsTrue(isInitialized);
        //    isInitialized = _w.Initialize();
        //    Assert.IsTrue(isInitialized);
        //}

        //[TearDown]
        //public void Dispose()
        //{
        //    _w.Dispose();
        //    _r.Dispose();
        //}

        private const string Transport = "EventHubWithHost";
        private readonly Writer _w = new Writer();
        private readonly Reader _r = new Reader();

        [Test]
        [TestCase(10, '=')]
        public void SendReadAsWait_1Msg(int msgBodySizeChars, char msgBodyFiller)
        {
            Thread.Sleep(5000);
            Message msg = WriteAndRead.CreateMessage(msgBodySizeChars, msgBodyFiller);
            msg.Type = "SendReadAsWait_1Msg";
            _w.Send(msg);
            Console.WriteLine("Message Sent.  Message.Type: '{0}', msg.Body: '{1}'", msg.Type,
                Encoding.UTF8.GetString(msg.Body));
            Thread.Sleep(5000);

        }
    }
}