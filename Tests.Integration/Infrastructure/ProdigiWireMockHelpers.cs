using WireMock.Matchers;

namespace PhotoPortfolio.Tests.Integration.Infrastructure;

public class ProdigiWireMockHelpers
{
    public static IMatcher[] GetOrderRequestMatchers()
    {
        var callbackUrlMatcher = new JsonPartialWildcardMatcher("{ \"callbackUrl\": \"*\" }");
        var merchantReferenceMatcher = new JsonPartialWildcardMatcher("{ \"merchantReference\": \"*\" }");
        var shippingMethodMatcher = new JsonPartialWildcardMatcher("{ \"shippingMethod\": \"*\" }");
        var idempotencyKeyMatcher = new JsonPartialWildcardMatcher("{ \"idempotencyKey\": \"*\" }");
        var recipientNameMatcher = new JsonPartialWildcardMatcher("{ \"recipient\": { \"name\": \"*\" } }");
        var recipientEmailMatcher = new JsonPartialWildcardMatcher("{ \"recipient\": { \"email\": \"*\" } }");
        var recipientPhoneNumberMatcher = new JsonPartialWildcardMatcher("{ \"recipient\": { \"phoneNumber\": \"*\" } }");
        var recipientAddressLine1Matcher = new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"line1\": \"*\" } } }");
        var recipientAddressLine2Matcher = new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"line2\": \"*\" } } }");
        var recipientAddressPostalOrZipCodeMatcher = new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"postalOrZipCode\": \"*\" } } }");
        var recipientAddressCountryCodeMatcher = new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"countryCode\": \"*\" } } }");
        var recipientAddressTownOrCityMatcher = new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"townOrCity\": \"*\" } } }");
        var recipientAddressStateOrCountyMatcher = new JsonPartialWildcardMatcher("{ \"recipient\": { \"address\": { \"stateOrCounty\": \"*\" } } }");
        var itemsMatcher = new JsonPathMatcher("$.items[*]"); // checks items exist
        var itemsMerchantReferenceMatcher = new JsonPartialWildcardMatcher("{ \"items\": [ { \"merchantReference\": \"*\" } ] }");
        var itemsSkuMatcher = new JsonPartialWildcardMatcher("{ \"items\": [ { \"sku\": \"*\" } ] }");
        var itemsCopiesMatcher = new JsonPartialWildcardMatcher("{ \"items\": [ { \"copies\": 1 } ] }");
        var itemsAttributesMatcher = new JsonPathMatcher("$.items[*].attributes"); // checks attributes exist on each item
        var itemsAssetsPrintAreaMatcher = new JsonPartialWildcardMatcher("{ \"items\": [ { \"assets\": [ { \"printArea\": \"*\" } ] } ] }");
        var itemsAssetsUrlMatcher = new JsonPartialWildcardMatcher("{ \"items\": [ { \"assets\": [ { \"url\": \"*\" } ] } ] }");
        var metadataMatcher = new JsonPartialWildcardMatcher("{ \"metadata\": { \"pi_id\": \"pi_*\" } }");

        var orderMatchers = new IMatcher[]
        {
            callbackUrlMatcher,
            merchantReferenceMatcher,
            shippingMethodMatcher,
            idempotencyKeyMatcher,
            recipientNameMatcher,
            recipientEmailMatcher,
            recipientPhoneNumberMatcher,
            recipientAddressLine1Matcher,
            recipientAddressLine2Matcher,
            recipientAddressPostalOrZipCodeMatcher,
            recipientAddressCountryCodeMatcher,
            recipientAddressTownOrCityMatcher,
            recipientAddressStateOrCountyMatcher,
            itemsMatcher,
            itemsMerchantReferenceMatcher,
            itemsSkuMatcher,
            itemsCopiesMatcher,
            itemsAttributesMatcher,
            itemsAssetsPrintAreaMatcher,
            itemsAssetsUrlMatcher,
            metadataMatcher
        };

        return orderMatchers;
    }
}
