using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccount.DataModels
{
    public class Account
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CustomerId { get; set; } = "";
        public decimal Balance { get; set; } = 0;
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
