using Microsoft.Win32;

namespace MyMoney.Services;

public interface IFileDialogService
{
    string? ShowSaveCsvFileDialog(string defaultFileName);
}

public class FileDialogService : IFileDialogService
{
    public string? ShowSaveCsvFileDialog(string defaultFileName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files|*.csv",
            DefaultExt = ".csv",
            AddExtension = true,
            Title = "Export transactions to CSV",
            FileName = defaultFileName,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
