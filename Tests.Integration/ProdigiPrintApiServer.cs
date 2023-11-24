using PhotoPortfolio.Shared.Models.Prodigi.Orders;
using PhotoPortfolio.Shared.Models.Prodigi.Quotes;
using PhotoPortfolio.Tests.Integration.Infrastructure;
using System.Text.Json;
using WireMock;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Types;
using WireMock.Util;

namespace PhotoPortfolio.Tests.Integration;

public class ProdigiPrintApiServer : IDisposable
{
    private WireMockServer? server;
    public string? Url { get; set; }

    public void Start()
    {
        server = WireMockServer.Start();
        Url = server.Url!;


        // "/quotes" endpoint responses
        // 

        var quoteRequestMatchers = ProdigiWireMockHelpers.GetQuoteRequestMatchers();

        server
            .Given(Request.Create()
            .WithPath("/quotes")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-created") // generates a "Created" outcome in response
            .WithBody(quoteRequestMatchers, MatchOperator.And)
            .UsingPost())
            .RespondWith(Response.Create()
                .WithCallback(req =>
                {
                    var request = JsonSerializer.Deserialize<CreateQuoteDto>(req.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var items = new List<object>();
                    var unitCostTotal = 0.0m;
                    var totalTax  = 0.0m;
                    var currency = request!.CurrencyCode;

                    foreach (var item in request!.Items)
                    {
                        var assets = new List<object>();
                        foreach (var asset in item.Assets)
                        {
                            assets.Add(new
                            {
                                printArea = asset.GetValueOrDefault("printArea"),
                            });
                        }

                        // generate a random unit cost amount
                        var rand = new Random();
                        var unitCostAmount = new decimal(Math.Round(rand.NextDouble() * 100, 2));
                        unitCostAmount = unitCostAmount < 10 ? unitCostAmount + 10 : unitCostAmount;

                        items.Add(new
                        {
                            id = "1234567",
                            sku = item.Sku,
                            copies = item.Copies,
                            unitCost = new
                            {
                                amount = unitCostAmount.ToString(),
                                currency
                            },
                            attributes = item.Attributes,
                            assets,
                            taxUnitCost = new
                            {
                                amount = Math.Round(unitCostAmount * 0.2m, 2).ToString(),
                                currency
                            }
                        });

                        unitCostTotal += unitCostAmount;
                    }

                    totalTax = Math.Round((unitCostTotal + 9.95m) * 0.2m, 2);

                    var responseMessage = new ResponseMessage
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, WireMockList<string>> { { "Content-Type", new WireMockList<string>("application/json") } },
                        BodyData = new BodyData
                        {
                            DetectedBodyType = BodyType.Json,
                            BodyAsJson = new
                            {
                                outcome = "Created",
                                issues = (string?)null,
                                quotes = new []
                                {
                                    new
                                    {
                                        shipmentMethod = request!.ShippingMethod,
                                        costSummary = new
                                        {
                                            items = new
                                            {
                                                amount = unitCostTotal.ToString(),
                                                currency
                                            },
                                            shipping = new
                                            {
                                                amount = "9.95",
                                                currency
                                            },
                                            totalCost = new
                                            {
                                                amount = (unitCostTotal + 9.95m + totalTax).ToString(),
                                                currency
                                            },
                                            totalTax = new
                                            {
                                                amount = totalTax.ToString(),
                                                currency
                                            }
                                        },
                                        shipments = new[]
                                        {
                                            new
                                            {
                                                carrier = new
                                                {
                                                    name = "DPD Local",
                                                    service = "DPD Local Next Day"
                                                },
                                                fulfillmentLocation = new
                                                {
                                                    countryCode = "GB",
                                                    labCode = "prodigi_gb2"
                                                },
                                                cost = new
                                                {
                                                    amount = "9.95",
                                                    currency = "GBP"
                                                },
                                                items = new[] { items.Count.ToString() },
                                                tax = new
                                                {
                                                    amount = "1.99",
                                                    currency = "GBP"
                                                }
                                            }
                                        },
                                        items,
                                    },
                                },
                                traceParent = "sent_from_mock_ProdigiPrintApiServer"
                            }
                        }
                    };

                    return Task.FromResult(responseMessage);
                }
            ).WithTransformer()
        );

        server
            .Given(Request.Create()
            .WithPath("/quotes")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-createdwithissues") // generates a "CreatedWithIssues" outcome
            .WithBody(quoteRequestMatchers, MatchOperator.And)
            .UsingPost())
            .RespondWith(Response.Create()
                .WithCallback(req =>
                {
                    var request = JsonSerializer.Deserialize<CreateQuoteDto>(req.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var issues = new List<object>
                    {
                        new
                        {
                            errorCode = "destinationCountryCode.UsSalesTaxWarning",
                            description = "Price may be subject to US sales tax"
                        }
                    };

                    var items = new List<object>();
                    var unitCostTotal = 0.0m;
                    var totalTax = 0.0m;
                    var currency = request!.CurrencyCode;

                    foreach (var item in request!.Items)
                    {
                        var assets = new List<object>();
                        foreach (var asset in item.Assets)
                        {
                            assets.Add(new
                            {
                                printArea = asset.GetValueOrDefault("printArea"),
                            });
                        }

                        // generate a random unit cost amount
                        var rand = new Random();
                        var unitCostAmount = new decimal(Math.Round(rand.NextDouble() * 100, 2));
                        unitCostAmount = unitCostAmount < 10 ? unitCostAmount + 10 : unitCostAmount;

                        items.Add(new
                        {
                            id = "1234567",
                            sku = item.Sku,
                            copies = item.Copies,
                            unitCost = new
                            {
                                amount = unitCostAmount.ToString(),
                                currency
                            },
                            attributes = item.Attributes,
                            assets,
                            taxUnitCost = new
                            {
                                amount = Math.Round(unitCostAmount * 0.2m, 2).ToString(),
                                currency
                            }
                        });

                        unitCostTotal += unitCostAmount;
                    }

                    totalTax = Math.Round((unitCostTotal + 9.95m) * 0.2m, 2);

                    var responseMessage = new ResponseMessage
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, WireMockList<string>> { { "Content-Type", new WireMockList<string>("application/json") } },
                        BodyData = new BodyData
                        {
                            DetectedBodyType = BodyType.Json,
                            BodyAsJson = new
                            {
                                outcome = "CreatedWithIssues",
                                issues,
                                quotes = new[]
                                {
                                    new
                                    {
                                        shipmentMethod = request!.ShippingMethod,
                                        costSummary = new
                                        {
                                            items = new
                                            {
                                                amount = unitCostTotal.ToString(),
                                                currency
                                            },
                                            shipping = new
                                            {
                                                amount = "9.95",
                                                currency
                                            },
                                            totalCost = new
                                            {
                                                amount = (unitCostTotal + 9.95m + totalTax).ToString(),
                                                currency
                                            },
                                            totalTax = new
                                            {
                                                amount = totalTax.ToString(),
                                                currency
                                            }
                                        },
                                        shipments = new[]
                                        {
                                            new
                                            {
                                                carrier = new
                                                {
                                                    name = "DPD Local",
                                                    service = "DPD Local Next Day"
                                                },
                                                fulfillmentLocation = new
                                                {
                                                    countryCode = "GB",
                                                    labCode = "prodigi_gb2"
                                                },
                                                cost = new
                                                {
                                                    amount = "9.95",
                                                    currency = "GBP"
                                                },
                                                items = new[] { items.Count.ToString() },
                                                tax = new
                                                {
                                                    amount = "1.99",
                                                    currency = "GBP"
                                                }
                                            }
                                        },
                                        items,
                                    },
                                },
                                traceParent = "sent_from_mock_ProdigiPrintApiServer"
                            }
                        }
                    };

                    return Task.FromResult(responseMessage);
                }
            ).WithTransformer()
        );

        server
            .Given(Request.Create()
            .WithPath("/quotes")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-badrequest") // generates a 400 Bad Request response
            .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithHeader("Content-Type", "application/problem+json")
                .WithBody(
                    @"{
                        ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
                        ""title"": ""One or more validation errors occurred."",
                        ""status"": 400,
                        ""traceId"": ""0HMHOVKKV3MHN:00000002"",
                        ""errors"": {
                            ""Orders"": [
                                ""The Quote object supplied is not valid""
                            ]
                        }
                    }"
                )
            );

