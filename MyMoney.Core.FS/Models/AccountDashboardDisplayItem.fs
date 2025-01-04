namespace MyMoney.Core.FS.Models

type AccountDashboardDisplayItem() =
    let mutable _AccountName = ""
    let mutable _Total = Currency(0m)

    member this.AccountName 
        with get () = _AccountName
        and set (value) = _AccountName <- value

    member this.Total
        with get () = _Total
        and set (value) = _Total <- value

    member this.FromAccount(account: Account):unit =
        _AccountName <- account.AccountName
        _Total <- account.Total

    member this.FromInitializers(accountName: string, total: Currency):unit =
        _AccountName <- accountName
        _Total <- total
