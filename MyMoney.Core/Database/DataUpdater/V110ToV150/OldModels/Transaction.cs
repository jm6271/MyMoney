using MyMoney.Core.Models;

namespace MyMoney.DataUpdater.V110ToV150.OldModels;

internal sealed class Transaction
{
    public int Id { get; set; }
    public string TransactionHash { get; set; } = "";
    public string Payee { get; set; } = "";
    public Category Category { get; set; } = new();
    public Currency Amount { get; set; } = new(0m);
    public string Memo { get; set; } = "";
    public string TransactionDetail { get; set; } = "";
    public DateTime Date { get; set; }
    public bool Reconciled { get; set; }
    public int AccountId { get; set; } = -1;
}
