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
        var photoId = "64e3bd8f003140c790b1d2e5";
        await Page.GotoAsync($"{RootUri.AbsoluteUri}photo/{photoId}");

        var image = Page.Locator("#photo-image");
        await Expect(image).ToBeVisibleAsync();
    }
}
