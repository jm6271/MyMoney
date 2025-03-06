namespace MyMoney.Core.FS.Models

open System

type Transaction (date:DateTime, payee:string, category:string, amount:Currency, memo:string) =
    let mutable _Payee = payee
    let mutable _Category = category
    let mutable _Memo = memo
    let mutable _Date = date
    let mutable _Amount = amount

    member this.Payee 
        with get() = _Payee
        and set value = _Payee <- value

    member this.Category
        with get() = _Category
        and set value = _Category <- value

    member this.Amount
        with get() = _Amount
        and set value = _Amount <- value

    member this.Memo
        with get() = _Memo
        and set value = _Memo <- value

    member this.Date
        with get() = _Date
        and set value = _Date <- value

    member this.DateFormatted
        with get() = _Date.ToShortDateString()

    member this.MonthAbbreviated
        with get() = _Date.ToString("MMM")

    member this.AmountFormatted
        with get() = 
            if (_Amount.Value > 0m)
                then "+" + _Amount.ToString()
            elif(_Amount.Value < 0m) then
                let t = Math.Abs(_Amount.Value)
                let tc = Currency(t)
                "-" + tc.ToString()
            else
                "$0.00"
