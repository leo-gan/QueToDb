using System.Collections.Generic;

namespace QueToDb.Dber
{
    public interface IWriter
    {
        bool Initialize(params string[] configs); // Return: true - if initializing was successfull ; false - otherwise
        string Write(Message msg); // returns Id of a new record
        List<string> Write(List<Message> msgList); // returns Ids of the new records
    }
}