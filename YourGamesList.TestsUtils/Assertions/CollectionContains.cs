using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace YourGamesList.TestsUtils.Assertions;

public abstract class CollectionContains : Contains
{
    public static SomeItemsConstraint AnySubstring(string expected) => new(new SubstringConstraint(expected));
}