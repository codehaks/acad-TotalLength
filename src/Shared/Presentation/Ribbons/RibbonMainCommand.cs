using System;

namespace MyApp.Presentation.Ribbons;

// Class that handles the command to be invoked when the button is clicked
public class RibbonMainCommand : System.Windows.Input.ICommand
{
    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter)
    {
        // Invoke your custom AutoCAD command here
        Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute("ShowMainWindow ", true, false, false);
    }

    public event EventHandler CanExecuteChanged;
}
