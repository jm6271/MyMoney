namespace MyMoney.Core.FS.Models

open System.Collections.ObjectModel
open System

type Budget () =
    let mutable _Id = 0
    let mutable _BudgetTitle = ""
    let mutable _BudgetDate = DateTime.Now
    let mutable _BudgetIncomeItems = ObservableCollection<BudgetItem>()
    let mutable _BudgetExpenseItems = ObservableCollection<BudgetItem>()

    member this.Id
        with get () = _Id
        and set value = _Id <- value

    member this.BudgetTitle
        with get () = _BudgetTitle
        and set value = _BudgetTitle <- value

    member this.BudgetDate
        with get () = _BudgetDate
        and set value = _BudgetDate <- value

    member this.BudgetIncomeItems
        with get () = _BudgetIncomeItems
        and set value = _BudgetIncomeItems <- value

    member this.BudgetExpenseItems
        with get () = _BudgetExpenseItems
        and set value = _BudgetExpenseItems <- value
