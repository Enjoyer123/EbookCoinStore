namespace EbookCoinWallet.Api.DTOs.Books;

public class CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public int PriceInCoins { get; set; }
    public List<Guid> AuthorIds { get; set; } = new();
    public List<Guid> CategoryIds { get; set; } = new();
}