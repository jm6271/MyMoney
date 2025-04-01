using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.ViewModels.ContentDialogs
{
    public partial class NewExpenseGroupDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _groupName = string.Empty;
    }
}
