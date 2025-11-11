


namespace BankingApp.Core.Application.Dtos.User
{
    public class SaveUserDto
    {
        public string? Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string DocumentIdNumber { get; set; }
        public decimal? InitialAmount { get; set; } // Monto inicial para clientes
        public List<string>? Roles { get; set; }


    }
}
