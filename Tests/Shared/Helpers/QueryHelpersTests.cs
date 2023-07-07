using FluentAssertions;
using PhotoPortfolio.Shared.Helpers;
using System;
using System.Collections.Generic;

namespace PhotoPortfolio.Tests.Shared.Helpers;

public class QueryHelpersTests
{
    [Fact]
    public void AddQueryString_ShouldReturnQueryString_WhenParamsWithStringsAreNotNull()
    {
        // Arrange
        string uri = "uri";
        string name = "name";
        string value = "value";

        // Act
        string result = QueryHelpers.AddQueryString(uri, name, value);

        // Assert
        result.Should().BeOfType<string>();
        result.Should().Be("uri?name=value");
    }

    [Fact]
    public void AddQueryString_ShouldReturnQueryString_WhenParamsWithDictionaryAreNotNull()
    {
        // Arrange
        string uri = "uri";
        var querystring = new Dictionary<string, string>
        {
            { "name", "value" }
        };

        // Act
        string result = QueryHelpers.AddQueryString(uri, querystring);

        // Assert
        result.Should().BeOfType<string>();
        result.Should().Be("uri?name=value");
    }

    [Fact]
    public void AddQueryString_ShouldThrowArgumentNullException_WhenUriIsNull()
    {
        // Arrange
        string uri = null!;
        string name = "name";
        string value = "value";

        // Act
        Action act = () => QueryHelpers.AddQueryString(uri, name, value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddQueryString_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        string uri = "uri";
        string name = null!;
        string value = "value";

        // Act
        Action act = () => QueryHelpers.AddQueryString(uri, name, value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddQueryString_ShouldThrowArgumentNullException_WhenValueIsNull()
    {
        // Arrange
        string uri = "uri";
        string name = "name";
        string value = null!;

        // Act
        Action act = () => QueryHelpers.AddQueryString(uri, name, value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddQueryString_ShouldThrowArgumentNullException_WhenUriWithQuerystringIsNull()
    {
        // Arrange
        string uri = null!;
        var querystring = new Dictionary<string, string>
        {
            { "name", "value" }
        };

        // Act
        Action act = () => QueryHelpers.AddQueryString(uri, querystring);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddQueryString_ShouldThrowArgumentNullException_WhenQuerystringIsNull()
    {
        // Arrange
        string uri = "uri";

        // Act
        Action act = () => QueryHelpers.AddQueryString(uri, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
