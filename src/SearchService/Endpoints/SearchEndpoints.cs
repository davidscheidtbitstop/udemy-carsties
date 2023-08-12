using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Entities;
using SearchService.Model;

namespace SearchService.Endpoints;

public static class SearchEndpoints
{

    public static void MapSearchEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/search");
        
        group.MapGet("/", SearchItems)
            .WithName("SearchItems");
    }
    
    private static async Task<IResult> SearchItems(
        string searchTerm, 
        int pageNumber = 1, 
        int pageSize = 4)
    {
        var query = DB.PagedSearch<Item>();
        
        query.Sort(x => x.Ascending(a => a.Make));
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query.Match(Search.Full, searchTerm).SortByTextScore();
        }
        
        query.PageNumber(pageNumber);
        query.PageSize(pageSize);
        
        var result = await query.ExecuteAsync();
        
        return Results.Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
    
}