﻿using MyMoney.ViewModels.ContentDialogs;
using MyMoney.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace MyMoney.Views.ContentDialogs
{
    /// <summary>
    /// Interaction logic for NewTransactionDialog.xaml
    /// </summary>
    public partial class NewTransactionDialog : ContentDialog
    {
        public NewTransactionDialog(ContentPresenter dialogHost, AccountsViewModel viewModel) : base(dialogHost)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
