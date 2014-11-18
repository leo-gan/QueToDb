using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using QueToDb.Dber;
using QueToDb.Dbs.DocumentDB;

namespace QueToDb.Tests.DocumentDB
{
    [TestFixture]
    public class WriterToReader
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
             _w.Initialize();
             _r.Initialize();
        }

        #endregion

        private readonly Writer _w = new Writer();
        private readonly Reader _r = new Reader();

        private static Message CreateMessage()
        {
            var msgBody = Encoding.UTF8.GetBytes("Test message body:" + Guid.NewGuid());
            var msg = new Message
            {
                Body = msgBody,
                Properties = new[]
                {
                    "Property1: Property1", "PropertiyGuid: " + Guid.NewGuid()
                },
                DateTimeStamp = DateTime.UtcNow,
                Type = "MyMsgType"
            };
            return msg;
        }

        private static bool CompareLists(IEnumerable<Message> msgWrittenList, IEnumerable<Message> msgReadList)
        {
            return msgWrittenList.Select(wMsg => msgReadList.Any(rMsg => wMsg.Body.ToString() == rMsg.Body.ToString())).All(foundEqual => foundEqual);
        }

        [Test]
        public void Sample()
        {
            var endpoint = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.Endpoint"];
            var authKey = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.AuthKey"];
            var databaseName = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.DbName"];
            var collectionName = ConfigurationManager.AppSettings["QueToDb.Dbs.DocumentDB.CollectionName"];

            using (var client = new DocumentClient(new Uri(endpoint), authKey))
            {
                 var database = client.CreateDatabaseQuery()
                     .Where(d => d.Id == databaseName)
                     .AsEnumerable()
                     .FirstOrDefault() ?? client.CreateDatabaseAsync(new Database { Id = databaseName }).Result;

                 var collection = client.CreateDocumentCollectionQuery(database.SelfLink)
                     .Where(c => c.Id == collectionName)
                     .AsEnumerable()
                     .FirstOrDefault() ??
                                  client.CreateDocumentCollectionAsync(database.SelfLink, new DocumentCollection { Id = collectionName }).Result;

                //DocumentDB supports strongly typed POCO objects and also dynamic objects
                dynamic andersonFamily = JsonConvert.DeserializeObject(
                    @"
{
    'id': 'AndersenFamily',
    'lastName': 'Andersen',
    'parents': [
        { 'firstName': 'Thomas' },
        { 'firstName': 'Mary Kay' }
    ],
    'children': [
        { 'firstName': 'John', 'gender': 'male', 'grade': 7 }
    ],
    'pets': [
        { 'givenName': 'Fluffy' }
    ],
    'address': { 'country': 'USA', 'state': 'WA', 'city': 'Seattle' }
}"
                    );
                dynamic wakefieldFamily = JsonConvert.DeserializeObject(
                    @"{
    'id': 'WakefieldFamily',
    'parents': [
        { 'familyName': 'Wakefield', 'givenName': 'Robin' },
        { 'familyName': 'Miller', 'givenName': 'Ben' }
    ],
    'children': [
        {
            'familyName': 'Wakefield',
            'givenName': 'Jesse',
            'gender': 'female',
            'grade': 1
        },
        {
            'familyName': 'Miller',
            'givenName': 'Lisa',
            'gender': 'female',
            'grade': 8
        }
    ],
    'pets': [
        { 'givenName': 'Goofy' },
        { 'givenName': 'Shadow' }
    ],
    'address': { 'country': 'USA', 'state': 'NY', 'county': 'Manhattan', 'city': 'NY' }
}"
                    );

                //persist the documents in DocumentDB
                 client.CreateDocumentAsync(collection.SelfLink, andersonFamily);
                 client.CreateDocumentAsync(collection.SelfLink, wakefieldFamily);

                //very simple query returning the full JSON document matching a simple WHERE clause
                var query = client.CreateDocumentQuery(collection.SelfLink, "SELECT * FROM Families f WHERE f.id = 'AndersenFamily'");
                var family = query.AsEnumerable().FirstOrDefault();

                Console.WriteLine("The Anderson family have the following pets:");
                foreach (var pet in family.pets)
                {
                    Console.WriteLine(pet.givenName);
                }

                //select JUST the child record out of the Family record where the child's gender is male
                query = client.CreateDocumentQuery(collection.DocumentsLink, "SELECT * FROM c IN Families.children WHERE c.gender='male'");
                var child = query.AsEnumerable().FirstOrDefault();

                Console.WriteLine("The Andersons have a son named {0} in grade {1} ", child.firstName, child.grade);

                //cleanup test database
                // client.DeleteDatabaseAsync(database.SelfLink);
            }
        }

        [Test]
        public void ReadWrite1Msg()
        {
            var msg = CreateMessage();

            var id = _w.Write(msg);

            var msgRead = _r.ReadOne(id);

            Assert.NotNull(msgRead);
            Assert.AreEqual(msg.Body, msgRead.Body);
            Assert.AreEqual(msg.Properties, msgRead.Properties);
            Assert.AreEqual(msg.DateTimeStamp.ToString(), msgRead.DateTimeStamp.ToString());
            Assert.AreEqual(msg.Type, msgRead.Type);
        }

        [Test]
        public void ReadWriteManyMsgInBatch()
        {
            const int max = 200;

            var sw = new Stopwatch();
            sw.Start();

            var msgWrittenList = new List<Message>();
            for (var i = 0; i < max; i++)
                msgWrittenList.Add(CreateMessage());

            var idList = _w.Write(msgWrittenList);

            var msgReadList = _r.Read(idList);
            Assert.NotNull(msgReadList);
            Assert.AreEqual(msgWrittenList.Count, msgReadList.Count);

            sw.Stop();
            Console.WriteLine("Messages written/read In Batch: {0}, in {1}:  msg/sec: {2}", max, sw.Elapsed,
                (max*1000)/sw.Elapsed.Milliseconds);

            Assert.IsTrue(CompareLists(msgWrittenList, msgReadList));
        }

        [Test]
        public void ReadWriteManyMsgInSequence()
        {
            const int max = 200;

            var msgWrittenList = new List<Message>();
            for (var i = 0; i < max; i++)
                msgWrittenList.Add(CreateMessage());

            var sw = new Stopwatch();
            sw.Start();

            var idList = msgWrittenList.Select(msg => _w.Write(msg)).ToList();

            var msgReadList = idList.Select(id => _r.ReadOne(id)).ToList();

            sw.Stop();
            Console.WriteLine("Messages sent/received In Sequence: {0}, in {1}:  msg/sec: {2}", max, sw.Elapsed,
                max*1000/sw.Elapsed.Milliseconds);

            Assert.IsTrue(CompareLists(msgWrittenList, msgReadList));
        }
    }
}