using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;

namespace MyMoney.Core.Models;

public partial class Transaction(DateTime date, string payee, Category category, Currency amount, string memo) : ObservableObject
{
    [ObservableProperty]
    private string _payee = payee;

    [ObservableProperty]
    private Category _category = category;

    [ObservableProperty]
    private Currency _amount = amount;

    [ObservableProperty]
    private string _memo = memo;

    [ObservableProperty]
    private DateTime _date = date;

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
