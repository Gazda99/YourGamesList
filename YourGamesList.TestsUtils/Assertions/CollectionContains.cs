using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace YourGamesList.TestsUtils.Assertions;

public abstract class CollectionContains : Contains
{
    public static ContainsAllSubstringsConstraint ContainsAll(params string[] expected) => new ContainsAllSubstringsConstraint(expected);
    public static ContainsNoneSubstringsConstraint ContainsNone(params string[] forbidden) => new ContainsNoneSubstringsConstraint(forbidden);
}