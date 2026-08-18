using EbookCoinWallet.Api.Models;

namespace EbookCoinWallet.Api.DTOs.Books;

public static class BookMapping
{
    public static BookResponse ToResponse(this Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        PriceInCoins = book.PriceInCoins,
        Authors = book.Authors.Select(a => a.Name).ToList(),
        Categories = book.Categories.Select(c => c.Name).ToList()
    };
}