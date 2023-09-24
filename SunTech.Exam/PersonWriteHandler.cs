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
using Microsoft.Extensions.Logging;

namespace SunTech.Exam
{
    public class PersonWriteHandler : FunctionBase
    {
        public PersonWriteHandler(CosmosClient cosmosClient)
        {
            CosmosClient = cosmosClient;
        }

        [FunctionName("PersonWriteHandler")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "person")] HttpRequest req,
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

            return null;
        }
    }
}
