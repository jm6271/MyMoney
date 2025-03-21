using System;

namespace MyMoney.Core.Models;

public class Transaction(DateTime date, string payee, string category, Currency amount, string memo)
{
    public string Payee { get; set; } = payee;

    public string Category { get; set; } = category;

    public Currency Amount { get; set; } = amount;

    public string Memo { get; set; } = memo;

    public DateTime Date { get; set; } = date;

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
                return "-" + Math.Abs(Amount.Value).ToString();
            else
                return "$0.00";
        }
    }
}
