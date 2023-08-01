namespace PhotoPortfolio.Tests.e2e.Infrastructure;

public class ExternalTestBase : PageTest
{
    protected static readonly Uri RootUri = new("[uri of your test environment]");

    public ExternalTestBase()
    {
    }
}
