using System;
using Newtonsoft.Json;

//https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-api-get-started#next-steps
namespace FavouriteLinkWebApp
{
    public class Link
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; } //Partition
        public string Url { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
