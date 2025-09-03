using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Core.Models
{
    public class GroupedBudget
    {
        public string Group { get; set; } = "";
        public Budget Budget { get; set; } = new();
    }
}