        server
            .Given(Request.Create()
            .WithPath("/quotes")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-unexpected-quote-structure") // generates a 200 OK response with a quote that can't be deserialized
            .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(
                    @"{
                        ""outcome"": ""Created"",
                        ""unexpectedfield"": ""unexpectedvalue""
                    "
                )
            );

        server
            .Given(Request.Create()
            .WithPath("/quotes")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-unexpected-outcome") // generates a 200 OK response with an unexpected 'outcome' value
            .UsingPost())
            .RespondWith(Response.Create()
                .WithCallback(req =>
                {
                    var request = JsonSerializer.Deserialize<CreateQuoteDto>(req.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var items = new List<object>();
                    var unitCostTotal = 0.0m;
                    var totalTax = 0.0m;
                    var currency = request!.CurrencyCode;

                    foreach (var item in request!.Items)
                    {
                        var assets = new List<object>();
                        foreach (var asset in item.Assets)
                        {
                            assets.Add(new
                            {
                                printArea = asset.GetValueOrDefault("printArea"),
                            });
                        }

                        // generate a random unit cost amount
                        var rand = new Random();
                        var unitCostAmount = new decimal(Math.Round(rand.NextDouble() * 100, 2));
                        unitCostAmount = unitCostAmount < 10 ? unitCostAmount + 10 : unitCostAmount;

                        items.Add(new
                        {
                            id = "1234567",
                            sku = item.Sku,
                            copies = item.Copies,
                            unitCost = new
                            {
                                amount = unitCostAmount.ToString(),
                                currency
                            },
                            attributes = item.Attributes,
                            assets,
                            taxUnitCost = new
                            {
                                amount = Math.Round(unitCostAmount * 0.2m, 2).ToString(),
                                currency
                            }
                        });

                        unitCostTotal += unitCostAmount;
                    }

                    totalTax = Math.Round((unitCostTotal + 9.95m) * 0.2m, 2);

                    var responseMessage = new ResponseMessage
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, WireMockList<string>> { { "Content-Type", new WireMockList<string>("application/json") } },
                        BodyData = new BodyData
                        {
                            DetectedBodyType = BodyType.Json,
                            BodyAsJson = new
                            {
                                outcome = "UnexpectedOutcome", // this is the unexpected outcome value
                                issues = (string?)null,
                                quotes = new[]
                                {
                                    new
                                    {
                                        shipmentMethod = request!.ShippingMethod,
                                        costSummary = new
                                        {
                                            items = new
                                            {
                                                amount = unitCostTotal.ToString(),
                                                currency
                                            },
                                            shipping = new
                                            {
                                                amount = "9.95",
                                                currency
                                            },
                                            totalCost = new
                                            {
                                                amount = (unitCostTotal + 9.95m + totalTax).ToString(),
                                                currency
                                            },
                                            totalTax = new
                                            {
                                                amount = totalTax.ToString(),
                                                currency
                                            }
                                        },
                                        shipments = new[]
                                        {
                                            new
                                            {
                                                carrier = new
                                                {
                                                    name = "DPD Local",
                                                    service = "DPD Local Next Day"
                                                },
                                                fulfillmentLocation = new
                                                {
                                                    countryCode = "GB",
                                                    labCode = "prodigi_gb2"
                                                },
                                                cost = new
                                                {
                                                    amount = "9.95",
                                                    currency = "GBP"
                                                },
                                                items = new[] { items.Count.ToString() },
                                                tax = new
                                                {
                                                    amount = "1.99",
                                                    currency = "GBP"
                                                }
                                            }
                                        },
                                        items,
                                    },
                                },
                                traceParent = "sent_from_mock_ProdigiPrintApiServer"
                            }
                        }
                    };

                    return Task.FromResult(responseMessage);
                }
            ).WithTransformer()
        );


