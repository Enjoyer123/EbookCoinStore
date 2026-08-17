namespace EbookCoinWallet.Api.Models;

public enum UserRole
{
    Customer,
    Admin,
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email {get; set;} = string.Empty;
    public string PasswordHash {get; set;} = string.Empty;
    public int CoinBalance {get; set;} = 0;
    public UserRole Role {get; set;} = UserRole.Customer;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}