using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System;
using SunTech.Exam.Model;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SunTech.Exam
{
    public class PersonGetAllCreate : FunctionBase
    {
        public PersonGetAllCreate(CosmosClient cosmosClient)
        {
            CosmosClient = cosmosClient;
        }

        [FunctionName("PersonGetAllCreate")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "person")] HttpRequest req,
            ILogger log)
        {
            await InitDatabase();

            if (req.Method == HttpMethods.Post)
            {
                try
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var person = JsonConvert.DeserializeObject<Person>(requestBody);
                    ItemResponse<Person> personResponse = await Container.CreateItemAsync<Person>
                        (person, new PartitionKey(person.PartitionKey));
                    return new CreatedResult("/person", person);
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message);
                }
            }

            var people = await QueryItemsAsync();
            return new OkObjectResult(people);
        }
        
        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of lastName in the WHERE filter results in a more efficient query
        /// </summary>
        public async Task<List<Person>> QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Person> queryResultSetIterator = this.Container.GetItemQueryIterator<Person>(queryDefinition);

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
    }
}