        // "/orders" endpoint responses
        //

        var orderRequestMatchers = ProdigiWireMockHelpers.GetOrderRequestMatchers();

        server
            .Given(Request.Create()
            .WithPath("/orders")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-created") // generates a "Created" outcome in response
            .WithBody(orderRequestMatchers, MatchOperator.And)
            .UsingPost())
            .RespondWith(Response.Create()
                .WithCallback(req =>
                {
                    var request = JsonSerializer.Deserialize<Order>(req.Body, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var items = new List<object>();

                    foreach (var item in request!.Items)
                    {
                        var assets = new List<object>();
                        foreach (var asset in item.Assets)
                        {
                            assets.Add(new
                            {
                                id = "ast_123",
                                printArea = asset.GetValueOrDefault("printArea"),
                                md5Hash = (string?)null,
                                url = asset.GetValueOrDefault("url"),
                                thumbnailUrl = (string?)null,
                                status = "InProgress"
                            });
                        }

                        items.Add(new
                        {
                            id = "ori_1234567",
                            status = "NotYetDownloaded",
                            merchantReference = item.MerchantReference,
                            sku = item.Sku,
                            copies = item.Copies,
                            sizing = item.Sizing,
                            thumbnailUrl = (string?)null,
                            attributes = item.Attributes,
                            assets,
                            recipientCost = (string?)null,
                            correlationIdentifier = "23989788686705152"
                        });
                    }
                
                    var responseMessage = new ResponseMessage
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, WireMockList<string>> {{ "Content-Type", new WireMockList<string>("application/json") }},
                        BodyData = new BodyData
                        {
                            DetectedBodyType = BodyType.Json,
                            BodyAsJson = new
                            {
                                outcome = "Created",
                                order = new
                                {
                                    id = "ord_1234567",
                                    created = "2023-10-16T14:14:51.02Z",
                                    lastUpdated = "2023-10-16T14:14:51.7746508Z",
                                    callbackUrl = request!.CallbackUrl,
                                    merchantReference = request.MerchantReference,
                                    shippingMethod = request.ShippingMethod,
                                    idempotencyKey = request.IdempotencyKey,
                                    status = new
                                    {
                                        stage = "InProgress",
                                        issues = Array.Empty<string>(),
                                        details = new
                                        {
                                            downloadAssets = "NotStarted",
                                            printReadyAssetsPrepared = "NotStarted",
                                            allocateProductionLocation = "NotStarted",
                                            inProduction = "NotStarted",
                                            shipping = "NotStarted"
                                        }
                                    },
                                    charges = Array.Empty<string>(),
                                    shipments = Array.Empty<string>(),
                                    recipient = new 
                                    {
                                        name = request.Recipient.Name,
                                        email = request.Recipient.Email,
                                        phoneNumber = request.Recipient.PhoneNumber,
                                        address = new
                                        {
                                            line1 = request.Recipient.Address.Line1,
                                            line2 = request.Recipient.Address.Line2,
                                            postalOrZipCode = request.Recipient.Address.PostalOrZipCode,
                                            countryCode = request.Recipient.Address.CountryCode,
                                            townOrCity = request.Recipient.Address.TownOrCity,
                                            stateOrCounty = request.Recipient.Address.StateOrCounty
                                        }
                                    },
                                    items,
                                    packingSlip = (string?)null,
                                    metadata = request.Metadata
                                },
                                traceParent = "sent_from_mock_ProdigiPrintApiServer"
                            }
                        }
                    };

                    return Task.FromResult(responseMessage);
                }
            ).WithTransformer()
        );

