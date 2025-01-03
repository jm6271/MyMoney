namespace MyMoney.Core.FS.Models

open System

type Currency(value:decimal) =
    member this.Value = value

    // Overloaded ToString() method
    // Returns a formatted string
    override this.ToString (): string = 
        if this.Value >= 0m then // normal number
            $"{this.Value:N2}"
        else // negative number, print in parenthesis
            $"(${Math.Abs(this.Value):N2})"

    // Overloaded operators
    static member (+) (c1: Currency, c2: Currency) =
        Currency(c1.Value + c2.Value)

    static member (-) (c1: Currency, c2: Currency) =
        Currency(c1.Value - c2.Value)

    static member (*) (c1: Currency, multiplier: decimal) =
        Currency(c1.Value * multiplier)

    static member (/) (c1: Currency, divisor: decimal) =
        Currency(c1.Value / divisor)
