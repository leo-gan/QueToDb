using System;

namespace QueToDb.Dber
{
    public class Message
    {
        public string Type { set; get; }
        public DateTime DateTimeStamp { set; get; }
        public string[] Properties { set; get; }
        public byte[] Body { set; get; }
    }
}