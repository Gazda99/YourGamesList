using System;
using System.Linq;
using NUnit.Framework.Constraints;

namespace YourGamesList.TestsUtils.Assertions;

public class ContainsAllSubstringsConstraint : Constraint
{
    private readonly string[] _expectedSubstrings;
    private readonly StringComparison _comparison;

    public ContainsAllSubstringsConstraint(string[] expectedSubstrings, StringComparison comparison = default)
    {
        _expectedSubstrings = expectedSubstrings;
        _comparison = comparison;
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        if (actual is not string message)
        {
            return new ConstraintResult(this, actual, ConstraintStatus.Error);
        }

        var missing = _expectedSubstrings.Where(s => !message.Contains(s, _comparison));

        var hasAll = !missing.Any();

        return new ConstraintResult(this, actual, hasAll);
    }

    public override string Description => $"Collection containing all of: [{string.Join(", ", _expectedSubstrings)}] (Comparison: {_comparison})";
}