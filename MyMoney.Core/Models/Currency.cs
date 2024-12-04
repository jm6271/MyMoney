namespace MyMoney.Core.Models
{
    public class Currency
    {
        public decimal Value { get; set; }

        public Currency(decimal value)
        {
            Value = value;
        }

        public Currency()
        {
            Value = 0;
        }

        // Overload the addition operator
        public static Currency operator +(Currency c1, Currency c2)
        {
            return new Currency(c1.Value + c2.Value);
        }

        // Overload the subtraction operator
        public static Currency operator -(Currency c1, Currency c2)
        {
            return new Currency(c1.Value - c2.Value);
        }

        // Overload the multiplication operator
        public static Currency operator *(Currency c, decimal multiplier)
        {
            return new Currency(c.Value * multiplier);
        }

        // Overload the division operator
        public static Currency operator /(Currency c, decimal divisor)
        {
            return new Currency(c.Value / divisor);
        }

        // Overload ToString() to return a formatted string
        public override string ToString()
        {
            if (Value >= 0)
                return $"${Value:F2}";
            else
                return $"(${Math.Abs(Value):F2})";
        }
    }
}
