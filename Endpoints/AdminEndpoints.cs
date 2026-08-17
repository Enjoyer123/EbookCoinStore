namespace EbookCoinWallet.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        app.MapGet("/api/admin/test", () => Results.Ok(new { message = "You are an Admin!" }))
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}