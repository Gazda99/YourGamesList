using System;
using System.Linq;
using NUnit.Framework.Constraints;

namespace YourGamesList.TestsUtils.Assertions;

public class ContainsNoneSubstringsConstraint : Constraint
{
    private readonly string[] _forbiddenSubstrings;
    private readonly StringComparison _comparison;

    public ContainsNoneSubstringsConstraint(string[] forbiddenSubstrings, StringComparison comparison = default)
    {
        _forbiddenSubstrings = forbiddenSubstrings;
        _comparison = comparison;
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        if (actual is not string message)
        {
            return new ConstraintResult(this, actual, ConstraintStatus.Error);
        }

        var foundItems = _forbiddenSubstrings
            .Where(s => message.Contains(s, _comparison));

        var containsNone = !foundItems.Any();

        return new ConstraintResult(this, actual, containsNone);
    }

    public override string Description => $"Collection containing none of: [{string.Join(", ", _forbiddenSubstrings)}] (Comparison: {_comparison})";
}