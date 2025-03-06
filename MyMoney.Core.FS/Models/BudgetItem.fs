namespace MyMoney.Core.FS.Models

type BudgetItem () =
    let mutable _Id = 0
    let mutable _Category = ""
    let mutable _Amount = Currency(0m)

    member this.Id
        with get () = _Id
        and set value = _Id <- value

    member this.Category
        with get () = _Category
        and set value = _Category <- value

    member this.Amount
        with get () = _Amount
        and set value = _Amount <- value