        server
            .Given(Request.Create()
            .WithPath("/orders")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-createdwithissues") // generates a "CreatedWithIssues" outcome in response
            .WithBody(orderRequestMatchers, MatchOperator.And)
            .UsingPost())
            .RespondWith(Response.Create()
                .WithCallback(req =>
                {
                    var request = JsonSerializer.Deserialize<Order>(req.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var issues = new List<object>
                    {
                        new
                        {
                            objectId = "ori_12345",
                            errorCode = "items.assets.NotDownloaded",
                            description = "Warning: Download attempt 1 of 10 failed for 'default' asset on item 'ori_12345' at location 'http://source.url'"
                        },
                        new
                        {
                            objectId = "ord_829398",
                            errorCode = "RequiresPaymentAuthorisation",
                            description = "Payment authorisation required for 'ord_829398' (195.02USD) please use the following URL to make payment: https://beta-dashboard.pwinty.com/payment/97323",
                            authorisationDetails = new
                            {
                                authorisationUrl = "https://beta-dashboard.pwinty.com/payment/97323",
                                paymentDetails = new
                                {
                                    amount = "195.02",
                                    currency = "USD"
                                }
                            }
                        }
                    };

                    var items = new List<object>();

                    foreach (var item in request!.Items)
                    {
                        var assets = new List<object>();
                        foreach (var asset in item.Assets)
                        {
                            assets.Add(new
                            {
                                id = "ast_123",
                                printArea = asset.GetValueOrDefault("printArea"),
                                md5Hash = (string?)null,
                                url = asset.GetValueOrDefault("url"),
                                thumbnailUrl = (string?)null,
                                status = "InProgress"
                            });
                        }

                        items.Add(new
                        {
                            id = "ori_1234567",
                            status = "NotYetDownloaded",
                            merchantReference = item.MerchantReference,
                            sku = item.Sku,
                            copies = item.Copies,
                            sizing = item.Sizing,
                            thumbnailUrl = (string?)null,
                            attributes = item.Attributes,
                            assets,
                            recipientCost = (string?)null,
                            correlationIdentifier = "23989788686705152"
                        });
                    }

                    var responseMessage = new ResponseMessage
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, WireMockList<string>> { { "Content-Type", new WireMockList<string>("application/json") } },
                        BodyData = new BodyData
                        {
                            DetectedBodyType = BodyType.Json,
                            BodyAsJson = new
                            {
                                outcome = "Created",
                                order = new
                                {
                                    id = "ord_1234567",
                                    created = "2023-10-16T14:14:51.02Z",
                                    lastUpdated = "2023-10-16T14:14:51.7746508Z",
                                    callbackUrl = request!.CallbackUrl,
                                    merchantReference = request.MerchantReference,
                                    shippingMethod = request.ShippingMethod,
                                    idempotencyKey = request.IdempotencyKey,
                                    status = new
                                    {
                                        stage = "InProgress",
                                        issues,
                                        details = new
                                        {
                                            downloadAssets = "NotStarted",
                                            printReadyAssetsPrepared = "NotStarted",
                                            allocateProductionLocation = "NotStarted",
                                            inProduction = "NotStarted",
                                            shipping = "NotStarted"
                                        }
                                    },
                                    charges = Array.Empty<string>(),
                                    shipments = Array.Empty<string>(),
                                    recipient = new
                                    {
                                        name = request.Recipient.Name,
                                        email = request.Recipient.Email,
                                        phoneNumber = request.Recipient.PhoneNumber,
                                        address = new
                                        {
                                            line1 = request.Recipient.Address.Line1,
                                            line2 = request.Recipient.Address.Line2,
                                            postalOrZipCode = request.Recipient.Address.PostalOrZipCode,
                                            countryCode = request.Recipient.Address.CountryCode,
                                            townOrCity = request.Recipient.Address.TownOrCity,
                                            stateOrCounty = request.Recipient.Address.StateOrCounty
                                        }
                                    },
                                    items,
                                    packingSlip = (string?)null,
                                    metadata = request.Metadata
                                },
                                traceParent = "sent_from_mock_ProdigiPrintApiServer"
                            }
                        }
                    };

                    return Task.FromResult(responseMessage);
                }
            ).WithTransformer()
        );

