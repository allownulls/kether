﻿using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading;
using Nethereum.Web3;
using Nethereum.Util;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexTypes;
using Kether.Models;

namespace Kether
{
    public class Ethereum : IContract
    {
        protected Web3 _web3;

        protected ContractSettings contractSettings;

        protected HexBigInteger _curTxCount;

        public Ethereum() { }

        public Ethereum(string contractAddress, string abi, string senderAddress, string senderPrimaryKey, string node)
        {
            contractSettings = new ContractSettings()
            {
                ContractAddress = contractAddress,
                Abi = abi,
                SenderAddress = senderAddress,
                SenderPrimaryKey = senderPrimaryKey,
                EthNode = node
            };

            _web3 = new Web3(node);
        }

        public Ethereum(ContractSettings settings)
        {
            contractSettings = settings;

            _web3 = new Web3(settings.EthNode);
        }

        protected HexBigInteger getNewTxCount()
        {
            HexBigInteger ret = new HexBigInteger(_curTxCount);

            _curTxCount = new HexBigInteger(_curTxCount.Value + new BigInteger(1));

            return ret;
        }

        public async Task<bool> UnlockAsync()
        {
            return await _web3.Personal.UnlockAccount.SendRequestAsync(contractSettings.SenderAddress, contractSettings.SenderPrimaryKey, 30);
        }

        /// <summary>
        /// Prepare data for sending transaction (when function changes the net state)
        /// </summary>
        /// <param name="functionName">Smartcontract function name</param>
        /// <param name="args">Smartcontract function parameters</param>
        /// <returns></returns>
        public string GetFunctionData(string functionName, object[] args)
        {
            var contract = _web3.Eth.GetContract(contractSettings.Abi, contractSettings.ContractAddress);

            var saveHash = contract.GetFunction(functionName);

            var result = saveHash.GetData(args);

            return result;
        }

        /// <summary>
        /// call smartcontract function (when function doesn't change the net state)
        /// </summary>
        /// <typeparam name="T">Smartcontract function return type</typeparam>
        /// <param name="functionName">Smartcontract function name</param>
        /// <param name="args">Smartcontract function parameters</param>
        /// <returns></returns>
        public async Task<T> CallContractFunctionAsync<T>(string functionName, object[] args) where T:struct
        {
            var contract = _web3.Eth.GetContract(contractSettings.Abi, contractSettings.ContractAddress);

            var function = contract.GetFunction(functionName);
            
            var result = await function.CallAsync<T>(args);

            return result;
        }
        
        /// <summary>
        /// send the transaction using prepared data
        /// </summary>
        /// <param name="functionData"></param>
        /// <returns></returns>
        public async Task<string> SendToNetworkAsync(string functionData)
        {
            string errMessage = string.Empty;

            string txId = null;
            bool retry = true;
            int retryCount = 0;

            while (retry)
            {
                retryCount++;

                if (_curTxCount == null)
                    _curTxCount = await _web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(contractSettings.SenderAddress);

                var gasPriceGwei = await _web3.Eth.GasPrice.SendRequestAsync();

                var gasPrice = new BigInteger(UnitConversion.Convert.FromWei(gasPriceGwei));

                var encoded = Web3.OfflineTransactionSigner.SignTransaction(contractSettings.SenderPrimaryKey, new BigInteger(3), contractSettings.ContractAddress, 0, getNewTxCount().Value, gasPriceGwei, 300000, functionData);

                try
                {
                    txId = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);
                }
                catch (Exception e)
                {
                    errMessage = e.Message;
                    Thread.Sleep(10000);
                }

                if (txId != null || retryCount > 10)
                {
                    retry = false;
                }
                else
                {
                    _curTxCount = null;
                }
            }

            return txId;
        }

        /// <summary>
        /// get the first event starting from specified block
        /// </summary>
        /// <param name="eventName">event name</param>
        /// <param name="filterBlockNumber">64-bit block number</param>
        /// <returns></returns>
        public async Task<EventData> GetEventDataAsync<Event>(string eventName, string filterBlockNumber) where Event:IEventModel, new()
        {
            //Infura doesn't support the filters, so have to request GetAllChanges.
            //In case using another service, could be done this way:
            //var filterAll = await hashCreated.CreateFilterAsync();
            //var log = await hashCreated.GetFilterChanges<HashCreatedEvent>(filterAll);

            var contract = _web3.Eth.GetContract(contractSettings.Abi, contractSettings.ContractAddress);
            var hashCreated = contract.GetEvent(eventName);
            var BlockNumber = ulong.Parse(filterBlockNumber);
            var filterAll = hashCreated.CreateFilterInput(new Nethereum.RPC.Eth.DTOs.BlockParameter(BlockNumber), null);

            var log = await hashCreated.GetAllChanges<Event>(filterAll);

            var retTimestamp = log[0].Event.Timestamp.ToString();

            var retValue = Convert.ToBase64String(log[0].Event.HeaderHash);

            return new EventData { Timestamp = retTimestamp, Value = retValue };
        }

        /// <summary>
        /// get full transaction data from receipt
        /// </summary>
        /// <param name="txId"></param>
        /// <returns></returns> 
        public async Task<TransactionData> GetReceiptTxDataAsync(string txId)
        {
            var receipt = await GetReceiptAsync(txId);

            var retBlockNumber = receipt.BlockNumber.Value.ToString();            
            var retDataAddress = receipt.TransactionHash;            
            var retTokenAmount = receipt.Logs[0].Value<string>("data");
            var retTokenContract = receipt.Logs[0]["address"].ToString();

            return new TransactionData { BlockNumber = retBlockNumber, DataAddress = retDataAddress, LogData = retTokenAmount, ContractAddress = retTokenContract };
        }

        /// <summary>
        /// get full transaction data from receipt
        /// </summary>
        /// <param name="txId"></param>
        /// <returns></returns> 
        public async Task<TransactionData> GetTxDataAsync(string txId)
        {
            TransactionReceipt receipt = null;

            int i = 0;
            var timeStart = DateTime.Now;

            while (receipt == null && i++ < 50)
            {
                Thread.Sleep(1000);
                receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            }

            var timeElapsed = DateTime.Now - timeStart;
           
            var retBlockNumber = receipt.BlockNumber.Value.ToString();
            var retDataAddress = receipt.TransactionHash;
//            var retTokenAmount = receipt.Logs[0].Value<string>("data");
//            var retTokenContract = receipt.Logs[0]["address"].ToString();

            string debugInfo = $"tried {i} times, took {timeElapsed.ToString("fff")} ms";

            return new TransactionData { BlockNumber = retBlockNumber, DataAddress = retDataAddress/*, LogData = retTokenAmount, ContractAddress = retTokenContract */, DebugInfo = debugInfo};
        }

        /// <summary>
        /// get transaction information (block number, transaction hash)
        /// </summary>
        /// <param name="transactionHash"></param>
        /// <returns></returns>
        public async Task<TransactionReceipt> GetReceiptAsync(string transactionHash)
        {
            var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            
            int n = 10;
            while (receipt == null && --n >= 0)
            {
                Thread.Sleep(1000);
                receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }

            return receipt;
        }
    }
} 
