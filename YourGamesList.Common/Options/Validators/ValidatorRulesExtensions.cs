using System.Diagnostics.CodeAnalysis;
using FluentValidation;

namespace YourGamesList.Common.Options.Validators;

[ExcludeFromCodeCoverage]
public static class ValidatorRulesExtensions
{
    public static IRuleBuilderOptions<T, TElement> IsValidAbsoluteUrl<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder)
    {
        return ruleBuilder.Must((rootObject, element, context) =>
        {
            context.MessageFormatter.AppendArgument("Value", element);

            return OptionsValidatorRules.IsValidAbsoluteUrl(element?.ToString() ?? string.Empty);
        }).WithMessage("{PropertyName} \"{Value}\" is not a valid URL.");
    }
}