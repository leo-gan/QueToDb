using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using QueToDb.Quer;

namespace QueToDb.Tests.Functional.Queues
{
    public class WriteAndRead
    {
        private static Message CreateMessage(int msgBodySizeChars, char msgBodyFiller)
        {
            string msgBodyBeginning = Guid.NewGuid() + " : Test message body: ";
            string msgBodyString = msgBodyBeginning + (msgBodyBeginning.Length < msgBodySizeChars
                ? new string(msgBodyFiller, msgBodySizeChars - msgBodyBeginning.Length)
                : null);
            byte[] msgBody = Encoding.UTF8.GetBytes(msgBodyString);
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

        public static void ReadWrite1Msg(IWriter w, IReader r, string transport, int msgBodySizeChars,
            char msgBodyFiller)
        {
            Message msg = CreateMessage(msgBodySizeChars, msgBodyFiller);

            w.Send(msg);

            Message msgRead = r.Receive();

            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body);
            Assert.AreEqual(msg.Properties, msgRead.Properties);
            Assert.AreEqual(msg.DateTimeStamp, msgRead.DateTimeStamp);
            Assert.AreEqual(msg.Type, msgRead.Type);
            Console.WriteLine(JsonConvert.SerializeObject(msgRead));
        }

        public static void ReadWriteManyMsgInBatch(IWriter w, IReader r, string transport, int msgBodySizeChars,
            char msgBodyFiller, int msgNumber)
        {
            Message msg = CreateMessage(msgBodySizeChars, msgBodyFiller);

            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < msgNumber; i++)
                w.Send(msg);

            var msgRead = new Message();
            int loopCounter = 0;
            for (int i = 0; i < msgNumber; i++, loopCounter++)
                msgRead = r.Receive();
            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body); // only last msg

            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body);

            //Console.WriteLine("ReadWriteManyMsgInBatch(): " + JsonConvert.SerializeObject(msgRead));
            sw.Stop();
            Console.WriteLine("{3}: In Batch: {0} msg * {1} byte :  {2} msg/sec", msgNumber, msgBodySizeChars,
                (msgNumber*1000)/sw.Elapsed.Milliseconds, transport);
        }

        public static void ReadWriteManyMsgInSequence(IWriter w, IReader r, string transport, int msgBodySizeChars,
            char msgBodyFiller, int msgNumber)
        {
            Message msg = CreateMessage(msgBodySizeChars, msgBodyFiller);
            var sw = new Stopwatch();
            sw.Start();

            var msgRead = new Message();
            int loopCounter = 0;
            for (int i = 0; i < msgNumber; i++, loopCounter++)
            {
                w.Send(msg);
                msgRead = r.Receive();
            }
            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body); // only last msg

            Assert.AreEqual(msgNumber, loopCounter);
            //Console.WriteLine("ReadWriteManyMsgInSequence(): " + JsonConvert.SerializeObject(msgRead));
            sw.Stop();
            Console.WriteLine("{3}: In Sequence: {0} msg * {1} byte :  {2} msg/sec", msgNumber, msgBodySizeChars,
                (msgNumber*1000)/sw.Elapsed.Milliseconds, transport);
        }
    }
}