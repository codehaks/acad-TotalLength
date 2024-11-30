using Autodesk.AutoCAD.Runtime;
using MyApp.Presentation.Views;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace MyApp.Commands;

public class AcadCommand
{
    [CommandMethod("ShowMainWindow")]
    public void ShowMainWindow()
    {
        var window = new MainWindow();
        new System.Windows.Interop.WindowInteropHelper(window)
            .Owner = Application.MainWindow.Handle;
        window.ShowDialog();
    }
}