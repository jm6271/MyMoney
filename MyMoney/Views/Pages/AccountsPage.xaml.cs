﻿using MyMoney.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace MyMoney.Views.Pages
{
    /// <summary>
    /// Interaction logic for Accounts.xaml
    /// </summary>
    public partial class AccountsPage : Page
    {
        public AccountsViewModel ViewModel { get; }

        public AccountsPage(AccountsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnPageNavigatedTo();

            UpdateListViewMaxHeight();

            ScrollTransactionsToBottom();
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddTransactionButtonClickCommand.Execute(null);

            ScrollTransactionsToBottom();
        }

        private void ScrollTransactionsToBottom()
        {
            TransactionsList.SelectedIndex = TransactionsList.Items.Count - 1;
            TransactionsList.ScrollIntoView(TransactionsList.SelectedItem);
            TransactionsList.SelectedIndex = -1;
        }

        private void UpdateListViewMaxHeight()
        {
            double cardHeight = TransactionsCard.ActualHeight;
            ViewModel.TransactionsMaxHeight = cardHeight - 36;
        }

        private void TransactionsList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update the memo gridview column width
            if (TransactionsList.View is Wpf.Ui.Controls.GridView gridView) 
            { 
                double totalWidth = TransactionsList.ActualWidth;
                double fixedWidth = 0;

                foreach (var column in gridView.Columns)
                {
                    fixedWidth += column.Width;
                }

                fixedWidth -= gridView.Columns[3].Width;

                double lastColumnWidth = totalWidth - fixedWidth - SystemParameters.VerticalScrollBarWidth - 10; 
                gridView.Columns[3].Width = lastColumnWidth > 0 ? lastColumnWidth : 0; 
            }
        }
    }
}
