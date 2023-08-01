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
}
