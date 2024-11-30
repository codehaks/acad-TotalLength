using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using MyApp.Common;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;


namespace MyApp.Presentation.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public MainViewModel()
    {
        OkCommand = new RelayCommand<Window>(ExecuteOkCommand, CanExecuteOkCommand);
        CancelCommand = new RelayCommand<Window>(ExecuteCancelCommand);

        SelectCommand = new RelayCommand<object>(ExecuteSelectCommand);

        _statusMessage = "Started";
    }

    // --- Properties --------------------------------------------------------

    #region Title
    private string _title = "ZLength 1.0";

    public string Title
    {
        get
        {
#if DEBUG
            return _title + $" - [{Version}]";
#else
            return _title;
#endif
        }
        set
        {
            _title = value;

            OnPropertyChanged();
        }
    }

    public string Version
    {
        get => DateTime.Now.ToString("HH-mm-ss");
    }
    #endregion

    #region Status
    private string _statusMessage;

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }
    #endregion

    // --- Commands ----------------------------------------------------------

    #region Select Command

    public ICommand SelectCommand { get; }

    private int _selectedObjectCount;

    public int SelectedObjectCount
    {
        get => _selectedObjectCount;
        set
        {
            _selectedObjectCount = value;
            OnPropertyChanged();
        }
    }

    private double _totalLength;

    public double TotalLength
    {
        get => _totalLength;
        set
        {
            _totalLength = value;
            OnPropertyChanged();
        }
    }



    private void ExecuteSelectCommand(object parameter)
    {
        try
        {
            // Get the current document and editor
            var document = Application.DocumentManager.MdiActiveDocument;
            var editor = document.Editor;

            // Define a selection filter for specific object types
            var filter = new SelectionFilter(new[]
            {
                new TypedValue((int)DxfCode.Start, "LINE,ARC,LWPOLYLINE")
            });

            // Prompt the user to select objects
            var result = editor.GetSelection(filter);

            if (result.Status == PromptStatus.OK)
            {
                var selectedObjects = result.Value;
                SelectedObjectCount = selectedObjects.Count;

                // Calculate total length
                double totalLength = 0.0;

                using (var transaction = document.TransactionManager.StartTransaction())
                {
                    foreach (var id in selectedObjects.GetObjectIds())
                    {
                        var entity = transaction.GetObject(id, OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Entity;

                        if (entity is Autodesk.AutoCAD.DatabaseServices.Curve curve)
                        {
                            totalLength += curve.GetDistanceAtParameter(curve.EndParam) - curve.GetDistanceAtParameter(curve.StartParam);
                        }
                    }

                    transaction.Commit();
                }

                TotalLength = totalLength;
                StatusMessage = $"Selected {SelectedObjectCount} objects. Total Length: {TotalLength:F2}";
            }
            else
            {
                StatusMessage = "No objects selected.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
    #endregion

    #region OK Command
    public ICommand OkCommand { get; }

    private void ExecuteOkCommand(Window window)
    {
        // Add your custom logic here
        StatusMessage = "OK Command Executed!";
        MessageBox.Show("OK button clicked. The window will now close.");

        // Close the window
        window?.Close();
    }

    private bool CanExecuteOkCommand(Window window)
    {
        // Add any condition to enable or disable the OK button
        return true; // Always enabled for now
    }
    #endregion

    #region Cancel Command
    public ICommand CancelCommand { get; }
    private void ExecuteCancelCommand(Window window)
    {
        // Close the window
        window?.Close();
    }
    #endregion

    // --- Events ------------------------------------------------------------

    #region Property Changed Event
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}