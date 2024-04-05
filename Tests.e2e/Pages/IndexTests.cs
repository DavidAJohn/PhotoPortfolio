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
    }

    [Test]
    public async Task IndexPage_ClickingSpecifiedMasonryGridImageAndXButton_ShouldShowImageModalAndClose()
    {
        await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri);

        var imageContainer = Page.Locator("div#masonry-grid-container > div:nth-child(8)"); // container div must be clicked to show modal
        await imageContainer.ClickAsync();

        var modal = Page.Locator("div.blazored-modal-container");
        await Expect(modal).ToBeVisibleAsync();

        var dialog = Page.GetByRole(AriaRole.Dialog);
        var closeButton = dialog.GetByLabel("close"); // targets the 'x' button
        await Expect(closeButton).ToBeVisibleAsync();
        await closeButton.ClickAsync();
    }

    [Test]
    public async Task IndexPage_ClickingSpecifiedMasonryGridImageAndCloseButton_ShouldShowImageModalAndClose()
    {
        await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri);

        var imageContainer = Page.Locator("div#masonry-grid-container > div:nth-child(8)"); // container div must be clicked to show modal
        await imageContainer.ClickAsync();

        var modal = Page.Locator("div.blazored-modal-container");
        await Expect(modal).ToBeVisibleAsync();

        
        //var dialog = Page.GetByRole(AriaRole.Dialog);
        //var closeButton = dialog.GetByRole(AriaRole.Button, new() { Name = "Close", Exact = true });
        var closeButton = Page.Locator("div[role=dialog] button:has-text(\"Close\")");

        await Expect(closeButton).ToBeVisibleAsync();
        await closeButton.ClickAsync();
    }

    [Test]
    public async Task IndexPage_ClickingSpecifiedMasonryGridImageAndBuyPrintsButton_ShouldGoToPhotoDetailsPage()
    {
        await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri);

        var imageContainer = Page.Locator("div#masonry-grid-container > div:nth-child(8)"); // container div must be clicked to show modal
        await imageContainer.ClickAsync();

        var modal = Page.Locator("div.blazored-modal-container");
        await Expect(modal).ToBeVisibleAsync();

        var buyButton = Page.Locator("div[role=dialog] button:has-text(\"Buy\")");
        //var buyButton = Page.Locator("div[role=dialog] button.lightbox-confirm");
        await Expect(buyButton).ToBeVisibleAsync();
        await buyButton.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/photo/")); // PhotoDetails page
    }
}
