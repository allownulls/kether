using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Kether.Models
{    
    public interface IEventModel
    {
        [Parameter("address", "validator", 1, true)]
        string Validator { get; set; }

        [Parameter("bytes32", "headerHash", 2, false)]
        byte[] HeaderHash { get; set; }

        [Parameter("uint", "creation_timestamp", 3, false)]
        int Timestamp { get; set; }

        [Parameter("uint", "headers_count", 4, false)]
        int HeadersCount { get; set; }
    }
}
