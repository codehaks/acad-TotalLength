using MyApp.Common;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MyApp.Presentation.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public MainViewModel()
    {
        OkCommand = new RelayCommand<Window>(ExecuteOkCommand, CanExecuteOkCommand);
        CancelCommand = new RelayCommand<Window>(ExecuteCancelCommand);
    }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    private string _title = "My Plugin";

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
        get => DateTime.Now.ToString("G");
    }

    // Example: Property to update when "OK" is pressed
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

    private void ExecuteCancelCommand(Window window)
    {
        // Close the window
        window?.Close();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}