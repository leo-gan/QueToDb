using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using QueToDb.Dber;

namespace QueToDb.Dbs.DocumentDB
{
    public class Reader  : IReader
    {
        private DocumentClient _client;
        private DocumentCollection _collection;
        private string _collectionName;
        private Database _database;

        #region Implementation of IReader

        public  bool Initialize(params string[] configs)
        {
            var endpoint = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.Endpoint"];
            var authKey = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.AuthKey"];
            var databaseName = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.DbName"];
            _collectionName = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.CollectionName"];
            // use hard coded params if they are not set up in config file
            if (String.IsNullOrEmpty(endpoint))
                if (configs.Length >= 1)
                    endpoint = configs[0];
                else return false;
            if (String.IsNullOrEmpty(authKey))
                if (configs.Length >= 2)
                    authKey = configs[1];
                else return false;
            if (String.IsNullOrEmpty(databaseName))
                if (configs.Length >= 3)
                    databaseName = configs[2];
                else return false;
            if (String.IsNullOrEmpty(_collectionName))
                if (configs.Length >= 4)
                    _collectionName = configs[3];
                else return false;

            _client = new DocumentClient(new Uri(endpoint), authKey);

            _database = _client.CreateDatabaseQuery()
                 .Where(d => d.Id == databaseName)
                 .AsEnumerable()
                 .FirstOrDefault() ?? _client.CreateDatabaseAsync(new Database { Id = databaseName }).Result;

            _collection = _client.CreateDocumentCollectionQuery(_database.SelfLink)
                .Where(c => c.Id == _collectionName)
                .AsEnumerable()
                .FirstOrDefault() ??
                          _client.CreateDocumentCollectionAsync(_database.SelfLink,
                              new DocumentCollection { Id = _collectionName }).Result;

            return true;
        }

        public Message ReadOne(string id)
        {
            // we assume the id is for the document with Message type.
            return (Message)(dynamic)_client.ReadDocumentAsync(id).Result.Resource;
        }

        public List<Message> Read(List<string> idlList)
        {
            return idlList.Select(ReadOne).ToList();
        }

        #endregion
    }
}