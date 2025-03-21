using System.Xml.Linq;

namespace MyMoney.Core.Models
{
    public class Currency(decimal value)
    {
        public decimal Value { get; set; } = value;

        public override string ToString()
        {
            if (this.Value >= 0m)
            {
                return $"${this.Value:N2}";
            }
            else
            {
                return $"(${Math.Abs(this.Value):N2})";
            }
        }

        public static Currency operator +(Currency c1, Currency c2)
        {
            return new Currency(c1.Value + c2.Value);
        }

        public static Currency operator -(Currency c1, Currency c2)
        {
            return new Currency(c1.Value - c2.Value);
        }

        public static Currency operator *(Currency c1, decimal multiplier)
        {
            return new Currency(c1.Value * multiplier);
        }

        public static Currency operator /(Currency c1, decimal divisor)
        {
            return new Currency(c1.Value / divisor);
        }

        public static bool operator ==(Currency c1, Currency c2)
        {
            return c1.Value == c2.Value;
        }

        public static bool operator !=(Currency c1, Currency c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            return this == (Currency)obj;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
}
