using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpendLess.Shared
{
    public class Family
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double ? Balance { get; set; } = 0;

        public Family(int id, string name, double ? balance)
        {
            this.Id = id;
            this.Name = name;
            this.Balance = balance;
        }

        public Family()
        {
        }
    }
}
