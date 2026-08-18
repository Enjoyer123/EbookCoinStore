namespace EbookCoinWallet.Api.Models;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int PriceInCoins { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Author> Authors { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}