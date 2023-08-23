namespace Stl.Tests;

public class ResultTest(ITestOutputHelper @out) : TestBase(@out)
{
    [Fact]
    public void ErrorTest()
    {
        var r = (Result<bool>)Result.Error(typeof(bool), new InvalidOperationException("Test"));
        r.HasError.Should().BeTrue();
        r.Error.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("Test");
    }
}
