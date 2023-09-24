using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using System.Threading.Tasks;
using System;

namespace SunTech.Exam
{
    public class FunctionBase
    {
        // The Cosmos client instance
        protected CosmosClient CosmosClient;
        protected Database Database;
        protected Microsoft.Azure.Cosmos.Container Container;

        // The name of the database and container we will create
        private string databaseId = "Town";
        private string containerId = "Person";

        public FunctionBase() 
        {
        }

        /// <summary>
        /// Initialize database 
        /// </summary>
        /// <returns></returns>
        protected async Task InitDatabase()
        {
            await CreateDatabaseAsync();
            await CreateContainerAsync();
        }

        #region Private Methods
        
        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            Database = await CosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", Database.Id);
        }

        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/partitionKey" as the partition key path since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync()
        {
            Container = await Database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            Console.WriteLine("Created Container: {0}\n", Container.Id);
        }

        #endregion
    }
}
