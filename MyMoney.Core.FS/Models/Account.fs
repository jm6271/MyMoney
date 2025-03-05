namespace MyMoney.Core.FS.Models

open System.Collections.ObjectModel
open System.ComponentModel

type Account() =
    let ev = Event<PropertyChangedEventHandler, PropertyChangedEventArgs>()
    let mutable _Id = 0
    let mutable _Transactions = ObservableCollection<Transaction>()
    let mutable _AccountName = ""
    let mutable _Total = Currency(0m)

    member this.Id
        with get () = _Id
        and set value = _Id <- value

    member this.Transactions
        with get () = _Transactions
        and set value = _Transactions <- value

    member this.AccountName
        with get () = _AccountName
        and set value = 
            _AccountName <- value
            ev.Trigger(this, PropertyChangedEventArgs("AccountName"))

    member this.Total
        with get () = _Total
        and set value = 
            _Total <- value
            ev.Trigger(this, PropertyChangedEventArgs("Total"))

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = ev.Publish
