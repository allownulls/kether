using System;
using Xunit;
using Xunit.Abstractions;
using Kether;
using Kether.Models;
using System.Text;

using System.IO;

namespace Kether.Test
{
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
            Kether.Models.EventData eventData = await eth.GetEventDataAsync(ethEventName, txData.BlockNumber);

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

