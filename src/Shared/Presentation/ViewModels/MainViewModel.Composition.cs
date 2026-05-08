using MyApp.Services;

namespace MyApp.Presentation.ViewModels;

// Runtime composition root — kept in its own file so the unit-test project can
// compile MainViewModel.cs without dragging in SelectService (and its AutoCAD
// dependencies). Tests use the ISelectService-taking constructor with a fake.
public partial class MainViewModel
{
    public MainViewModel() : this(new SelectService())
    {
    }
}
