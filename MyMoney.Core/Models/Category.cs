using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMoney.Core.Models
{
    public class Category
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
    }
}
