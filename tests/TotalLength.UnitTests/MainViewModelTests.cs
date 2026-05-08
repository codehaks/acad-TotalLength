using System;
using System.Collections.Generic;
using System.ComponentModel;
using MyApp.Core;
using MyApp.Presentation.ViewModels;
using MyApp.Services;
using Xunit;

namespace MyApp.UnitTests;

public class MainViewModelTests
{
    private sealed class FakeSelectService : ISelectService
    {
        public SelectOptions? LastOptions { get; private set; }
        public (int SelectedCount, double TotalLength, string Message) Result { get; set; }
            = (0, 0.0, string.Empty);

        public (int SelectedCount, double TotalLength, string Message) SelectObjects(SelectOptions selectOptions)
        {
            LastOptions = selectOptions;
            return Result;
        }
    }

    [Fact]
    public void Constructor_NullSelectService_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(null!));
    }

    [Fact]
    public void Constructor_DefaultsAllSelectionFlagsTrue()
    {
        var vm = new MainViewModel(new FakeSelectService());

        Assert.True(vm.SelectOptions.SelectLines);
        Assert.True(vm.SelectOptions.SelectPolyLines);
        Assert.True(vm.SelectOptions.SelectArcs);
    }

    [Fact]
    public void Constructor_InitialStatusMessage_IsStarted()
    {
        var vm = new MainViewModel(new FakeSelectService());

        Assert.Equal("Started", vm.StatusMessage);
    }

    [Fact]
    public void SelectCommand_DelegatesToSelectServiceWithCurrentOptions()
    {
        var fake = new FakeSelectService();
        var vm = new MainViewModel(fake);
        vm.SelectOptions = new SelectOptions { SelectArcs = true };

        vm.SelectCommand.Execute(null);

        Assert.Same(vm.SelectOptions, fake.LastOptions);
    }

    [Fact]
    public void SelectCommand_PopulatesResultProperties()
    {
        var fake = new FakeSelectService
        {
            Result = (3, 12.345678, "ok"),
        };
        var vm = new MainViewModel(fake);

        vm.SelectCommand.Execute(null);

        Assert.Equal(3, vm.SelectedObjectCount);
        Assert.Equal(12.35, vm.TotalLength);
        Assert.Equal("ok", vm.StatusMessage);
    }

    [Fact]
    public void SelectCommand_RaisesPropertyChangedForResultProperties()
    {
        var fake = new FakeSelectService { Result = (1, 2.0, "msg") };
        var vm = new MainViewModel(fake);
        var changes = new List<string?>();
        vm.PropertyChanged += (_, e) => changes.Add(e.PropertyName);

        vm.SelectCommand.Execute(null);

        Assert.Contains(nameof(MainViewModel.SelectedObjectCount), changes);
        Assert.Contains(nameof(MainViewModel.TotalLength), changes);
        Assert.Contains(nameof(MainViewModel.StatusMessage), changes);
    }

    [Fact]
    public void SelectOptions_Setter_RaisesPropertyChanged()
    {
        var vm = new MainViewModel(new FakeSelectService());
        string? raised = null;
        vm.PropertyChanged += (_, e) => raised = e.PropertyName;

        vm.SelectOptions = new SelectOptions();

        Assert.Equal(nameof(MainViewModel.SelectOptions), raised);
    }

    [Fact]
    public void CancelCommand_NullWindow_DoesNotThrow()
    {
        var vm = new MainViewModel(new FakeSelectService());

        // Cancel uses optional Window parameter; null is the test path.
        vm.CancelCommand.Execute(null);
    }
}
