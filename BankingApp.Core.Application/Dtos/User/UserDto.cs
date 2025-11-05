namespace BankingApp.Core.Application.Dtos.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public bool IsVerified { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string DocumentIdNumber { get; set; }
        public required string Role { get; set; }
    }
}
