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

        const string ethContractAddress = "0x816a772c93dd3d62c05d58eff9e3739502fcf2b6";        
        const string ethSenderAddress = "0x949A6d7D69A72795301472199eB0255a030B0462";
        const string ethSenderPK = "42e301d528240956beb9801af5be6e1fc7836630f86046700e7912585b24969d";        
        const string ethNode = "https://ropsten.infura.io/ALJyYuZ7YioSxeuzglYz";

        string ethAbi;

        public Ethereum(ITestOutputHelper output)
        {
            this.output = output;

            using (StreamReader r = new StreamReader("storage-abi.json"))
            {
                ethAbi = r.ReadToEnd();
            }
        }

        [Fact]
        public async void Notarize()
        {
            const string ethMethodName = "saveHeaderHash";
            const string ethEventName = "FileProofHashCreated";

            string testHashValue = "test hash value";

            Kether.Ethereum eth = new Kether.Ethereum(ethContractAddress,
                                                      ethAbi,
                                                      ethSenderAddress,
                                                      ethSenderPK,
                                                      ethNode
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

            string eventValue = Encoding.UTF8.GetString(System.Convert.FromBase64String(eventData.Value));

            Assert.True(String.Compare(eventValue, testHashValue) == 0);
        }

        [Fact]
        public async void Check()
        {
            string ethMethodName = "does_header_exist";            
            string testHashValue = "test hash value";

            Kether.Ethereum eth = new Kether.Ethereum(ethContractAddress,
                                                      ethAbi,
                                                      null,
                                                      null,
                                                      ethNode
                );

            bool check = await eth.CallContractFunctionAsync<bool>(ethMethodName, new string[] { testHashValue });

            Assert.True(check);
            
            output.WriteLine("Ethereum response:");
            output.WriteLine($"Function {ethMethodName} returned: {check}");
        }
    }
}

