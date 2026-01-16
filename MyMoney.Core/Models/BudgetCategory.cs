using System;
using System.Collections.Generic;
using System.Text;

namespace MyMoney.Core.Models
{
    /// <summary>
    /// Used for displaying budget categories, using a group and an item to allow categories to be grouped together.
    /// </summary>
    public class BudgetCategory
    {
        public string Group { get; set; } = "";
        public string Item { get; set; } = "";
    }
}
