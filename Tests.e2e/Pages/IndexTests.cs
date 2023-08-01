namespace PhotoPortfolio.Tests.e2e.Pages;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class IndexTests : LocalBlazorTestBase
{
    [Test]
    public async Task IndexPage_ShouldHaveExpectedTitle()
    {
        //await Page.SetViewportSizeAsync(1920, 1080);
        //await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri);

        await Expect(Page).ToHaveTitleAsync(new Regex("PhotoPortfolio"));
    }

    [Test]
    public async Task IndexPage_ClickingGalleryLink_ShouldGoToGalleryPage()
    {
        //await Page.SetViewportSizeAsync(1920, 1080);
        //await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri);

        var galleryLink = Page.GetByRole(AriaRole.Link, new() { Name = "Gallery", Exact = true });
        await Expect(galleryLink).ToHaveAttributeAsync("href", "/gallery");
        await galleryLink.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/gallery"));
    }

    [Test]
    public async Task IndexPage_ClickingAboutLink_ShouldGoToAboutPage()
    {
        //await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri);

        await Page.GetByRole(AriaRole.Link, new() { Name = "More About Me" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/about"));
    }

    [Test]
    public async Task IndexPage_BasketShouldInitiallyBeZero()
    {
        //await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri);

        var basket = Page.Locator("span#cart-items");
        await Expect(basket).ToHaveTextAsync("0");
    }

    [Test]
    public async Task IndexPage_ClickingBasket_ShouldShowEmptyBasketDropdown()
    {
        //await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri);

        await Page.GetByText("Basket:").ClickAsync();

        var basketDropdown = Page.GetByRole(AriaRole.Menu);
        await Expect(basketDropdown).ToBeVisibleAsync();

        var basketDropdownText = Page.Locator("div[role=menu]>div>span"); // too flaky?
        await Expect(basketDropdownText).ToHaveTextAsync("Your basket is currently empty");

        await Expect(Page.Locator("text=Your basket is currently empty")).ToBeVisibleAsync();
    }
}
