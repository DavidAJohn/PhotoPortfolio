using Microsoft.Playwright;

namespace PhotoPortfolio.Tests.Integration.Client.Pages;

[Collection("Test collection")]
public class IndexTests
{
    private readonly SharedTestContext _testContext;

    public IndexTests(SharedTestContext testContext)
    {
        _testContext = testContext;
    }

    [SkippableFact]
    public async Task IndexPage_ShouldHaveExpectedTitle()
    {
        Skip.If(true, "This test is duplicated in the e2e project");

        var page = await _testContext.Browser.NewPageAsync();

        await page.PauseAsync();
        await page.GotoAsync(SharedTestContext.AppUrl);

        var title = await page.TitleAsync();

        //Assert.Equal("PhotoPortfolio", title);
        title.Should().MatchRegex("PhotoPortfolio");
    }

    [SkippableFact]
    public async Task IndexPage_ClickingGalleryLink_ShouldGoToGalleryPage()
    {
        Skip.If(true, "This test is duplicated in the e2e project");

        var page = await _testContext.Browser.NewPageAsync();

        //await page.PauseAsync();
        await page.GotoAsync(SharedTestContext.AppUrl);

        var galleryLink = page.GetByRole(AriaRole.Link, new() { Name = "Gallery", Exact = true });
        var href = await galleryLink.GetAttributeAsync("href");
        href.Should().MatchRegex("gallery");
        await galleryLink.ClickAsync();

        //Assert.Equal(SharedTestContext.AppUrl + "/gallery", page.Url);
        page.Url.Should().EndWith("/gallery");
    }

    [SkippableFact]
    public async Task IndexPage_ClickingAboutLink_ShouldGoToAboutPage()
    {
        Skip.If(true, "This test is duplicated in the e2e project");

        var page = await _testContext.Browser.NewPageAsync();

        //await page.PauseAsync();
        await page.GotoAsync(SharedTestContext.AppUrl);

        var galleryLink = page.GetByRole(AriaRole.Link, new() { Name = "More About Me", Exact = true });
        var href = await galleryLink.GetAttributeAsync("href");
        href.Should().MatchRegex("about");
        await galleryLink.ClickAsync();

        //Assert.Equal(SharedTestContext.AppUrl + "/about", page.Url);
        page.Url.Should().EndWith("/about");
    }

    [SkippableFact]
    public async Task IndexPage_BasketShouldInitiallyBeZero()
    {
        Skip.If(true, "This test is duplicated in the e2e project");

        var page = await _testContext.Browser.NewPageAsync();

        //await page.PauseAsync();
        await page.GotoAsync(SharedTestContext.AppUrl);

        var basket = page.Locator("span#cart-items");
        var text = await basket.TextContentAsync();
        text.Should().Be("0");
    }

    [SkippableFact]
    public async Task IndexPage_ClickingBasket_ShouldShowEmptyBasketDropdown()
    {
        Skip.If(true, "This test is duplicated in the e2e project");

        var page = await _testContext.Browser.NewPageAsync();

        //await page.PauseAsync();
        await page.GotoAsync(SharedTestContext.AppUrl);

        var basket = page.GetByText("Basket:");
        await basket.ClickAsync();

        var basketDropdown = page.Locator("div[role=menu]>div>span");
        var basketDropdownText = await basketDropdown.TextContentAsync();
        basketDropdownText.Should().Be("Your basket is currently empty");
    }
}
