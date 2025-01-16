namespace MyMoney.Core.FS.Models

open System

type Transaction (date:DateTime, payee:string, category:string, spend:Currency, receive:Currency, balance:Currency, memo:string) =
    let mutable _Payee = payee
    let mutable _Category = category
    let mutable _Spend = spend
    let mutable _Receive = receive
    let mutable _Balance = balance
    let mutable _Memo = memo
    let mutable _Date = date

    member this.Payee 
        with get() = _Payee
        and set (value) = _Payee <- value

    member this.Category
        with get() = _Category
        and set (value) = _Category <- value

    member this.Spend 
        with get() = _Spend
        and set (value) = _Spend <- value

    member this.Receive
        with get() = _Receive
        and set (value) = _Receive <- value

    member this.Balance
        with get() = _Balance
        and set (value) = _Balance <- value

    member this.Memo
        with get() = _Memo
        and set (value) = _Memo <- value

    member this.Date
        with get() = _Date
        and set (value) = _Date <- value

    member this.DateFormatted
        with get() = _Date.ToShortDateString()

    member this.MonthAbbreviated
        with get() = _Date.ToString("MMM")

    member this.AmountFormatted
        with get() = 
            if (_Spend.Value > 0m)
                then "-" + _Spend.ToString()
            elif(_Receive.Value > 0m)
                then "+" + _Receive.ToString()
            else
                "$0.00"

