using System;

namespace QueToDb.Quer
{
    [Serializable]
    public class Message
    {
        public string Type { set; get; }
        public DateTime DateTimeStamp { set; get; }
        public string[] Properties { set; get; }
        public byte[] Body { set; get; }
    }
}