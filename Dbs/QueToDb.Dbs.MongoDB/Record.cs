using MongoDB.Bson;
using QueToDb.Dber;

namespace QueToDb.Dbs.MongoDB
{
    public class Record
    {
        public ObjectId Id { get; set; }
        public Message Message { get; set; }
    }
}