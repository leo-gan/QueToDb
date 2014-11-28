namespace QueToDb.Quer
{
    public interface IReader
    {
        bool Initialize(params string[] configs); // Return: true - if initializing was successfull ; false - otherwise
        void Dispose();
        Message Receive();
    }
}