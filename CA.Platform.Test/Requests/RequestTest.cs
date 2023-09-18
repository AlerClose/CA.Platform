using CA.Platform.Test.Database;
using CA.Platform.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace CA.Platform.Test;

public class RequestTest: BaseTest<DataContext>
{
    protected override void SetupInternal(IServiceCollection services)
    {
        base.SetupInternal(services);
        AddAssembly(GetType().Assembly);
    }

    [Test]
    public async Task ShouldWorkGetRequest()
    {
        var result = await Mediator.Send(new TestRequestGet()
        {
            Test = "data"
        });
        Assert.IsTrue("data" == result);
    }
}