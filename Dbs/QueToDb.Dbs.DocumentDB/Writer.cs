using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using QueToDb.Dber;

namespace QueToDb.Dbs.DocumentDB
{
    public class Writer : IWriter
    {
        private DocumentClient _client;
        private DocumentCollection _collection;
        private Database _database;

        #region Implementation of IWriter

        public bool Initialize(params string[] configs)
        {
            string endpoint = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.Endpoint"];
            string authKey = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.AuthKey"];
            string databaseName = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.DbName"];
            string collectionName = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.CollectionName"];
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
            if (String.IsNullOrEmpty(collectionName))
                if (configs.Length >= 4)
                    collectionName = configs[3];
                else return false;

            _client = new DocumentClient(new Uri(endpoint), authKey);


            _database = _client.CreateDatabaseQuery()
                .Where(d => d.Id == databaseName)
                .AsEnumerable()
                .FirstOrDefault() ?? _client.CreateDatabaseAsync(new Database {Id = databaseName}).Result;

            _collection = _client.CreateDocumentCollectionQuery(_database.SelfLink)
                .Where(c => c.Id == collectionName)
                .AsEnumerable()
                .FirstOrDefault() ??
                          _client.CreateDocumentCollectionAsync(_database.SelfLink,
                              new DocumentCollection {Id = collectionName}).Result;
            return true;
        }

        /// <summary>
        ///     It uses a document.SelfLink as the document Id.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string Write(Message msg)
        {
            return _client.CreateDocumentAsync(_collection.SelfLink, msg).Result.Resource.SelfLink;
        }

        public List<string> Write(List<Message> msgList)
        {
            return
                msgList.Select(msg => _client.CreateDocumentAsync(_collection.SelfLink, msg).Result.Resource.SelfLink)
                    .ToList();
        }

        #endregion
    }
}