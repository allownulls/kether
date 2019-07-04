using System.Numerics;
using System.Threading.Tasks;
using System.Threading;

using Nethereum.Web3;
using Nethereum.Util;
using Nethereum.RPC.Eth.DTOs;

using Newtonsoft.Json;

namespace Kether
{
    public class Ethereum
    {
        Web3 _web3;

        string _contractAddress;
        string _abi;
        string _senderAddress;
        string _senderPK;
        private Nethereum.Hex.HexTypes.HexBigInteger _curTxCount;

        public Ethereum(string contractAddress, string abi, string senderAddress, string senderPrimaryKey, string node)
        {
            _contractAddress = contractAddress;
            _abi = abi;
            _senderAddress = senderAddress;
            _senderPK = senderPrimaryKey;
            _web3 = new Web3(node);
        }
       
        private Nethereum.Hex.HexTypes.HexBigInteger getNewTxCount()
        {
            Nethereum.Hex.HexTypes.HexBigInteger ret = new Nethereum.Hex.HexTypes.HexBigInteger(_curTxCount);

            _curTxCount = new Nethereum.Hex.HexTypes.HexBigInteger(_curTxCount.Value + new BigInteger(1));

            return ret;
        }

        public async Task<bool> Unlock()
        {
            return await _web3.Personal.UnlockAccount.SendRequestAsync(_senderAddress, _senderPK, 30);
        }

        public async Task<bool> TestHashStore(string hash)
        {
            var contract = _web3.Eth.GetContract(_abi, _contractAddress);

            var isExist = contract.GetFunction("does_header_exist");

            var result = await isExist.CallAsync<bool>(hash);

            return result;
        }

        public string GetFunctionData(string functionName, string argBase64)
        {
            var contract = _web3.Eth.GetContract(_abi, _contractAddress);

            var saveHash = contract.GetFunction(functionName);// "saveHeaderHash"
                      
            var result = saveHash.GetData(System.Convert.FromBase64String(argBase64));            

            return result;
        }

        public async Task<string> CallContractFunctionAsync(string functionName, string argBase64)
        {
            var contract = _web3.Eth.GetContract(_abi, _contractAddress);

            var function = contract.GetFunction(functionName); //"saveHeaderHash"
            
            var result = await function.CallAsync<string>(argBase64);

            return result;
        }

        public async Task<string> SendToNetwork(string functionData)
        {
            string errMessage = string.Empty;

            string txId = null;
            bool retry = true;
            int retryCount = 0;

            while (retry)
            {
                retryCount++;

                if (_curTxCount == null)
                    _curTxCount = await _web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(_senderAddress);

                var gasPriceGwei = await _web3.Eth.GasPrice.SendRequestAsync();

                var gasPrice = new BigInteger(UnitConversion.Convert.FromWei(gasPriceGwei));

                var encoded = Web3.OfflineTransactionSigner.SignTransaction(_senderPK, new BigInteger(3), _contractAddress, 0, getNewTxCount().Value, gasPriceGwei, 300000, functionData);

                try
                {
                    txId = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);
                }
                catch (System.Exception e)
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

        public async Task<string> GetEventData(string eventName, string filterBlockNumber)
        {
            //Infura doesn't support the filters, so have to request GetAllChanges.
            //In case using another service, could be done this way:
            //var filterAll = await hashCreated.CreateFilterAsync();
            //var log = await hashCreated.GetFilterChanges<HashCreatedEvent>(filterAll);

            var contract = _web3.Eth.GetContract(_abi, _contractAddress);
            var hashCreated = contract.GetEvent(eventName);// "FileProofHashCreated"
            var filterAll = hashCreated.CreateFilterInput(new Nethereum.RPC.Eth.DTOs.BlockParameter(), null);
            var BlockNumber = new Nethereum.Hex.HexTypes.HexBigInteger(filterBlockNumber);
            var log = await hashCreated.GetAllChanges<HashCreatedEvent>(filterAll);

            var retTimestamp = log[0].Event.Timestamp.ToString();
            var retGlobalHash = System.Convert.ToBase64String(log[0].Event.HeaderHash);

            return JsonConvert.SerializeObject(new { Timestamp = retTimestamp,  GlobalHash = retGlobalHash });
        }

        public async Task<string> GetTxData(string txId)
        {
            var receipt = await GetReceiptAsync(_web3, txId);

            var retBlockNumber = receipt.BlockNumber.Value.ToString();            
            var retDataAddress = receipt.TransactionHash;            

            return JsonConvert.SerializeObject(new { BlockNumber = retBlockNumber, DataAddress = retDataAddress});
        }

        public async Task<TransactionReceipt> GetReceiptAsync(Web3 web3, string transactionHash)
        {
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

            while (receipt == null)
            {
                Thread.Sleep(1000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }

            return receipt;
        }

        public static Ethereum getInstance(string contractAddress, string abi, string senderAddress, string senderPrivateKey, string ethNode)
        {
            return new Ethereum(contractAddress, abi, senderAddress, senderPrivateKey, ethNode);
        }
    }
} 