        server
            .Given(Request.Create()
            .WithPath("/orders")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-alreadyexists") // generates an "AlreadyExists" outcome in response
            .WithBody(orderRequestMatchers, MatchOperator.And)
            .UsingPost())
            .RespondWith(Response.Create()
                .WithCallback(req =>
                {
                    var request = JsonSerializer.Deserialize<Order>(req.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var items = new List<object>();

                    foreach (var item in request!.Items)
                    {
                        var assets = new List<object>();
                        foreach (var asset in item.Assets)
                        {
                            assets.Add(new
                            {
                                id = "ast_123",
                                printArea = asset.GetValueOrDefault("printArea"),
                                md5Hash = (string?)null,
                                url = asset.GetValueOrDefault("url"),
                                thumbnailUrl = (string?)null,
                                status = "InProgress"
                            });
                        }

                        items.Add(new
                        {
                            id = "ori_1234567",
                            status = "Ok",
                            merchantReference = item.MerchantReference,
                            sku = item.Sku,
                            copies = item.Copies,
                            sizing = item.Sizing,
                            thumbnailUrl = (string?)null,
                            attributes = item.Attributes,
                            assets,
                            recipientCost = (string?)null,
                            correlationIdentifier = "23989788686705152"
                        });
                    }

                    var charges = new List<object>();
                    var itemsInCharges = new List<object>();

                    itemsInCharges.Add(new
                    {
                        id = "chi_992753",
                        itemId = (string?)null,
                        cost = new
                        {
                            amount = "9.95",
                            currency = "GBP"
                        },
                        shipmentId = "shp_665175",
                        chargeType = "Shipping"
                    });

                    foreach (var item in request!.Items)
                    {
                        itemsInCharges.Add(new
                        {
                            id = "chi_992753",
                            itemId = (string?)null,
                            cost = new
                            {
                                amount = "10.00",
                                currency = "GBP"
                            },
                            shipmentId = (string?)null,
                            chargeType = "Item"
                        });
                    }

                    charges.Add(new
                    {
                        id = "chg_465100",
                        prodigiInvoiceNumber = (string?)null,
                        totalCost = new
                        {
                            amount = "18.30",
                            currency = "GBP"
                        },
                        totalTax = new
                        {
                            amount = "3.05",
                            currency = "GBP"
                        },
                        items = itemsInCharges
                    });

                    var shipments = new List<object>();
                    var itemsInShipment = new List<object>();

                    foreach (var item in request!.Items)
                    {
                        itemsInShipment.Add(new
                        {
                            itemId = "ori_1426359"
                        });
                    }

                    shipments.Add(new
                    {
                        id = "shp_665175",
                        dispatchDate = "2023-10-16T14:29:10.283Z",
                        carrier = new
                        {
                            name = "EVRi",
                            service = "Evri Standard 48"
                        },
                        fulfillmentLocation = new
                        {
                            countryCode = "GB",
                            labCode = "prodigi_gb3"
                        },
                        tracking = new
                        {
                            number = "PH000000000GB",
                            url = "https://www.royalmail.com/portal/rm/track?trackNumber=PH000000000GB"
                        },
                        items = itemsInShipment,
                        status = "Shipped"
                    });
                    
                    var responseMessage = new ResponseMessage
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, WireMockList<string>> { { "Content-Type", new WireMockList<string>("application/json") } },
                        BodyData = new BodyData
                        {
                            DetectedBodyType = BodyType.Json,
                            BodyAsJson = new
                            {
                                outcome = "AlreadyExists",
                                order = new
                                {
                                    id = "ord_1234567",
                                    created = "2023-10-16T14:14:51.02Z",
                                    lastUpdated = "2023-10-16T14:14:51.7746508Z",
                                    callbackUrl = request!.CallbackUrl,
                                    merchantReference = request.MerchantReference,
                                    shippingMethod = request.ShippingMethod,
                                    idempotencyKey = request.IdempotencyKey,
                                    status = new
                                    {
                                        stage = "Complete",
                                        issues = Array.Empty<string>(),
                                        details = new
                                        {
                                            downloadAssets = "Complete",
                                            printReadyAssetsPrepared = "Complete",
                                            allocateProductionLocation = "Complete",
                                            inProduction = "Complete",
                                            shipping = "Complete"
                                        }
                                    },
                                    charges = Array.Empty<string>(),
                                    shipments,
                                    recipient = new
                                    {
                                        name = request.Recipient.Name,
                                        email = request.Recipient.Email,
                                        phoneNumber = request.Recipient.PhoneNumber,
                                        address = new
                                        {
                                            line1 = request.Recipient.Address.Line1,
                                            line2 = request.Recipient.Address.Line2,
                                            postalOrZipCode = request.Recipient.Address.PostalOrZipCode,
                                            countryCode = request.Recipient.Address.CountryCode,
                                            townOrCity = request.Recipient.Address.TownOrCity,
                                            stateOrCounty = request.Recipient.Address.StateOrCounty
                                        }
                                    },
                                    items,
                                    packingSlip = (string?)null,
                                    metadata = request.Metadata
                                },
                                traceParent = "sent_from_mock_ProdigiPrintApiServer"
                            }
                        }
                    };

                    return Task.FromResult(responseMessage);
                }
            ).WithTransformer()
        );

