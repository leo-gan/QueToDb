using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using QueToDb.Dber;

namespace QueToDb.Dbs.MongoDB
{
    public class Reader : IReader
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
            // use hard coded params if they are not set up in config file
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

        public Message ReadOne(string id)
        {
            return _collection.FindOneById(new ObjectId(id)).Message;
        }

        public List<Message> Read(List<string> idlList)
        {
            List<ObjectId> objectIdList = idlList.Select(id => new ObjectId(id)).ToList();
            IMongoQuery query = Query<Record>.In(b => b.Id, objectIdList);
            return _collection.FindAs<Record>(query).Select(record => record.Message).ToList();
        }

        /// <summary>
        ///     This method is specific for MongoDb.
        /// </summary>
        /// <param name="query">Example: var query = Query<Book>.EQ(b => b.Author, "Kurt Vonnegut"); </param>
        /// <returns></returns>
        public List<Message> Read(QueryDocument query)
        {
            return _collection.FindAs<Record>(query).Select(record => record.Message).ToList();
        }
    }
}