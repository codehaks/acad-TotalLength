using MyApp.Core;

namespace MyApp.Services;

public interface ISelectService
{
    (int SelectedCount, double TotalLength, string Message) SelectObjects(SelectOptions selectOptions);
}
