namespace SecureTokenAPI.Models
{
    public class UserToken
    {
        public int Id { get; set; }
        public Guid Token { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
    }
}
