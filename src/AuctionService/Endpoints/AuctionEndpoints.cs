﻿using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Filters;
using AuctionService.Models;
using AuctionService.Validators;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
            .WithName("CreateAuction");
        
        group.MapPut("/{id}", UpdateAuction)
            .WithName("UpdateAuction");

        group.MapDelete("/{id}", DeleteAuction)
            .WithName("DeleteAuction");
    }

    private static async Task<Ok<IReadOnlyCollection<AuctionDto>>> GetAllAuctions(
        AuctionDbContext dbContext)
    {
        var auctions = await dbContext.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();

        return TypedResults.Ok(auctions.Adapt<IReadOnlyCollection<AuctionDto>>());
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

    private static async Task<Results<CreatedAtRoute<AuctionDto>, BadRequest>> CreateAuction(
        AuctionDbContext dbContext,
        CreateAuctionDto auctionDto)
    {
        var auction = auctionDto.Adapt<Auction>();
        auction.Seller = "test";

        await dbContext.AddAsync(auction);
        var result = await dbContext.SaveChangesAsync() > 0;
        
        if (!result)
        {
            throw new Exception("Failed to save auction");
        }

        return TypedResults.CreatedAtRoute(auction.Adapt<AuctionDto>(),
            routeName: nameof(GetAuctionById),
            routeValues: new { id = auction.Id });
    }
    
    private static async Task<Results<Ok, NotFound<string>, BadRequest<string>>> UpdateAuction(
        AuctionDbContext dbContext,
        Guid id,
        UpdateAuctionDto updateAuctionDto)
    {
        var auction = await dbContext.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null)
            return TypedResults.NotFound<string>($"ID: {id} not found");

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await dbContext.SaveChangesAsync() > 0;

        if (!result)
            return TypedResults.BadRequest<string>("Problem saving changes");
        
        return TypedResults.Ok();
    }
    
    private static async Task<Results<Ok, NotFound<string>, BadRequest<string>>> DeleteAuction(
        AuctionDbContext dbContext,
        Guid id)
    {
        var auction = await dbContext.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null)
            return TypedResults.NotFound<string>($"ID: {id} not found");

        dbContext.Auctions.Remove(auction);
        var result = await dbContext.SaveChangesAsync() > 0;

        if (!result)
            return TypedResults.BadRequest<string>("could not update DB");
            
        return TypedResults.Ok();
    }
   
}