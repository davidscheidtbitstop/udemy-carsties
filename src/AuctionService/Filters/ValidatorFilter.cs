using FluentValidation;

namespace AuctionService.Filters;

public class ValidatorFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidatorFilter(IValidator<T> validator)
    {
        _validator = validator;
    }
    
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var obj = context.Arguments.FirstOrDefault(x => x?.GetType() == typeof(T)) as T;

        if (obj is null)
        {
            return Results.BadRequest();
        }

        var validationResult = await _validator.ValidateAsync(obj);

        if (!validationResult.IsValid)
        {
            return Results.BadRequest(String.Join("/n", validationResult.Errors));
        }

        return await next(context);
    }
}