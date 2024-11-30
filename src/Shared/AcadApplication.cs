using Autodesk.AutoCAD.Runtime;

namespace MyApp;

public class AcadApplication : IExtensionApplication
{
    public void Initialize()
    {
        RibbonManager.AddRibbons();
    }

    public void Terminate()
    {
    }
}