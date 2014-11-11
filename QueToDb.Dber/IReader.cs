using System.Collections.Generic;

namespace QueToDb.Dber
{
    public interface IReader
    {
        bool Initialize(params string[] configs); // Return: true - if initializing was successfull ; false - otherwise
        Message ReadOne(string id);
        List<Message> Read(List<string> idlList);
    }
}