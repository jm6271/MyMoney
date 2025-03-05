namespace MyMoney.Core.FS.Models

type BudgetReportItem () =
    let mutable _Id = 0;
    let mutable _Category = ""
    let mutable _Budgeted = Currency(0m)
    let mutable _Actual = Currency(0m)
    let mutable _Remaining = Currency(0m)

    member this.Id
        with get () = _Id
        and set value = _Id <- value

    member this.Category 
        with get () = _Category
        and set value = _Category <- value
    
    member this.Budgeted
        with get() = _Budgeted
        and set value = _Budgeted <- value
    
    member this.Actual
        with get() = _Actual
        and set value = _Actual <- value
    
    member this.Remaining
        with get () = _Remaining
        and set value = _Remaining <- value
