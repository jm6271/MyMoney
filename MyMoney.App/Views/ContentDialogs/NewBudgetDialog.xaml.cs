using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using MyMoney.App.ViewModels.ContentDialogs;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MyMoney.App.Views.ContentDialogs
{
    public sealed partial class NewBudgetDialog : ContentDialog
    {
        NewBudgetDialogViewModel ViewModel;
        public NewBudgetDialog()
        {
            this.InitializeComponent();
            ViewModel = new();
            DataContext = ViewModel;
        }

        public ComboBox CmbBudgetDates => cmbBudgetDates;
        public CheckBox ChkUseLastMonthBudget => chkUseLastMonthBudget;
    }
}
