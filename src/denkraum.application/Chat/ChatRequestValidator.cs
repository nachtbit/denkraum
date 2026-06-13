using FluentValidation;

namespace Denkraum.Application.Chat;

public sealed class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(request => request.Question).NotEmpty().MaximumLength(4_000);
        RuleFor(request => request.TopK).InclusiveBetween(1, 20);
    }
}
