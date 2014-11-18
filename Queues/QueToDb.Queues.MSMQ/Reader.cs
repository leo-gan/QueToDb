using System;
using System.Configuration;
using System.Messaging;
using QueToDb.Quer;
using Message = QueToDb.Quer.Message;

namespace QueToDb.Queues.MSMQ
{
    public class Reader : IReader
    {
        private readonly string _address = ConfigurationManager.AppSettings["QueToDb.Queues.MSMQ.Address"];
        private MessageQueue _q;

        #region IReader Members

        public bool Initialize(params string[] configs)
        {
            // a nonexisted queue created only in Writer!
            _q = new MessageQueue(_address) {Formatter = new BinaryMessageFormatter()};
            return true;
        }

        public Message Receive()
        {
            try
            {
                _q.Formatter = new BinaryMessageFormatter();
                System.Messaging.Message receive = _q.Receive(new TimeSpan(0, 0, 0, 0, 100));
                return receive != null ? (Message) receive.Body : null;
            }
            catch
            {
                return null; // if message is not delivered yet}
            }
        }

        #endregion
    }
}