using MongoDB.Bson;
using System;

namespace IOWebApplication.Infrastructure.Models.Cdn
{
    public class MongoItemVM
    {
        public ObjectId Id { get; set; }
        public string Filename { get; set; }
        public BsonDocument MetaData { get; set; }
        public string Length { get; set; }
        public DateTime UploadDateTime { get; set; }
    }
}
