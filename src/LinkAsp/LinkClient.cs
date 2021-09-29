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
        public string Name { get; set; }
        public string Group { get; set; } //Partition
        public string Url { get; set; }
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
    public class LinkClient
    {
        private string databaseId = ConfigurationManager.AppSettings["databaseName"];
        private string containerId = ConfigurationManager.AppSettings["containerName"];
        private Database database;
        private Container container;
        private CosmosClient cosmosClient = new CosmosClient(ConfigurationManager.AppSettings["accountEndpoint"], ConfigurationManager.AppSettings["accountKey"]);

        private async Task InitDbEnvironment()
        {
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            container = await database.CreateContainerIfNotExistsAsync(containerId, "/Group");
        }

        /// <summary>
        /// Checks if item already exist with id and partition key.
        /// If it does not exist it creates the item in the container.
        /// </summary>
        /// <param name="link"></param>
        /// <returns>Returns true if it was added, false if it wasn't.</returns>

        public async Task<bool> TryAddLink(Link link)
        {
            await InitDbEnvironment();
            try
            {
                await container.ReadItemAsync<Link>(link.Url, new PartitionKey(link.Group));
                return false;
            }
            catch (CosmosException ex)
            {
                await container.CreateItemAsync<Link>(link, new PartitionKey(link.Group));
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
        public async Task<bool> TryDeleteLink(string id, string partitionKeyValue)
        {
            await InitDbEnvironment();
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
        public async Task<List<Link>> GetAllLinksAsync()
        {
            await InitDbEnvironment();
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
        public async Task ReplaceLinkItemAsync(Link link)
        {
            await InitDbEnvironment();
            ItemResponse<Link> linkResponse = await container.ReadItemAsync<Link>(link.Url, new PartitionKey(link.Group));
            var oldLink = linkResponse.Resource;
            linkResponse = await container.ReplaceItemAsync<Link>(link, oldLink.Url, new PartitionKey(oldLink.Group));
        }
        /// <summary>
        /// Delete the database and dispose of the Cosmos Client instance
        /// </summary>
        private async Task DeleteDatabaseAndCleanupAsync()
        {
            await InitDbEnvironment();

            DatabaseResponse databaseResourceResponse = await database.DeleteAsync();
            // Also valid: await this.cosmosClient.Databases["FamilyDatabase"].DeleteAsync();

            Console.WriteLine("Deleted Database: {0}\n", databaseId);

            //Dispose of CosmosClient
            cosmosClient.Dispose();
        }
    }
}
