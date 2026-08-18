using Microsoft.EntityFrameworkCore;
using EbookCoinWallet.Api.Data;
using EbookCoinWallet.Api.Models;
using EbookCoinWallet.Api.DTOs.Books;

namespace EbookCoinWallet.Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books");

        group.MapPost("/", async (CreateBookRequest request, AppDbContext context) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title) || request.PriceInCoins < 0)
                return Results.BadRequest("Title is required and price must be non-negative.");

            var authors = await context.Authors
                .Where(a => request.AuthorIds.Contains(a.Id))
                .ToListAsync();

            if (authors.Count != request.AuthorIds.Count)
                return Results.BadRequest("One or more AuthorIds are invalid.");

            var categories = await context.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .ToListAsync();

            if (categories.Count != request.CategoryIds.Count)
                return Results.BadRequest("One or more CategoryIds are invalid.");

            var book = new Book
            {
                Title = request.Title,
                PriceInCoins = request.PriceInCoins,
                Authors = authors,
                Categories = categories
            };

            context.Books.Add(book);
            await context.SaveChangesAsync();

            return Results.Created($"/api/books/{book.Id}", book.ToResponse());

        }).RequireAuthorization(policy => policy.RequireRole("Admin"));
    
        group.MapGet("/", async (AppDbContext context, int page = 1, int pageSize = 10, string? search = null) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = context.Books
                .Where(b => !b.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => EF.Functions.ILike(b.Title, $"%{search}%"));
            }

            query = query.OrderBy(b => b.Title);

            var totalCount = await query.CountAsync();

            var books = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Authors)
                .Include(b => b.Categories)
                .ToListAsync();

            var response = books.Select(b => b.ToResponse());

            return Results.Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                items = response
            });
        });

        group.MapGet("/{id}", async (Guid id, AppDbContext context) =>
        {
            var book = await context.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.Authors)
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book is null)
                return Results.NotFound();

            return Results.Ok(book.ToResponse());

        });

        group.MapPut("/{id}", async (Guid id, CreateBookRequest request, AppDbContext context) =>
        {
            var book = await context.Books
                .Include(b => b.Authors)
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (book is null)
                return Results.NotFound();

            if (string.IsNullOrWhiteSpace(request.Title) || request.PriceInCoins < 0)
                return Results.BadRequest("Title is required and price must be non-negative.");

            var authors = await context.Authors
                .Where(a => request.AuthorIds.Contains(a.Id))
                .ToListAsync();

            if (authors.Count != request.AuthorIds.Count)
                return Results.BadRequest("One or more AuthorIds are invalid.");

            var categories = await context.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .ToListAsync();

            if (categories.Count != request.CategoryIds.Count)
                return Results.BadRequest("One or more CategoryIds are invalid.");

            book.Title = request.Title;
            book.PriceInCoins = request.PriceInCoins;
            book.Authors = authors;
            book.Categories = categories;

            await context.SaveChangesAsync();

            return Results.Ok(book.ToResponse());

        }).RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapDelete("/{id}", async (Guid id, AppDbContext context) =>
        {
            var book = await context.Books
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (book is null)
                return Results.NotFound();

            book.IsDeleted = true;
            await context.SaveChangesAsync();

            return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}