using WireMock.Matchers;

namespace PhotoPortfolio.Tests.Integration.Infrastructure;

public class ProdigiWireMockHelpers
{
    public static IMatcher[] GetOrderRequestMatchers()
    {
        return new IMatcher[]
        {
            new JsonPartialWildcardMatcher("{ \"callbackUrl\": \"*\" }"),
            new JsonPartialWildcardMatcher("{ \"merchantReference\": \"*\" }"),
            new JsonPartialWildcardMatcher("{ \"shippingMethod\": \"*\" }"),
            new JsonPartialWildcardMatcher("{ \"idempotencyKey\": \"*\" }"),
            new JsonPartialWildcardMatcher("{ \"recipient\": { \"name\": \"*\" } }"),
            new JsonPartialWildcardMatcher("{ \"recipient\": { \"email\": \"*\" } }"),
            new JsonPartialWildcardMatcher("{ \"recipient\": { \"phoneNumber\": \"*\" } }"),
            new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"line1\": \"*\" } } }"),
            new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"line2\": \"*\" } } }"),
            new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"postalOrZipCode\": \"*\" } } }"),
            new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"countryCode\": \"*\" } } }"),
            new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"townOrCity\": \"*\" } } }"),
            new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"stateOrCounty\": \"*\" } } }"),
            new JsonPathMatcher("$.items[*]"), // checks items exist
            new JsonPartialWildcardMatcher("{ \"items\": [ { \"merchantReference\": \"*\" } ] }"),
            new JsonPartialWildcardMatcher("{ \"items\": [ { \"sku\": \"*\" } ] }"),
            new JsonPartialWildcardMatcher("{ \"items\": [ { \"copies\": 1 } ] }"),
            new JsonPathMatcher("$.items[*].attributes"), // checks attributes exist on each item
            new JsonPartialWildcardMatcher("{ \"items\": [ { \"assets\": [ { \"printArea\": \"*\" } ] } ] }"),
            new JsonPartialWildcardMatcher("{ \"items\": [ { \"assets\": [ { \"url\": \"*\" } ] } ] }"),
            new JsonPartialWildcardMatcher("{ \"metadata\": { \"pi_id\": \"pi_*\" } }"),
        };
    }

    public static IMatcher[] GetQuoteRequestMatchers()
    {
        return new IMatcher[]
        {
            new JsonPartialWildcardMatcher("{ \"shippingMethod\": \"*\" }"),
            new JsonPartialWildcardMatcher("{ \"destinationCountryCode\": \"*\" }"),
            new JsonPartialWildcardMatcher("{ \"currencyCode\": \"*\" }"),
            new JsonPathMatcher("$.items[*]"), // checks 'items' property exists
            new JsonPartialWildcardMatcher("{ \"items\": [ { \"sku\": \"*\" } ] }"),
            new JsonPartialWildcardMatcher("{ \"items\": [ { \"copies\": 1 } ] }"),
            new JsonPathMatcher("$.items[*].attributes"), // checks 'attributes' property exists on each item
            new JsonPartialWildcardMatcher("{ \"items\": [ { \"assets\": [ { \"printArea\": \"*\" } ] } ] }"),
        };
    }
}
