using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MongoDB.Driver;
using QueToDb.Dber;

namespace QueToDb.Dbs.MongoDB
{
    public class Writer : IWriter
    {
        private MongoClient _client;
        private MongoCollection<Record> _collection;
        private string _connectionString;
        private MongoDatabase _database;
        private MongoServer _server;

        public bool Initialize(params string[] configs)
        {
            _connectionString =
                ConfigurationManager.AppSettings["QueToDb.Dbs.MongoDB.ConnectionString"];
            string databaseName = ConfigurationManager.AppSettings["QueToDb.Dbs.MongoDB.DbName"];
            string collectionName = ConfigurationManager.AppSettings["QueToDb.Dbs.MongoDB.CollectionName"];
            // use hard coded params if they donot set up in config file
            if (String.IsNullOrEmpty(_connectionString))
                if (configs.Length >= 1)
                    _connectionString = configs[0];
                else return false;
            if (String.IsNullOrEmpty(databaseName))
                if (configs.Length >= 2)
                    databaseName = configs[1];
                else return false;
            if (String.IsNullOrEmpty(collectionName))
                if (configs.Length >= 3)
                    collectionName = configs[2];
                else return false;

            _client = new MongoClient(_connectionString);
            _server = _client.GetServer();
            _database = _server.GetDatabase(databaseName);
            _collection = _database.GetCollection<Record>(collectionName);

            return true;
        }

        public string Write(Message msg)
        {
            var record = new Record {Message = msg};
            _collection.Insert(record); // Insert changes the Id in record
            return record.Id.ToString();
        }

        public List<string> Write(List<Message> msgList)
        {
            var records = msgList.Select(msg => new Record {Message = msg}).ToList();
            _collection.InsertBatch(records); // InsertBatch changes the Ids in records

            return records.Select(rec => rec.Id.ToString()).ToList();
        }
    }
}