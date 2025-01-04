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

    static member FromAccount(account: Account):AccountDashboardDisplayItem =
        let displayItem = AccountDashboardDisplayItem()
        displayItem.AccountName <- account.AccountName
        displayItem.Total <- account.Total

        displayItem

    member this.FromInitializers(accountName: string, total: Currency):AccountDashboardDisplayItem =
        let displayItem = AccountDashboardDisplayItem()
        displayItem.AccountName <- accountName
        displayItem.Total <- total

        displayItem
