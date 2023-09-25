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

namespace SunTech.Exam
{
    public class PersonUpdateDeleteHandler : FunctionBase
    {
        public PersonUpdateDeleteHandler(CosmosClient cosmosClient)
        {
            CosmosClient = cosmosClient;
        }

        [FunctionName("PersonUpdateDeleteHandler")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", "delete", Route = "person/{id}/lastname/{lastname}")] 
            HttpRequest req, ILogger log, string id, string lastname)
        {
            await InitDatabase();

            if (req.Method == HttpMethods.Put)
            {
                try
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var person = JsonConvert.DeserializeObject<Person>(requestBody);

                    ItemResponse<Person> personResponse = await Container.ReadItemAsync<Person>(id, new PartitionKey(lastname));
                    var itemBody = personResponse.Resource;

                    itemBody.FirstName = person.FirstName; 
                    itemBody.LastName = person.LastName;
                    itemBody.BirthdayInEpoch = person.BirthdayInEpoch;

                    // replace the item with the updated content
                    personResponse = await Container.ReplaceItemAsync<Person>(itemBody, itemBody.Id, new PartitionKey(itemBody.PartitionKey));

                    return new OkObjectResult(person);
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message);
                }
            }
            else if (req.Method == HttpMethods.Delete)
            {

            }

            return null;
        }
    }
}
