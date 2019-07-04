using System;
using Xunit;
using Xunit.Abstractions;
using Kether;
using Kether.Models;
using System.Text;

namespace Kether.Test
{
    public class Ethereum
    {
        private readonly ITestOutputHelper output;

        public Ethereum(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async void Notarize()
        {
            string ethContractAddress = "0x816a772c93dd3d62c05d58eff9e3739502fcf2b6"; //"0xdab48ba055663eff50a52c0da2ef1cd40a1a2a20";
            string ethAbi = "[{\"constant\":false,\"inputs\":[{\"name\":\"headerHash\",\"type\":\"bytes32\"}],\"name\":\"saveHeaderHash\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"header_hash\",\"type\":\"bytes32\"}],\"name\":\"does_header_exist\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"\",\"type\":\"bytes32\"}],\"name\":\"Hashes\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"owner\",\"outputs\":[{\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"validator\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"headerHash\",\"type\":\"bytes32\"},{\"indexed\":false,\"name\":\"creation_timestamp\",\"type\":\"uint256\"},{\"indexed\":false,\"name\":\"headers_count\",\"type\":\"uint256\"}],\"name\":\"FileProofHashCreated\",\"type\":\"event\"}]";
            string ethSenderAddress = "0x949A6d7D69A72795301472199eB0255a030B0462";
            string ethSenderPK = "42e301d528240956beb9801af5be6e1fc7836630f86046700e7912585b24969d";
            string ethMethodName = "saveHeaderHash";
            string ethEventName = "FileProofHashCreated";
            string ethNode = "https://ropsten.infura.io/ALJyYuZ7YioSxeuzglYz";

            string testHashValue = //"test hash value"; // new 
                "t28moOekzkB5soZ0uGo+SptRadFAWMwu/9258b8maD0=";

            Kether.Ethereum eth = new Kether.Ethereum(ethContractAddress,
                                                      ethAbi,
                                                      ethSenderAddress,
                                                      ethSenderPK,
                                                      ethNode                                                                                             
                );

            var sourceBytes = System.Convert.FromBase64String(testHashValue);
            var contractFuncData = eth.GetFunctionData(ethMethodName, new object[] { sourceBytes });
            //var contractFuncData = eth.GetFunctionData(ethMethodName, new string[] { testHashValue });

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
                        
            Assert.True(String.Compare(eventData.Value, testHashValue) == 0);            
        }

        [Fact]
        public async void Token()
        {
            string ethContractAddress = "0x816a772c93dd3d62c05d58eff9e3739502fcf2b6"; //"0xdab48ba055663eff50a52c0da2ef1cd40a1a2a20";
            string ethAbi = "[{\"constant\":false,\"inputs\":[{\"name\":\"headerHash\",\"type\":\"bytes32\"}],\"name\":\"saveHeaderHash\",\"outputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"header_hash\",\"type\":\"bytes32\"}],\"name\":\"does_header_exist\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[{\"name\":\"\",\"type\":\"bytes32\"}],\"name\":\"Hashes\",\"outputs\":[{\"name\":\"\",\"type\":\"bool\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"constant\":true,\"inputs\":[],\"name\":\"owner\",\"outputs\":[{\"name\":\"\",\"type\":\"address\"}],\"payable\":false,\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"payable\":false,\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"name\":\"validator\",\"type\":\"address\"},{\"indexed\":false,\"name\":\"headerHash\",\"type\":\"bytes32\"},{\"indexed\":false,\"name\":\"creation_timestamp\",\"type\":\"uint256\"},{\"indexed\":false,\"name\":\"headers_count\",\"type\":\"uint256\"}],\"name\":\"FileProofHashCreated\",\"type\":\"event\"}]";
            string ethSenderAddress = "0x949A6d7D69A72795301472199eB0255a030B0462";
            string ethSenderPK = "42e301d528240956beb9801af5be6e1fc7836630f86046700e7912585b24969d";
            string ethMethodName = "saveHeaderHash";
            string ethEventName = "FileProofHashCreated";
            string ethNode = "https://ropsten.infura.io/ALJyYuZ7YioSxeuzglYz";

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

            Assert.True(String.Compare(eventData.Value, testHashValue) == 0);
        }
    }
}

