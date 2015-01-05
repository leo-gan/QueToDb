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
        public static Message CreateMessage(int msgBodySizeChars, char msgBodyFiller)
        {
            string msgBodyBeginning = Guid.NewGuid() + " - Test message body";
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
            Console.WriteLine("ReadWrite1Msg: " + JsonConvert.SerializeObject(msgRead));
            Console.WriteLine("ReadWrite1Msg: msgRead.Body: " + Encoding.Default.GetString(msgRead.Body));
        }

        public static void ReadWriteManyMsgInBatch(IWriter w, IReader r, string transport, int msgBodySizeChars,
            char msgBodyFiller, int msgNumber)
        {
            Message msg = CreateMessage(msgBodySizeChars, msgBodyFiller);

            var sw = new Stopwatch();
            sw.Start();
            //Console.WriteLine("ReadWriteManyMsgInBatch: Started for msgNumber: " + msgNumber);

            for (int i = 0; i < msgNumber; i++)
            {
                w.Send(msg);
                //Console.WriteLine("\tReadWriteManyMsgInBatch: send " + i);
            }

            var msgRead = new Message();
            int loopCounter = 0;
            for (int i = 0; i < msgNumber; i++, loopCounter++)
            {
                msgRead = r.Receive();
                //Console.WriteLine("\tReadWriteManyMsgInBatch: received " + i);
            }
            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body); // only last msg

            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body);

            //Console.WriteLine("ReadWriteManyMsgInBatch(): " + JsonConvert.SerializeObject(msgRead));
            //sw.Stop();
            //Console.WriteLine("ReadWriteManyMsgInBatch: Finished for msgNumber: " + msgNumber);
            Console.WriteLine("{3}: In Batch: {0} msg * {1} byte :  {2} msg/sec", msgNumber, msgBodySizeChars,
                (msgNumber*1000)/sw.Elapsed.Milliseconds, transport);
        }

        public static void ReadWriteManyMsgInSequence(IWriter w, IReader r, string transport, int msgBodySizeChars,
            char msgBodyFiller, int msgNumber)
        {
            Message msg = CreateMessage(msgBodySizeChars, msgBodyFiller);
            var sw = new Stopwatch();
            sw.Start();
            //Console.WriteLine("ReadWriteManyMsgInSequence: Started for msgNumber: " + msgNumber);

            var msgRead = new Message();
            int loopCounter = 0;
            for (int i = 0; i < msgNumber; i++, loopCounter++)
            {
                w.Send(msg);
                msgRead = r.Receive();
                //Console.WriteLine("\tReadWriteManyMsgInSequence: send and receive " + i);
            }
            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body); // only last msg

            Assert.AreEqual(msgNumber, loopCounter);

            //sw.Stop();
            //Console.WriteLine("ReadWriteManyMsgInSequence: Finished for msgNumber: " + msgNumber);
            Console.WriteLine("{3}: In Sequence: {0} msg * {1} byte :  {2} msg/sec", msgNumber, msgBodySizeChars,
                (msgNumber*1000)/sw.Elapsed.Milliseconds, transport);
        }
    }
}