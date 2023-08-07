using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace PhotoPortfolio.Tests.Integration;

public class ProdigiPrintApiServer : IDisposable
{
    private WireMockServer _server;
    public string Url { get; set; }

    public void Start()
    {
        _server = WireMockServer.Start();
        Url = _server.Url!;

        _server
            .Given(Request.Create()
            .WithPath("/quotes")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-000000000000")
            .WithBody(new JsonPartialWildcardMatcher(@"{
                ""shippingMethod"": ""ReturnCreatedWithIssues"",
                ""destinationCountryCode"": ""GB"",
                ""currencyCode"": ""GBP"",
                ""items"": [
                    {
                        ""sku"": ""eco-can-16x24"",
                        ""copies"": 1,
                        ""attributes"": {
                            ""wrap"" : ""MirrorWrap""
                        },
                        ""assets"": [
                            {
                            ""printArea"": ""default""
                            }
                        ]
                    }
                ]
            }", true))
            .UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody(GenerateQuoteCreatedWithIssuesResponseBody()
        ));

        _server
            .Given(Request.Create()
            .WithPath("/quotes")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-000000000000")
            .WithBody(new JsonMatcher(@"{
                ""shippingMethod"": ""Standard"",
                ""destinationCountryCode"": ""GB"",
                ""currencyCode"": ""GBP"",
                ""items"": [
                    {
                        ""sku"": ""eco-can-16x24"",
                        ""copies"": 1,
                        ""attributes"": {
                            ""wrap"" : ""MirrorWrap""
                        },
                        ""assets"": [
                            {
                            ""printArea"": ""default""
                            }
                        ]
                    }
                ]
            }", true))
            .UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody(GenerateQuoteCreatedResponseBody()
        ));

    }

    private static string GenerateQuoteCreatedResponseBody()
    {
        return @"{
        ""outcome"": ""Created"",
        ""issues"": null,
        ""quotes"": [
            {
                ""shipmentMethod"": ""Standard"",
                ""costSummary"": {
                    ""items"": {
                        ""amount"": ""19.00"",
                        ""currency"": ""GBP""
                    },
                    ""shipping"": {
                        ""amount"": ""9.95"",
                        ""currency"": ""GBP""
                    },
                    ""totalCost"": {
                        ""amount"": ""34.74"",
                        ""currency"": ""GBP""
                    },
                    ""totalTax"": {
                        ""amount"": ""5.79"",
                        ""currency"": ""GBP""
                    }
                },
                ""shipments"": [
                    {
                        ""carrier"": {
                            ""name"": ""DPD Local"",
                            ""service"": ""DPD Local Next Day""
                        },
                        ""fulfillmentLocation"": {
                            ""countryCode"": ""GB"",
                            ""labCode"": ""prodigi_gb2""
                        },
                        ""cost"": {
                            ""amount"": ""9.95"",
                            ""currency"": ""GBP""
                        },
                        ""items"": [
                            ""1""
                        ],
                        ""tax"": {
                            ""amount"": ""1.99"",
                            ""currency"": ""GBP""
                        }
                    }
                ],
                ""items"": [
                    {
                        ""id"": ""1"",
                        ""sku"": ""ECO-CAN-16x24"",
                        ""copies"": 1,
                        ""unitCost"": {
                            ""amount"": ""19.00"",
                            ""currency"": ""GBP""
                        },
                        ""attributes"": {
                            ""wrap"": ""MirrorWrap""
                        },
                        ""assets"": [
                            {
                                ""printArea"": ""default""
                            }
                        ],
                        ""taxUnitCost"": {
                            ""amount"": ""3.80"",
                            ""currency"": ""GBP""
                        }
                    }
                ]
            }
        ],
        ""traceParent"": ""sent_from_mock_ProdigiPrintApiServer""
    }";
    }

    private static string GenerateQuoteCreatedWithIssuesResponseBody()
    {
        return @"{
        ""outcome"": ""CreatedWithIssues"",
        ""issues"": [
            {
                ""errorCode"" : ""destinationCountryCode.UsSalesTaxWarning"",
                ""description"" : ""Price may be subject to US sales tax""
            }
        ],
        ""quotes"": [
            {
                ""shipmentMethod"": ""Standard"",
                ""costSummary"": {
                    ""items"": {
                        ""amount"": ""19.00"",
                        ""currency"": ""GBP""
                    },
                    ""shipping"": {
                        ""amount"": ""9.95"",
                        ""currency"": ""GBP""
                    },
                    ""totalCost"": {
                        ""amount"": ""34.74"",
                        ""currency"": ""GBP""
                    },
                    ""totalTax"": {
                        ""amount"": ""5.79"",
                        ""currency"": ""GBP""
                    }
                },
                ""shipments"": [
                    {
                        ""carrier"": {
                            ""name"": ""DPD Local"",
                            ""service"": ""DPD Local Next Day""
                        },
                        ""fulfillmentLocation"": {
                            ""countryCode"": ""GB"",
                            ""labCode"": ""prodigi_gb2""
                        },
                        ""cost"": {
                            ""amount"": ""9.95"",
                            ""currency"": ""GBP""
                        },
                        ""items"": [
                            ""1""
                        ],
                        ""tax"": {
                            ""amount"": ""1.99"",
                            ""currency"": ""GBP""
                        }
                    }
                ],
                ""items"": [
                    {
                        ""id"": ""1"",
                        ""sku"": ""ECO-CAN-16x24"",
                        ""copies"": 1,
                        ""unitCost"": {
                            ""amount"": ""19.00"",
                            ""currency"": ""GBP""
                        },
                        ""attributes"": {
                            ""wrap"": ""MirrorWrap""
                        },
                        ""assets"": [
                            {
                                ""printArea"": ""default""
                            }
                        ],
                        ""taxUnitCost"": {
                            ""amount"": ""3.80"",
                            ""currency"": ""GBP""
                        }
                    }
                ]
            }
        ],
        ""traceParent"": ""sent_from_mock_ProdigiPrintApiServer""
    }";
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }
}
