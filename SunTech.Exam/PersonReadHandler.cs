using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using SunTech.Exam.Model;
using System.Collections.Generic;

namespace SunTech.Exam
{
    public class PersonReadHandler : FunctionBase
    {
        public PersonReadHandler(CosmosClient cosmosClient)
        {
            CosmosClient = cosmosClient;
        }

        [FunctionName("PersonReadHandler")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "person")] HttpRequest req,
            ILogger log)
        {
            await InitDatabase();

            try
            {
                var people = await QueryItemsAsync();
                return new OkObjectResult(people);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
            }

            return null;
        }

        #region Private Methods

        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of lastName in the WHERE filter results in a more efficient query
        /// </summary>
        private async Task<List<Person>> QueryItemsAsync()
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

        #endregion
    }
}
