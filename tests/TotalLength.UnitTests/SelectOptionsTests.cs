using MyApp.Core;
using Xunit;

namespace MyApp.UnitTests;

public class SelectOptionsTests
{
    [Fact]
    public void ToString_AllFlagsFalse_ReturnsEmpty()
    {
        var options = new SelectOptions();

        Assert.Equal(string.Empty, options.ToString());
    }

    [Fact]
    public void ToString_OnlyLines_ReturnsLine()
    {
        var options = new SelectOptions { SelectLines = true };

        Assert.Equal("LINE", options.ToString());
    }

    [Fact]
    public void ToString_OnlyPolyLines_ReturnsLwPolyline()
    {
        var options = new SelectOptions { SelectPolyLines = true };

        Assert.Equal("LWPOLYLINE", options.ToString());
    }

    [Fact]
    public void ToString_OnlyArcs_ReturnsArc()
    {
        var options = new SelectOptions { SelectArcs = true };

        Assert.Equal("ARC", options.ToString());
    }

    [Fact]
    public void ToString_AllFlagsTrue_JoinsWithComma()
    {
        var options = new SelectOptions
        {
            SelectLines = true,
            SelectPolyLines = true,
            SelectArcs = true,
        };

        Assert.Equal("LINE,LWPOLYLINE,ARC", options.ToString());
    }

    [Fact]
    public void ToString_LinesAndArcs_OmitsPolylines()
    {
        var options = new SelectOptions
        {
            SelectLines = true,
            SelectArcs = true,
        };

        Assert.Equal("LINE,ARC", options.ToString());
    }
}
