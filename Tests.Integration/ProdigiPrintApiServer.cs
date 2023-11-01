using PhotoPortfolio.Shared.Models.Prodigi.Orders;
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

        server
            .Given(Request.Create()
            .WithPath("/quotes")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-created") // generates a "Created" outcome in response
            //.WithBody(new JsonMatcher(@"{
            //    ""shippingMethod"": ""Standard"",
            //    ""destinationCountryCode"": ""GB"",
            //    ""currencyCode"": ""GBP"",
            //    ""items"": [
            //        {
            //            ""sku"": ""ECO-CAN-16x24"",
            //            ""copies"": 1,
            //            ""attributes"": {
            //                ""wrap"" : ""MirrorWrap""
            //            },
            //            ""assets"": [
            //                {
            //                ""printArea"": ""default""
            //                }
            //            ]
            //        }
            //    ]
            //}", true))
            .UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody(GenerateQuoteCreatedResponseBody())
        );

        server
            .Given(Request.Create()
            .WithPath("/quotes")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-createdwithissues")
            //.WithBody(new JsonPartialWildcardMatcher(@"{
            //    ""shippingMethod"": ""ReturnCreatedWithIssues"",
            //    ""destinationCountryCode"": ""GB"",
            //    ""currencyCode"": ""GBP"",
            //    ""items"": [
            //        {
            //            ""sku"": ""eco-can-16x24"",
            //            ""copies"": 1,
            //            ""attributes"": {
            //                ""wrap"" : ""MirrorWrap""
            //            },
            //            ""assets"": [
            //                {
            //                ""printArea"": ""default""
            //                }
            //            ]
            //        }
            //    ]
            //}", true))
            .UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody(GenerateQuoteCreatedWithIssuesResponseBody())
        );


        // "/orders" endpoint responses
        //

        server
            .Given(Request.Create()
            .WithPath("/orders")
            .WithHeader("X-API-Key", "00000000-0000-0000-0000-created") // generates a "Created" outcome in response
            //.WithBody(new JsonMatcher(@"{
            //    ""callbackUrl"": ""https://localhost:7200/callbacks"",
            //    ""merchantReference"": ""MyMerchantReference940e45"",
            //    ""shippingMethod"": ""Standard"",
            //    ""idempotencyKey"": ""650067efd4547ce468940e45"",
            //    ""recipient"": {
            //        ""address"": {
            //            ""line1"": ""1 Test Place"",
            //            ""line2"": ""Testville"",
            //            ""postalOrZipCode"": ""N1 2EF"",
            //            ""countryCode"": ""GB"",
            //            ""townOrCity"": ""Testington"",
            //            ""stateOrCounty"": null
            //        },
            //        ""name"": ""Mr Test"",
            //        ""email"": ""test@test.com"",
            //        ""phoneNumber"": ""440000000000""
            //    },
            //    ""items"": [
            //        {
            //            ""merchantReference"": ""MyItemId"",
            //            ""sku"": ""global-fap-16x24"",
            //            ""copies"": 1,
            //            ""sizing"": ""fillPrintArea"",
            //            ""attributes"": {
            //            },
            //            ""assets"": [
            //                {
            //                    ""printArea"": ""default"",
            //                    ""url"": ""https://photoportfolioimgs.blob.core.windows.net/repo/DavidAJohn_SevernBridge.jpg""
            //                }
            //            ]
            //        }
            //    ]
            //}", true))
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
            })
            .WithTransformer()
        );

        server
           .Given(Request.Create()
           .WithPath("/orders")
           .WithHeader("X-API-Key", "00000000-0000-0000-0000-createdwithissues") // generates a "CreatedWithIssues" outcome in response
           //.WithBody(new JsonMatcher(@"{
           //     ""callbackUrl"": ""https://localhost:7200/callbacks"",
           //     ""merchantReference"": ""MyMerchantReference940e45"",
           //     ""shippingMethod"": ""Standard"",
           //     ""idempotencyKey"": ""650067efd4547ce468940e45"",
           //     ""recipient"": {
           //         ""address"": {
           //             ""line1"": ""1 Test Place"",
           //             ""line2"": ""Testville"",
           //             ""postalOrZipCode"": ""N1 2EF"",
           //             ""countryCode"": ""GB"",
           //             ""townOrCity"": ""Testington"",
           //             ""stateOrCounty"": null
           //         },
           //         ""name"": ""Mr Test"",
           //         ""email"": ""test@test.com"",
           //         ""phoneNumber"": ""440000000000""
           //     },
           //     ""items"": [
           //         {
           //             ""merchantReference"": ""MyItemId"",
           //             ""sku"": ""global-fap-16x24"",
           //             ""copies"": 1,
           //             ""sizing"": ""fillPrintArea"",
           //             ""attributes"": {
           //             },
           //             ""assets"": [
           //                 {
           //                     ""printArea"": ""default"",
           //                     ""url"": ""https://photoportfolioimgs.blob.core.windows.net/repo/DavidAJohn_SevernBridge.jpg""
           //                 }
           //             ]
           //         }
           //     ]
           // }", true))
           .UsingPost())
           .RespondWith(Response.Create().WithStatusCode(200)
           .WithHeader("Content-Type", "application/json")
           .WithBody(
                GenerateOrderCreatedWithIssuesResponseBody(
                    "{{ JsonPath.SelectToken request.body \"$.idempotencyKey\" }}"
                )
            )
            .WithTransformer()
       );

       server
           .Given(Request.Create()
           .WithPath("/orders")
           .WithHeader("X-API-Key", "00000000-0000-0000-0000-alreadyexists") // generates an "AlreadyExists" outcome in response
           //.WithBody(new JsonMatcher(@"{
           //     ""callbackUrl"": ""https://localhost:7200/callbacks"",
           //     ""merchantReference"": ""MyMerchantReference940e45"",
           //     ""shippingMethod"": ""Standard"",
           //     ""idempotencyKey"": ""650067efd4547ce468940e45"",
           //     ""recipient"": {
           //         ""address"": {
           //             ""line1"": ""1 Test Place"",
           //             ""line2"": ""Testville"",
           //             ""postalOrZipCode"": ""N1 2EF"",
           //             ""countryCode"": ""GB"",
           //             ""townOrCity"": ""Testington"",
           //             ""stateOrCounty"": null
           //         },
           //         ""name"": ""Mr Test"",
           //         ""email"": ""test@test.com"",
           //         ""phoneNumber"": ""440000000000""
           //     },
           //     ""items"": [
           //         {
           //             ""merchantReference"": ""MyItemId"",
           //             ""sku"": ""global-fap-16x24"",
           //             ""copies"": 1,
           //             ""sizing"": ""fillPrintArea"",
           //             ""attributes"": {
           //             },
           //             ""assets"": [
           //                 {
           //                     ""printArea"": ""default"",
           //                     ""url"": ""https://photoportfolioimgs.blob.core.windows.net/repo/DavidAJohn_SevernBridge.jpg""
           //                 }
           //             ]
           //         }
           //     ]
           // }", true))
           .UsingPost())
           .RespondWith(Response.Create().WithStatusCode(200)
           .WithHeader("Content-Type", "application/json")
           .WithBody(
                GenerateOrderAlreadyExistsResponseBody(
                    "{{ JsonPath.SelectToken request.body \"$.idempotencyKey\" }}"
                )
            )
            .WithTransformer()
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
           .RespondWith(Response.Create().WithStatusCode(200)
           .WithHeader("Content-Type", "application/json")
           .WithBody(
                GenerateOrderCreatedWithUnexpectedOutcomeResponse(
                    "{{ JsonPath.SelectToken request.body \"$.idempotencyKey\" }}"
                )
            )
            .WithTransformer()
       );
    }
    
    public void Dispose()
    {
        server!.Stop();
        server.Dispose();
    }

    // Static body creation methods
    //
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

    //private static string GenerateOrderCreatedResponseBody(Order request)
    //{
    //    var json = JsonSerializer.Serialize(request);

    //    return @"{
    //        ""outcome"": ""Created"",
    //        ""order"": {
    //            ""id"": ""ord_1103294"",
    //            ""created"": ""2023-10-16T14:14:51.02Z"",
    //            ""lastUpdated"": ""2023-10-16T14:14:51.7746508Z"",
    //            ""callbackUrl"": """ + request.CallbackUrl + @""",
    //            ""merchantReference"": """ + request.MerchantReference + @""",
    //            ""shippingMethod"": """ + request.ShippingMethod + @""",
    //            ""idempotencyKey"": """ + request.IdempotencyKey + @""",
    //            ""status"": {
    //                ""stage"": ""InProgress"",
    //                ""issues"": [],
    //                ""details"": {
    //                    ""downloadAssets"": ""NotStarted"",
    //                    ""printReadyAssetsPrepared"": ""NotStarted"",
    //                    ""allocateProductionLocation"": ""NotStarted"",
    //                    ""inProduction"": ""NotStarted"",
    //                    ""shipping"": ""NotStarted""
    //                }
    //            },
    //            ""charges"": [],
    //            ""shipments"": [],
    //            ""recipient"": {
    //                ""name"": """ + request.Recipient.Name + @""",
    //                ""email"": ""test@test.com"",
    //                ""phoneNumber"": ""440000000000"",
    //                ""address"": {
    //                    ""line1"": ""1 Test Place"",
    //                    ""line2"": ""Testville"",
    //                    ""postalOrZipCode"": ""N1 2EF"",
    //                    ""countryCode"": ""GB"",
    //                    ""townOrCity"": ""Testington"",
    //                    ""stateOrCounty"": null
    //                }
    //            },
    //            ""items"": [
    //                {
    //                    ""id"": ""ori_1426359"",
    //                    ""status"": ""NotYetDownloaded"",
    //                    ""merchantReference"": ""MyItemId"",
    //                    ""sku"": ""GLOBAL-FAP-16X24"",
    //                    ""copies"": 1,
    //                    ""sizing"": ""fillPrintArea"",
    //                    ""thumbnailUrl"": null,
    //                    ""attributes"": {},
    //                    ""assets"": [
    //                        {
    //                            ""id"": ""ast_189"",
    //                            ""printArea"": ""default"",
    //                            ""md5Hash"": null,
    //                            ""url"": ""https://photoportfolioimgs.blob.core.windows.net/repo/DavidAJohn_SevernBridge.jpg"",
    //                            ""thumbnailUrl"": null,
    //                            ""status"": ""InProgress""
    //                        }
    //                    ],
    //                    ""recipientCost"": null,
    //                    ""correlationIdentifier"": ""23989788686705152""
    //                }
    //            ],
    //            ""packingSlip"": null,
    //            ""metadata"": {
    //                ""mycustomkey"": ""some-guid""
    //            }
    //        },
    //        ""traceParent"": ""sent_from_mock_ProdigiPrintApiServer""
    //    }";
    //}

    private static string GenerateOrderCreatedWithIssuesResponseBody(string idempotencyKey)
    {
        return @"{
            ""outcome"": ""CreatedWithIssues"",
            ""order"": {
                ""id"": ""ord_1103294"",
                ""created"": ""2023-10-16T14:14:51.02Z"",
                ""lastUpdated"": ""2023-10-16T14:14:51.7746508Z"",
                ""callbackUrl"": ""https://localhost:7200/callbacks"",
                ""merchantReference"": ""MyMerchantReference940e45"",
                ""shippingMethod"": ""Standard"",
                ""idempotencyKey"": """ + idempotencyKey + @""",
                ""status"": {
                    ""stage"": ""InProgress"",
                    ""issues"": [
                        {
                            ""objectId"": ""ori_12345"",
                            ""errorCode"" : ""items.assets.NotDownloaded"",
                            ""description"" : ""Warning: Download attempt 1 of 10 failed for 'default' asset on item 'ori_12345' at location 'http://source.url' ""
                        },
                        {
                            ""objectId"": ""ord_829398"",
                            ""errorCode"": ""RequiresPaymentAuthorisation"",
                            ""description"": ""Payment authorisation required for 'ord_829398' (195.02USD) please use the following URL to make payment: https://beta-dashboard.pwinty.com/payment/97323"",
                            ""authorisationDetails"": {
                                ""authorisationUrl"": ""https://beta-dashboard.pwinty.com/payment/97323"",
                                ""paymentDetails"": {
                                    ""amount"": ""195.02"",
                                    ""currency"": ""USD""
                                }
                            }
                        }
                    ],
                    ""details"": {
                        ""downloadAssets"": ""NotStarted"",
                        ""printReadyAssetsPrepared"": ""NotStarted"",
                        ""allocateProductionLocation"": ""NotStarted"",
                        ""inProduction"": ""NotStarted"",
                        ""shipping"": ""NotStarted""
                    }
                },
                ""charges"": [],
                ""shipments"": [],
                ""recipient"": {
                    ""name"": ""Mr Test"",
                    ""email"": ""test@test.com"",
                    ""phoneNumber"": ""440000000000"",
                    ""address"": {
                        ""line1"": ""1 Test Place"",
                        ""line2"": ""Testville"",
                        ""postalOrZipCode"": ""N1 2EF"",
                        ""countryCode"": ""GB"",
                        ""townOrCity"": ""Testington"",
                        ""stateOrCounty"": null
                    }
                },
                ""items"": [
                    {
                        ""id"": ""ori_1426359"",
                        ""status"": ""NotYetDownloaded"",
                        ""merchantReference"": ""MyItemId"",
                        ""sku"": ""GLOBAL-FAP-16X24"",
                        ""copies"": 1,
                        ""sizing"": ""fillPrintArea"",
                        ""thumbnailUrl"": null,
                        ""attributes"": {},
                        ""assets"": [
                            {
                                ""id"": ""ast_189"",
                                ""printArea"": ""default"",
                                ""md5Hash"": null,
                                ""url"": ""https://photoportfolioimgs.blob.core.windows.net/repo/DavidAJohn_SevernBridge.jpg"",
                                ""thumbnailUrl"": null,
                                ""status"": ""InProgress""
                            }
                        ],
                        ""recipientCost"": null,
                        ""correlationIdentifier"": ""23989788686705152""
                    }
                ],
                ""packingSlip"": null,
                ""metadata"": {
                    ""mycustomkey"": ""some-guid""
                }
            },
            ""traceParent"": ""sent_from_mock_ProdigiPrintApiServer""
        }";
    }

    private static string GenerateOrderAlreadyExistsResponseBody(string idempotencyKey)
    {
        return @"{
            ""outcome"": ""AlreadyExists"",
            ""order"": {
                ""id"": ""ord_1103294"",
                ""created"": ""2023-10-16T14:14:51.02Z"",
                ""lastUpdated"": ""2023-10-16T14:14:51.7746508Z"",
                ""callbackUrl"": ""https://localhost:7200/callbacks"",
                ""merchantReference"": ""MyMerchantReference940e45"",
                ""shippingMethod"": ""Standard"",
                ""idempotencyKey"": """ + idempotencyKey + @""",
                ""status"": {
                    ""stage"": ""Complete"",
                    ""issues"": [],
                    ""details"": {
                        ""downloadAssets"": ""Complete"",
                        ""printReadyAssetsPrepared"": ""Complete"",
                        ""allocateProductionLocation"": ""Complete"",
                        ""inProduction"": ""Complete"",
                        ""shipping"": ""Complete""
                    }
                },
                ""charges"": [
                    {
                        ""id"": ""chg_465100"",
                        ""prodigiInvoiceNumber"": null,
                        ""totalCost"": {
                            ""amount"": ""18.30"",
                            ""currency"": ""GBP""
                        },
                        ""totalTax"": {
                            ""amount"": ""3.05"",
                            ""currency"": ""GBP""
                        },
                        ""items"": [
                            {
                                ""id"": ""chi_992751"",
                                ""itemId"": null,
                                ""cost"": {
                                    ""amount"": ""5.25"",
                                    ""currency"": ""GBP""
                                },
                                ""shipmentId"": ""shp_665175"",
                                ""chargeType"": ""Shipping""
                            },
                            {
                                ""id"": ""chi_992753"",
                                ""itemId"": ""ori_1426359"",
                                ""cost"": {
                                    ""amount"": ""10.00"",
                                    ""currency"": ""GBP""
                                },
                                ""shipmentId"": null,
                                ""chargeType"": ""Item""
                            }
                        ]
                    }
                ],
                ""shipments"": [
                    {
                        ""id"": ""shp_665175"",
                        ""dispatchDate"": ""2023-10-16T14:29:10.283Z"",
                        ""carrier"": {
                            ""name"": ""EVRi"",
                            ""service"": ""Evri Standard 48""
                        },
                        ""fulfillmentLocation"": {
                            ""countryCode"": ""GB"",
                            ""labCode"": ""prodigi_gb3""
                        },
                        ""tracking"": {
                            ""number"": ""PH000000000GB"",
                            ""url"": ""https://www.royalmail.com/portal/rm/track?trackNumber=PH000000000GB""
                        },
                        ""items"": [
                            {
                                ""itemId"": ""ori_1426359""
                            }
                        ],
                        ""status"": ""Shipped""
                    }
                ],
                ""recipient"": {
                    ""name"": ""Mr Test"",
                    ""email"": ""test@test.com"",
                    ""phoneNumber"": ""440000000000"",
                    ""address"": {
                        ""line1"": ""1 Test Place"",
                        ""line2"": ""Testville"",
                        ""postalOrZipCode"": ""N1 2EF"",
                        ""countryCode"": ""GB"",
                        ""townOrCity"": ""Testington"",
                        ""stateOrCounty"": null
                    }
                },
                ""items"": [
                    {
                        ""id"": ""ori_1426359"",
                        ""status"": ""Ok"",
                        ""merchantReference"": ""MyItemId"",
                        ""sku"": ""GLOBAL-FAP-16X24"",
                        ""copies"": 1,
                        ""sizing"": ""fillPrintArea"",
                        ""thumbnailUrl"": ""https://pwintyimages.blob.core.windows.net/imagestorage-sandbox/1103294/665175/default-220709-thumbnail.jpg?skoid=9a8153f9-c101-46c8-90eb-53dee276678d&sktid=95af27ff-c62f-4dc3-b489-99265b850f61&skt=2023-10-16T17%3A00%3A10Z&ske=2023-10-22T17%3A05%3A10Z&sks=b&skv=2021-10-04&sv=2021-10-04&st=2023-10-16T17%3A12%3A52Z&se=2023-10-23T17%3A14%3A52Z&sr=b&sp=rw&sig=%2FieJN3REPdS4s7z7Ndw3Cark%2FVnh192KPuknq5okVeY%3D"",
                        ""attributes"": {},
                        ""assets"": [
                            {
                                ""id"": ""ast_189"",
                                ""printArea"": ""default"",
                                ""md5Hash"": ""8342859ab52073c4e01e581d7fc54d9e"",
                                ""url"": ""https://photoportfolioimgs.blob.core.windows.net/repo/DavidAJohn_SevernBridge.jpg"",
                                ""thumbnailUrl"": ""https://pwintyimages.blob.core.windows.net/imagestorage-sandbox/1103294/665175/default-220709-thumbnail.jpg?skoid=9a8153f9-c101-46c8-90eb-53dee276678d&sktid=95af27ff-c62f-4dc3-b489-99265b850f61&skt=2023-10-16T17%3A00%3A10Z&ske=2023-10-22T17%3A05%3A10Z&sks=b&skv=2021-10-04&sv=2021-10-04&st=2023-10-16T17%3A12%3A52Z&se=2023-10-23T17%3A14%3A52Z&sr=b&sp=rw&sig=%2FieJN3REPdS4s7z7Ndw3Cark%2FVnh192KPuknq5okVeY%3D"",
                                ""status"": ""Complete""
                            }
                        ],
                        ""recipientCost"": null,
                        ""correlationIdentifier"": ""23989788686705152""
                    }
                ],
                ""packingSlip"": null,
                ""metadata"": {
                    ""mycustomkey"": ""some-guid""
                }
            },
            ""traceParent"": ""sent_from_mock_ProdigiPrintApiServer""
        }";
    }

    private static string GenerateOrderCreatedWithUnexpectedOutcomeResponse(string idempotencyKey)
    {
        return @"{
            ""outcome"": ""UnexpectedOutcome"",
            ""order"": {
                ""id"": ""ord_1103294"",
                ""created"": ""2023-10-16T14:14:51.02Z"",
                ""lastUpdated"": ""2023-10-16T14:14:51.7746508Z"",
                ""callbackUrl"": ""https://localhost:7200/callbacks"",
                ""merchantReference"": ""MyMerchantReference940e45"",
                ""shippingMethod"": ""Standard"",
                ""idempotencyKey"": """ + idempotencyKey + @""",
                ""status"": {
                    ""stage"": ""InProgress"",
                    ""issues"": [],
                    ""details"": {
                        ""downloadAssets"": ""NotStarted"",
                        ""printReadyAssetsPrepared"": ""NotStarted"",
                        ""allocateProductionLocation"": ""NotStarted"",
                        ""inProduction"": ""NotStarted"",
                        ""shipping"": ""NotStarted""
                    }
                },
                ""charges"": [],
                ""shipments"": [],
                ""recipient"": {
                    ""name"": ""Mr Test"",
                    ""email"": ""test@test.com"",
                    ""phoneNumber"": ""440000000000"",
                    ""address"": {
                        ""line1"": ""1 Test Place"",
                        ""line2"": ""Testville"",
                        ""postalOrZipCode"": ""N1 2EF"",
                        ""countryCode"": ""GB"",
                        ""townOrCity"": ""Testington"",
                        ""stateOrCounty"": null
                    }
                },
                ""items"": [
                    {
                        ""id"": ""ori_1426359"",
                        ""status"": ""NotYetDownloaded"",
                        ""merchantReference"": ""MyItemId"",
                        ""sku"": ""GLOBAL-FAP-16X24"",
                        ""copies"": 1,
                        ""sizing"": ""fillPrintArea"",
                        ""thumbnailUrl"": null,
                        ""attributes"": {},
                        ""assets"": [
                            {
                                ""id"": ""ast_189"",
                                ""printArea"": ""default"",
                                ""md5Hash"": null,
                                ""url"": ""https://photoportfolioimgs.blob.core.windows.net/repo/DavidAJohn_SevernBridge.jpg"",
                                ""thumbnailUrl"": null,
                                ""status"": ""InProgress""
                            }
                        ],
                        ""recipientCost"": null,
                        ""correlationIdentifier"": ""23989788686705152""
                    }
                ],
                ""packingSlip"": null,
                ""metadata"": {
                    ""mycustomkey"": ""some-guid""
                }
            },
            ""traceParent"": ""sent_from_mock_ProdigiPrintApiServer""
        }";
    }
}
