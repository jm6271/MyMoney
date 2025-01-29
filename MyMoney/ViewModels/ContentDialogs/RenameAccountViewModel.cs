namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class RenameAccountViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _NewName = string.Empty;
    }
}
