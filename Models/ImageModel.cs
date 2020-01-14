using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dicloud_api.Models
{
    [BsonIgnoreExtraElements]
    public class ImageModel
    {
        public string SavedDate { get; set; }

        public DateTime LastAccessDate { get; set; }

        public string Path { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        public ImageModel()
        {
            Tags = new Dictionary<string, string>();
        }
    }
}
