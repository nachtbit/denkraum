using FluentValidation;

namespace Denkraum.Application.Search;

public sealed class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    public SearchRequestValidator()
    {
        RuleFor(request => request.Query).NotEmpty().MaximumLength(2_000);
        RuleFor(request => request.TopK).InclusiveBetween(1, 20);
    }
}
