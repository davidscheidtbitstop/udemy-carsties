using AuctionService.DTOs;
using FluentValidation;

namespace AuctionService.Validators;

public class CreateAuctionDtoValidator : AbstractValidator<CreateAuctionDto>
{
    public CreateAuctionDtoValidator()
    {
        RuleFor(createAuctionDto => createAuctionDto.Make).NotEmpty();
        RuleFor(createAuctionDto => createAuctionDto.Model).NotEmpty();
        RuleFor(createAuctionDto => createAuctionDto.Year).NotEmpty();
        RuleFor(createAuctionDto => createAuctionDto.Color).NotEmpty();
        RuleFor(createAuctionDto => createAuctionDto.Mileage).NotEmpty();
        RuleFor(createAuctionDto => createAuctionDto.ImageUrl).NotEmpty();
        RuleFor(createAuctionDto => createAuctionDto.ReservePrice).NotEmpty();
        RuleFor(createAuctionDto => createAuctionDto.AuctionEnd).NotEmpty();
    }
}