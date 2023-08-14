namespace PhotoPortfolio.Tests.e2e.Pages;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PhotoDetailsTests : LocalBlazorTestBase
{
    [Test]
    public async Task PhotoDetailsPage_ShouldShow404PageWhenIdNotSpecified()
    {
        //await Page.PauseAsync();

        await Page.GotoAsync($"{RootUri.AbsoluteUri}photo/");

        await Expect(Page.Locator("text=Page Not Found")).ToBeVisibleAsync();
    }

    [Test]
    public async Task PhotoDetailsPage_ShouldShowErrorWhenIdIsNotValid()
    {
        //await Page.PauseAsync();

        await Page.GotoAsync($"{RootUri.AbsoluteUri}photo/notanid");

        var errorBox = Page.GetByRole(AriaRole.Alert);
        await Expect(errorBox).ToBeVisibleAsync();
        await Expect(errorBox).ToHaveTextAsync(new Regex("Could not find"));
    }

    [Test]
    public async Task PhotoDetailsPage_ShouldShowPhotoWhenIdIsValid()
    {
        //await Page.PauseAsync();
        var photoId = "63c862cd2165ce1d017b83a8";
        await Page.GotoAsync($"{RootUri.AbsoluteUri}photo/{photoId}");

        var image = Page.Locator("img#photo-image");
        await Expect(image).ToBeVisibleAsync();
    }
}
