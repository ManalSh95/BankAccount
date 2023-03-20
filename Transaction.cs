using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccount.DataModels
{
    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string AccountId { get; set; } = "";
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; } = 0;
        public DateTime TransactionDate { get; set; }
    }
}
