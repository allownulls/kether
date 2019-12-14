using System;
using System.Collections.Generic;
using System.Text;

namespace Kether.Models
{
    public class TransactionData
    {
        public string BlockNumber { get; set; }

        public string DataAddress { get; set; }

        public string LogData { get; set; }

        public string ContractAddress { get; set; }
    }
}
