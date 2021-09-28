using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

//https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-api-get-started#next-steps
namespace FavouriteLinkWebApp
{
    public class Link
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
    public class LinkClient
    {
        private static string databaseId = "Links";
        private static string containerId = "all";
        private static Database database;
        private static Container container;
        private static CosmosClient cosmosClient = new CosmosClient(ConfigurationManager.AppSettings["accountEndpoint"], ConfigurationManager.AppSettings["accountKey"]);
        public static async Task Main(string[] args)
        {
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            container = await database.CreateContainerIfNotExistsAsync(containerId, "/Tag");

            var youtube = new Link() { Id = "1", Name = "youtube", Url = "https://youtube.com/", Tag = "Learning" };

            await TryAddLink(youtube);
            //await GetAllLinksAsync();

            await ReplaceLinkItemAsync(youtube);
            bool didDelete = await TryDeleteLink(youtube.Id, youtube.Tag);
            //Console.WriteLine("Expect delete: true == " + didDelete);
            // didDelete = await TryDeleteLink(youtube.Id, youtube.Tag);
            // Console.WriteLine("Expect delete: false == " + didDelete);
        }

        /// <summary>
        /// Checks if item already exist with id and partition key.
        /// If it does not exist it creates the item in the container.
        /// </summary>
        /// <param name="link"></param>
        /// <returns>Returns true if it was added, false if it wasn't.</returns>

        public static async Task<bool> TryAddLink(Link link)
        {
            try
            {
                await container.ReadItemAsync<Link>(link.Id, new PartitionKey(link.Tag));
                return false;
            }
            catch (CosmosException ex)
            {
                await container.CreateItemAsync<Link>(link, new PartitionKey(link.Tag));
                return true;
            }
        }
        /// <summary>
        /// Checks if item already exist with id and partition key.
        /// If it does exist it creates the item in the container. 
        /// </summary>
        /// <param name="id">The id string of the link-object</param>
        /// <param name="partitionKeyValue">The tag value of the link-object</param>
        /// <returns></returns>
        public static async Task<bool> TryDeleteLink(string id, string partitionKeyValue)
        {
            try
            {
                await container.ReadItemAsync<Link>(id, new PartitionKey(partitionKeyValue));
                await container.DeleteItemAsync<Link>(id, new PartitionKey(partitionKeyValue));
                return true;
            }
            catch (CosmosException ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Runs a query (using Azure Cosmos DB SQL syntax) against the container "all" and retrieves all links.
        /// </summary>
        private static async Task<List<Link>> GetAllLinksAsync()
        {
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM all");
            FeedIterator<Link> queryResultSetIterator = container.GetItemQueryIterator<Link>(queryDefinition);

            var links = new List<Link>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Link> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Link link in currentResultSet)
                {
                    links.Add(link);
                    Console.WriteLine(link.ToString());
                }
            }
            return links;
        }
        /// <summary>
        /// Replace an item in the container
        /// </summary>
        private static async Task ReplaceLinkItemAsync(Link link)
        {
            ItemResponse<Link> linkResponse = await container.ReadItemAsync<Link>(link.Id, new PartitionKey(link.Tag));
            var oldLink = linkResponse.Resource;
            linkResponse = await container.ReplaceItemAsync<Link>(link, oldLink.Id, new PartitionKey(oldLink.Tag));
        }
        /// <summary>
        /// Delete the database and dispose of the Cosmos Client instance
        /// </summary>
        private async Task DeleteDatabaseAndCleanupAsync()
        {
            DatabaseResponse databaseResourceResponse = await database.DeleteAsync();
            // Also valid: await this.cosmosClient.Databases["FamilyDatabase"].DeleteAsync();

            Console.WriteLine("Deleted Database: {0}\n", databaseId);

            //Dispose of CosmosClient
            cosmosClient.Dispose();
        }
    }
}
