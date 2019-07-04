using System;
using System.Collections.Generic;
using System.Text;

namespace Kether.Models
{
    public class ContractSettings
    {
        public string ContractAddress { get; set; }
        public string Abi { get; set; }
        public string SenderAddress { get; set; }
        public string SenderPrimaryKey { get; set; }
        public string EthNode { get; set; }
    }
}
