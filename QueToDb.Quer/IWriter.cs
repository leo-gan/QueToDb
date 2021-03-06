﻿namespace QueToDb.Quer
{
    public interface IWriter
    {
        bool Initialize(params string[] configs); // Return: true - if initializing was successfull ; false - otherwise
        void Dispose();
        void Send(Message msg);
    }
}