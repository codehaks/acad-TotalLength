using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using MyApp.Common;
using MyApp.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;


namespace MyApp.Presentation.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly SelectService _selectService;

    public MainViewModel()
    {
        OkCommand = new RelayCommand<Window>(ExecuteOkCommand, CanExecuteOkCommand);
        CancelCommand = new RelayCommand<Window>(ExecuteCancelCommand);

        SelectCommand = new RelayCommand<object>(ExecuteSelectCommand);

        _statusMessage = "Started";
        _selectService = new SelectService();
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
        var (count, length, message) = _selectService.SelectObjects();
        SelectedObjectCount = count;
        TotalLength = length;
        StatusMessage = message;
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