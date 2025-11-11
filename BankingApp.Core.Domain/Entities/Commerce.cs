namespace BankingApp.Core.Domain.Entities
{
    public class Commerce
    {
        public int Id { get; set; }
        public string ?UserId { get; set; }

        public bool IsActive { get; set; }

        public required string Name { get; set; }
        public required string Description { get; set; }

        public  required string Logo { get; set; }
        public DateTime  CreatedAt { get; set; } = DateTime.Now;

    }
}
