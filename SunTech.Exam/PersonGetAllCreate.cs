using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Configuration;
using System;
using SunTech.Exam.Model;
using System.Collections.Generic;

namespace SunTech.Exam
{
    public class PersonGetAllCreate
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient _cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "Town";
        private string containerId = "Person";


        public PersonGetAllCreate(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }


        [FunctionName("PersonGetAllCreate")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "person")] HttpRequest req)
        {
            await InitDatabase();

            if (req.Method == HttpMethods.Post)
            {
                try
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var person = JsonConvert.DeserializeObject<Person>(requestBody);
                    ItemResponse<Person> personResponse = await container.CreateItemAsync<Person>
                        (person, new PartitionKey(person.PartitionKey));
                    return new CreatedResult("/person", person);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            var people = await QueryItemsAsync();
            return new OkObjectResult(people);
        }

        #region Private Methods

        // <QueryItemsAsync>
        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of lastName in the WHERE filter results in a more efficient query
        /// </summary>
        private async Task<List<Person>> QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Person> queryResultSetIterator = this.container.GetItemQueryIterator<Person>(queryDefinition);

            List<Person> people = new();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Person> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Person person in currentResultSet)
                {
                    people.Add(person);
                    Console.WriteLine("\tRead {0}\n", person);
                }
            }

            return people;
        }

        /// <summary>
        /// Initialize database 
        /// </summary>
        /// <returns></returns>
        private async Task InitDatabase()
        {
            await CreateDatabaseAsync();
            await CreateContainerAsync();
        }

        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", database.Id);
        }

        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/partitionKey" as the partition key path since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync()
        {
            container = await database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            Console.WriteLine("Created Container: {0}\n", container.Id);
        }

        #endregion
    }
}
