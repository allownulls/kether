using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Kether.Models;

namespace Kether.Test
{
    [Event("FileProofHashCreated")]
    public class HashCreatedEvent : IEventModel
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

    public class Ethereum
    {
        private readonly ITestOutputHelper output;

        string ethAbi;
        dynamic config;

        public Ethereum(ITestOutputHelper output)
        {
            this.output = output;

            using (StreamReader r = new StreamReader("storage-abi.json"))
            {
                ethAbi = r.ReadToEnd();
            }

            string json;

            using (StreamReader r = new StreamReader("storage-config.json"))
            {
                json = r.ReadToEnd();
            }

            config = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        }

        [Fact]
        public async void Notarize()
        {
            const string ethMethodName = "saveHeaderHash";
            const string ethEventName = "FileProofHashCreated";

            string testHashValue = "test hash value";

            Kether.Ethereum eth = new Kether.Ethereum((string)config.contractAddress,
                                                      ethAbi,
                                                      (string)config.senderAddress,
                                                      (string)config.senderPK,
                                                      (string)config.node
                );

            var contractFuncData = eth.GetFunctionData(ethMethodName, new string[] { testHashValue });
            string txId = await eth.SendToNetworkAsync(contractFuncData);

            Kether.Models.TransactionData txData = await eth.GetTxDataAsync(txId);
            Kether.Models.EventData eventData = await eth.GetEventDataAsync<HashCreatedEvent>(ethEventName, txData.BlockNumber);

            Assert.True(txData != null);
            Assert.True(eventData != null);
            Assert.True(!String.IsNullOrEmpty(txData.BlockNumber));
            Assert.True(!String.IsNullOrEmpty(txData.DataAddress));
            Assert.True(!String.IsNullOrEmpty(eventData.Timestamp));

            output.WriteLine("Ethereum response:");
            output.WriteLine($"Block Number: {txData.BlockNumber}");
            output.WriteLine($"Data Address: {txData.DataAddress}");
            output.WriteLine($"Timestamp: {eventData.Timestamp}");
            output.WriteLine($"Stored Value: {eventData.Value}");
            output.WriteLine($"Test hash value: {testHashValue}");
            output.WriteLine($"Sending to network: {txData.DebugInfo}");

            string eventValue = Encoding.UTF8.GetString(System.Convert.FromBase64String(eventData.Value));

            Assert.True(String.Compare(eventValue, testHashValue) == 0);
        }

        [Fact]
        public async void Check()
        {
            string ethMethodName = "does_header_exist";            
            string testHashValue = "test hash value";

            Kether.Ethereum eth = new Kether.Ethereum((string)config.contractAddress,
                                                      ethAbi,
                                                      null,
                                                      null,
                                                      (string)config.node
                );

            bool check = await eth.CallContractFunctionAsync<bool>(ethMethodName, new string[] { testHashValue });

            Assert.True(check);
            
            output.WriteLine("Ethereum response:");
            output.WriteLine($"Function {ethMethodName} returned: {check}");
        }
    }
}

