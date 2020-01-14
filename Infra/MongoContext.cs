using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dicloud_api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace dicloud_api.Infra
{
    public class MongoContext
    {
        private MongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<ImageModel> Collection { get { return _database.GetCollection<ImageModel>("studies"); } }

        public MongoContext()
        {
            //var client = new MongoClient("mongodb+srv://<username>:<password>@<cluster-address>/test?w=majority");
            _client = new MongoClient("mongodb://localhost:27017");
            _database = _client.GetDatabase("dicloud");
        }

        public async Task InsertStudy(ImageModel model)
        {
            var collection = _database.GetCollection<ImageModel>("studies");
            await collection.InsertOneAsync(model);
        }

        public async Task<IEnumerable<ImageModel>> GetStudiesByPatientName(string name)
        {
            //var collection = _database.GetCollection<ImageModel>("studies");
            //var filter = Builders<ImageModel>.Filter.Eq("Tags.PatientName", name);
            var filter = Builders<ImageModel>.Filter.Regex("Tags.PatientName", new BsonRegularExpression(name.Replace("*", string.Empty)));
            var documents = await Collection.Find(filter).ToListAsync();
            return documents;
        }

        public async Task<IEnumerable<ImageModel>> GetStudiesByPatientId(string patientId)
        {
            var filter = Builders<ImageModel>.Filter.Eq("Tags.PatientID", patientId);
            var documents = await Collection.Find(filter).ToListAsync();
            return documents;
        }

        public async Task<IEnumerable<ImageModel>> GetAllStudies()
        {
            var documents = await Collection.Find(new BsonDocument()).ToListAsync();
            return documents;
        }

        public async Task<string> GetStudyPath(string studyInstanceUID, string InstanceNumber)
        {
            var filter = Builders<ImageModel>.Filter.Eq("Tags.StudyInstanceUID", studyInstanceUID) & Builders<ImageModel>.Filter.Eq("Tags.InstanceNumber", InstanceNumber);
            var document = await Collection.Find(filter).FirstAsync();
            return document.Path;
        }

        public async Task<bool> StudyExists(string studyInstanceUID, string InstanceNumber, string SeriesInstanceUID)
        {
            var filter = Builders<ImageModel>.Filter.Eq("Tags.StudyInstanceUID", studyInstanceUID)
                       & Builders<ImageModel>.Filter.Eq("Tags.InstanceNumber", InstanceNumber)
                       & Builders<ImageModel>.Filter.Eq("Tags.SeriesInstanceUID", SeriesInstanceUID);

            var exists = await Collection.Find(filter).AnyAsync();
            return exists;
        }

        public async Task<bool> StudyExists(string SOPInstanceUID)
        {
            var filter = Builders<ImageModel>.Filter.Eq("Tags.SOPInstanceUID", SOPInstanceUID);
            var exists = await Collection.Find(filter).AnyAsync();
            return exists;
        }

        public async Task UpdateLastAccessDate(string SOPInstanceUID)
        {
            var filter = Builders<ImageModel>.Filter.Eq("Tags.SOPInstanceUID", SOPInstanceUID);
            var update = Builders<ImageModel>.Update.Set(x => x.LastAccessDate, DateTime.Now);
            await Collection.UpdateOneAsync(filter, update);
        }

        public async Task UpdateLastAccessDate(string studyInstanceUID, string InstanceNumber)
        {
            var filter = Builders<ImageModel>.Filter.Eq("Tags.StudyInstanceUID", studyInstanceUID) & Builders<ImageModel>.Filter.Eq("Tags.InstanceNumber", InstanceNumber);
            var update = Builders<ImageModel>.Update.Set(x => x.LastAccessDate, DateTime.Now);
            await Collection.UpdateOneAsync(filter, update);
        }
    }
}
