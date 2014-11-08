using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace QueToDb.Quer
{
    public interface IReader
    {
        bool Initialize(params string[] configs); // Return: true - if initializing was successfull ; false - otherwise
        Message Receive();
    }
}
