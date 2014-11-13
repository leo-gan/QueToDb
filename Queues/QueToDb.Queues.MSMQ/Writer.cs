using System.Configuration;
using System.Messaging;
using Newtonsoft.Json;
using QueToDb.Quer;
using Message = QueToDb.Quer.Message;

namespace QueToDb.Queues.MSMQ
{
    public class Writer : IWriter
    {
        private readonly string _address = ConfigurationManager.AppSettings["QueToDb.Queues.MSMQ.Address"];
        private MessageQueue _q;

        public bool Initialize(params string[] configs)
        {
            // if queue is not existeed, create it
            try
            {
                if (!MessageQueue.Exists(_address))
                    MessageQueue.Create(_address);
                _q = new MessageQueue(_address) {Formatter = new BinaryMessageFormatter()};
            }
            catch
            {
                return false;
            }

            return true;
        }


        public void Send(Message msg)
        {
            var msgString = JsonConvert.SerializeObject(msg);
            _q.Send(msgString);
        }
    }
}