namespace PhotoPortfolio.Tests.e2e.Pages;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class GalleryIndexTests : LocalBlazorTestBase
{
    [Test]
    public async Task GalleryIndexPage_ShouldHaveExpectedTitle()
    {
        //await Page.SetViewportSizeAsync(1920, 1080);
        //await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri + "gallery");

        await Expect(Page).ToHaveTitleAsync(new Regex("PhotoPortfolio"));
        await Expect(Page).ToHaveURLAsync(new Regex("/gallery"));
    }

    [Test]
    public async Task GalleryIndexPage_ShouldShowListOfGalleries()
    {
        //await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri + "gallery");

        var galleries = Page.Locator("#gallery-container");
        await Expect(galleries).ToHaveCountAsync(3); // public galleries from GalleryList
    }

    [Test]
    public async Task GalleryIndexPage_ShouldGoToSpecificGalleryWhenSeeMoreLinkClicked()
    {
        //await Page.PauseAsync();

        await Page.GotoAsync(RootUri.AbsoluteUri);

        var galleryName = "Landscapes";

        await Page.GetByRole(AriaRole.Link, new() { Name = "Gallery", Exact = true }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = galleryName })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "See More ➜" }).First.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/galleries/"));

        await Expect(Page.Locator("div").Filter(new() { HasText = $"Gallery {galleryName}" }).Nth(3)).ToBeVisibleAsync();

        await Expect(Page.Locator("#text-block-content")).ToHaveTextAsync(galleryName);
    }
}
