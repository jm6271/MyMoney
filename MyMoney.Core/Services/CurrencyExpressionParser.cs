using System;
using System.Collections.Generic;
using System.Text;
using MyMoney.Core.Models;

namespace MyMoney.Core.Services
{
    public class CurrencyExpressionParser
    {
        public static bool TryEvaluate(string expression, out decimal amount, out string? error)
        {
            try
            {
                // Strip dollar signs
                expression = expression.Replace("$", "").Trim();

                // Strip commas
                expression = expression.Replace(",", "");

                // Evaluate with NCalc
                var expr = new NCalc.Expression(expression);

                var result = System.Convert.ToDecimal(expr.Evaluate());

                amount = result;
                error = null;
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                amount = 0;
                return false;
            }
        }
    }
}
