using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccount.DataModels
{
    public class Customer : User
    {
        public DateTime CreatedAt { get; set; }
    }
}