        server
            .Given(Request.Create()
            .WithPath("/orders")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-badrequest") // generates a 400 Bad Request response
            .UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400)
            .WithHeader("Content-Type", "application/problem+json")
            .WithBody(
                @"{
                    ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
                    ""title"": ""One or more validation errors occurred."",
                    ""status"": 400,
                    ""traceId"": ""0HMHOVKKV3MHN:00000002"",
                    ""errors"": {
                        ""Orders"": [
                            ""The Order object supplied is not valid""
                        ]
                    }
                }"
            )
        );

        server
            .Given(Request.Create()
            .WithPath("/orders")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-unexpectedorderstructure") // generates a 200 OK response with an order that can't be deserialized
            .UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody(
                @"{
                    ""outcome"": ""Created"",
                    ""unexpectedfield"": ""unexpectedvalue""
                "
            )
        );

        server
            .Given(Request.Create()
            .WithPath("/orders")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-unexpectedoutcome") // generates a 200 OK response with an unexpected outcome
            .UsingPost())
            .RespondWith(Response.Create()
                .WithCallback(req =>
                {
                    var request = JsonSerializer.Deserialize<Order>(req.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var items = new List<object>();

                    foreach (var item in request!.Items)
                    {
                        var assets = new List<object>();
                        foreach (var asset in item.Assets)
                        {
                            assets.Add(new
                            {
                                id = "ast_123",
                                printArea = asset.GetValueOrDefault("printArea"),
                                md5Hash = (string?)null,
                                url = asset.GetValueOrDefault("url"),
                                thumbnailUrl = (string?)null,
                                status = "InProgress"
                            });
                        }

                        items.Add(new
                        {
                            id = "ori_1234567",
                            status = "NotYetDownloaded",
                            merchantReference = item.MerchantReference,
                            sku = item.Sku,
                            copies = item.Copies,
                            sizing = item.Sizing,
                            thumbnailUrl = (string?)null,
                            attributes = item.Attributes,
                            assets,
                            recipientCost = (string?)null,
                            correlationIdentifier = "23989788686705152"
                        });
                    }

                    var responseMessage = new ResponseMessage
                    {
                        StatusCode = 200,
                        Headers = new Dictionary<string, WireMockList<string>> { { "Content-Type", new WireMockList<string>("application/json") } },
                        BodyData = new BodyData
                        {
                            DetectedBodyType = BodyType.Json,
                            BodyAsJson = new
                            {
                                outcome = "UnexpectedOutcome", // this is the unexpected outcome value
                                order = new
                                {
                                    id = "ord_1234567",
                                    created = "2023-10-16T14:14:51.02Z",
                                    lastUpdated = "2023-10-16T14:14:51.7746508Z",
                                    callbackUrl = request!.CallbackUrl,
                                    merchantReference = request.MerchantReference,
                                    shippingMethod = request.ShippingMethod,
                                    idempotencyKey = request.IdempotencyKey,
                                    status = new
                                    {
                                        stage = "InProgress",
                                        issues = Array.Empty<string>(),
                                        details = new
                                        {
                                            downloadAssets = "NotStarted",
                                            printReadyAssetsPrepared = "NotStarted",
                                            allocateProductionLocation = "NotStarted",
                                            inProduction = "NotStarted",
                                            shipping = "NotStarted"
                                        }
                                    },
                                    charges = Array.Empty<string>(),
                                    shipments = Array.Empty<string>(),
                                    recipient = new
                                    {
                                        name = request.Recipient.Name,
                                        email = request.Recipient.Email,
                                        phoneNumber = request.Recipient.PhoneNumber,
                                        address = new
                                        {
                                            line1 = request.Recipient.Address.Line1,
                                            line2 = request.Recipient.Address.Line2,
                                            postalOrZipCode = request.Recipient.Address.PostalOrZipCode,
                                            countryCode = request.Recipient.Address.CountryCode,
                                            townOrCity = request.Recipient.Address.TownOrCity,
                                            stateOrCounty = request.Recipient.Address.StateOrCounty
                                        }
                                    },
                                    items,
                                    packingSlip = (string?)null,
                                    metadata = request.Metadata
                                },
                                traceParent = "sent_from_mock_ProdigiPrintApiServer"
                            }
                        }
                    };

                    return Task.FromResult(responseMessage);
                }
            ).WithTransformer()
        );
    }
    
    public void Dispose()
    {
        server!.Stop();
        server.Dispose();
    }
}
