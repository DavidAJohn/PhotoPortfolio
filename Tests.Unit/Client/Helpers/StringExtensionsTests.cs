using PhotoPortfolio.Client.Helpers;

namespace PhotoPortfolio.Tests.Unit.Client.Helpers;

public class StringExtensionsTests
{
    [Fact]
    public void String_Under_Limit_Returns_Expected_Value()
    {
        string testString = "test";

        string returnedString = testString.Truncate(5);

        Assert.Equal(testString, returnedString);
    }

    [Fact]
    public void String_Over_Limit_Returns_Expected_Value()
    {
        string testString = "long test string";

        string returnedString = testString.Truncate(10);

        Assert.NotEqual(testString, returnedString);
        Assert.Equal("long te...", returnedString);
    }

    [Fact]
    public void Empty_String_Returns_Expected_Value()
    {
        string testString = "";

        string returnedString = testString.Truncate(5);

        Assert.Equal(testString, returnedString);
    }
}
