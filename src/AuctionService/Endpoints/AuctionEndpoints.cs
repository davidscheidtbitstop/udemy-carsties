using System.Security.Claims;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Filters;
using AuctionService.Models;
using AuctionService.Validators;
using Contracts;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Endpoints;

public static class AuctionEndpoints
{
    public static void MapAuctionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/auctions");
        
        group.MapGet("/", GetAllAuctions)
            .WithName("GetAllAuctions");

        group.MapGet("/{id}", GetAuctionById)
            .WithName("GetAuctionById");

        group.MapPost("/", CreateAuction)
            .AddEndpointFilter<ValidatorFilter<CreateAuctionDto>>()
            .RequireAuthorization()
            .WithName("CreateAuction");
        
        group.MapPut("/{id}", UpdateAuction)
            .RequireAuthorization()
            .WithName("UpdateAuction");

        group.MapDelete("/{id}", DeleteAuction)
            .RequireAuthorization()
            .WithName("DeleteAuction");
    }

    private static async Task<Ok<List<AuctionDto>>> GetAllAuctions(
        AuctionDbContext dbContext, string date)
    {
        var query = dbContext.Auctions.OrderBy(x => x.Item.Make)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(
                DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        
        var results = await query.ProjectToType<AuctionDto>()
            .ToListAsync();
        return TypedResults.Ok(results);
    }

    private static async Task<Results<Ok<AuctionDto>, NotFound>> GetAuctionById(
        AuctionDbContext dbContext,
        Guid id)
    {
        var auction = await dbContext.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(auction.Adapt<AuctionDto>());
    }

    private static async Task<Results<CreatedAtRoute<AuctionDto>, BadRequest<string>>> CreateAuction(
        AuctionDbContext dbContext,
        ClaimsPrincipal user,
        IPublishEndpoint publishEndpoint,
        CreateAuctionDto auctionDto)
    {
        var auction = auctionDto.Adapt<Auction>();
        auction.Seller = user.Identity.Name;

        await dbContext.AddAsync(auction);
        
        var newAuction = auction.Adapt<AuctionDto>();
        
        await publishEndpoint.Publish(newAuction.Adapt<AuctionCreated>());
        
        var result = await dbContext.SaveChangesAsync() > 0;
        
        if (!result)
        {
            return TypedResults.BadRequest("Could not save changes to the DB");
        }
        
        return TypedResults.CreatedAtRoute(newAuction,
            routeName: nameof(GetAuctionById),
            routeValues: new { id = auction.Id });
    }
    
    private static async Task<Results<Ok, NotFound<string>, ForbidHttpResult, BadRequest<string>>> UpdateAuction(
        AuctionDbContext dbContext,
        ClaimsPrincipal user,
        IPublishEndpoint publishEndpoint,
        Guid id,
        UpdateAuctionDto updateAuctionDto)
    {
        var auction = await dbContext.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null)
            return TypedResults.NotFound<string>($"ID: {id} not found");

        if (auction.Seller != user.Identity.Name)
            return TypedResults.Forbid();
        
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var updatedAuction = auction.Adapt<AuctionDto>();
        
        await publishEndpoint.Publish(updatedAuction.Adapt<AuctionUpdated>());
        
        var result = await dbContext.SaveChangesAsync() > 0;

        if (!result)
            return TypedResults.BadRequest<string>("Problem saving changes");
        
        return TypedResults.Ok();
    }
    
    private static async Task<Results<Ok, NotFound<string>, ForbidHttpResult, BadRequest<string>>> DeleteAuction(
        AuctionDbContext dbContext,
        ClaimsPrincipal user,
        IPublishEndpoint publishEndpoint,
        Guid id)
    {
        var auction = await dbContext.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null)
            return TypedResults.NotFound<string>($"ID: {id} not found");

        if (auction.Seller != user.Identity.Name) 
            return TypedResults.Forbid();
        
        dbContext.Auctions.Remove(auction);

        await publishEndpoint.Publish(new AuctionDeleted{ Id = id.ToString() });
        
        var result = await dbContext.SaveChangesAsync() > 0;

        if (!result)
            return TypedResults.BadRequest<string>("could not update DB");
            
        return TypedResults.Ok();
    }
}
