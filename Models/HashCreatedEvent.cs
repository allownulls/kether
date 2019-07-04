using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Kether
{

    // event FileProofHashCreated(address indexed validator, bytes32 headerHash, uint creation_timestamp, uint headers_count);
    [Event("FileProofHashCreated")]
    public class HashCreatedEvent
    {
        [Parameter("address", "validator", 1, true)]
        public string Validator { get; set; }

        [Parameter("bytes32", "headerHash", 2, false)]
        public byte[] HeaderHash { get; set; }

        [Parameter("uint", "creation_timestamp", 3, false)]
        public int Timestamp { get; set; }

        [Parameter("uint", "headers_count", 4, false)]
        public int HeadersCount { get; set; }
    }
}
