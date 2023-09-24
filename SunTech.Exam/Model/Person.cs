using Newtonsoft.Json;

namespace SunTech.Exam.Model
{
    public class Person
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int BirthdayInEpoch { get; set; }

        public string Email { get; set; }
    }
}
