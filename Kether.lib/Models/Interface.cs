using System;
using System.Collections.Generic;
using System.Text;
using Nethereum.RPC.Eth.DTOs;
using System.Threading.Tasks;
using System.Threading;

namespace Kether.Models
{
    public interface IContract
    {
        //Nethereum.Hex.HexTypes.HexBigInteger getNewTxCount();

        Task<bool> UnlockAsync();
        string GetFunctionData(string functionName, object[] args);
        Task<T> CallContractFunctionAsync<T>(string functionName, object[] args) where T : struct;
        Task<string> SendToNetworkAsync(string functionData);
        Task<EventData> GetEventDataAsync(string eventName, string filterBlockNumber);
        Task<TransactionData> GetTxDataAsync(string txId);
        Task<TransactionReceipt> GetReceiptAsync(string transactionHash);

    }
}
