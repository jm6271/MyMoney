using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyMoney.Core.Models;

public partial class Transaction : ObservableObject
{
    public Transaction(DateTime date, string payee, Category category, Currency amount, string memo)
    {
        Payee = payee;
        Category = category;
        Amount = amount;
        Memo = memo;
        Date = date;

        if (TransactionHash == "")
        {
            // Generate a hash for this transaction
            TransactionHash = Guid.NewGuid().ToString();
        }
    }

    public string TransactionHash { get; private set; } = "";

    [ObservableProperty]
    private string _payee;

    [ObservableProperty]
    private Category _category;

    [ObservableProperty]
    private Currency _amount;

    [ObservableProperty]
    private string _memo;

    [ObservableProperty]
    private string _transactionDetail = "";

    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private bool _reconciled = false;

    public string DateFormatted
    {
        get { return Date.ToShortDateString(); }
    }

    public string MonthAbbreviated
    {
        get { return Date.ToString("MMM"); }
    }

    public string AmountFormatted
    {
        get
        {
            if (Amount.Value > 0m)
                return "+" + Amount.ToString();
            else if (Amount.Value < 0m)
                return "-" + new Currency(Math.Abs(Amount.Value)).ToString();
            else
                return "$0.00";
        }
    }
}
