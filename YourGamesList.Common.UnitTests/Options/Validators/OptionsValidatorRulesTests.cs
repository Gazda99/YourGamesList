using YourGamesList.Common.Options.Validators;

namespace YourGamesList.Common.UnitTests.Options.Validators;

public class OptionsValidatorRulesTests
{
    [Test]
    [TestCaseSource(nameof(UrlTestCases))]
    public void IsValidAbsoluteUrl_ValidateUrl(string url, bool expectedResult)
    {
        //ACT
        var res = OptionsValidatorRules.IsValidAbsoluteUrl(url);

        //ASSERT
        Assert.That(res, Is.EqualTo(expectedResult));
    }

    private static readonly TestCaseData[] UrlTestCases =
    [
        new TestCaseData("http://example.com", true),
        new TestCaseData("https://www.google.com/search?q=test", true),
        new TestCaseData("https://localhost:8080/api/data", true),
        new TestCaseData("http://127.0.0.1", true),
        new TestCaseData("/relative/path", false),
        new TestCaseData("www.example.com", false),
        new TestCaseData("ftp://example.com", false),
        new TestCaseData("mailto:test@example.com", false),
        new TestCaseData("invalid url string", false),
        new TestCaseData("", false),
        new TestCaseData("   ", false),
        new TestCaseData(null, false)
    ];
}