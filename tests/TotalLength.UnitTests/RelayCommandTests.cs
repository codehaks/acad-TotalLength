using System;
using MyApp.Common;
using Xunit;

namespace MyApp.UnitTests;

public class RelayCommandTests
{
    [Fact]
    public void Constructor_NullExecute_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RelayCommand<string>(null!));
    }

    [Fact]
    public void CanExecute_NoPredicate_ReturnsTrue()
    {
        var command = new RelayCommand<string>(_ => { });

        Assert.True(command.CanExecute("anything"));
    }

    [Fact]
    public void CanExecute_PredicateProvided_ForwardsParameter()
    {
        string? received = null;
        var command = new RelayCommand<string>(_ => { }, p =>
        {
            received = p;
            return p == "ok";
        });

        Assert.True(command.CanExecute("ok"));
        Assert.Equal("ok", received);
        Assert.False(command.CanExecute("nope"));
    }

    [Fact]
    public void Execute_InvokesActionWithParameter()
    {
        string? received = null;
        var command = new RelayCommand<string>(p => received = p);

        command.Execute("hello");

        Assert.Equal("hello", received);
    }

    [Fact]
    public void Execute_NullParameterForReferenceType_Allowed()
    {
        string? received = "untouched";
        var command = new RelayCommand<string>(p => received = p);

        command.Execute(null);

        Assert.Null(received);
    }
}
