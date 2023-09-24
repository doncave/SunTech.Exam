namespace SunTech.Exam.Model
{
    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int BirthdayInEpoch { get; set; }

        public string Email { get; set; }

        public string PartitionKey { get; set; }
    }
}
