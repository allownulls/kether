using System;
using System.Threading.Tasks;
using System.Numerics;

using Kether.Models;


namespace Kether
{
    public class ERC20 : Ethereum
    {
        Ethereum eth = null;
        public ERC20(string ethContractAddress,
                     string ethAbi,
                     string ethSenderAddress,
                     string ethSenderPK,
                     string ethNode)
        {
            eth = new Ethereum(ethContractAddress,
                               ethAbi,
                               ethSenderAddress,
                               ethSenderPK,
                               ethNode);
        }

        public ERC20(ContractSettings settings)
        {
            eth = new Ethereum(settings.ContractAddress,
                               settings.Abi,
                               settings.SenderAddress,
                               settings.SenderPrimaryKey,
                               settings.EthNode);
        }

        public async Task<string> totalSupplyAsync()
        {
            var ret = await eth.CallContractFunctionAsync<BigInteger>("totalSupply", new string[] { });

            ret = ret / new BigInteger(1e18);

            return ret.ToString();
        }

        public async Task<string> balanceOfAsync(string ownerAddress)
        {
            var ret = await eth.CallContractFunctionAsync<BigInteger>("balanceOf", new string[] { ownerAddress });

            ret = ret / new BigInteger(1e18);

            return ret.ToString();
        }

        public async Task<string> allowanceAsync(string ownerAddress, string spenderAddress)
        {
            var ret = await eth.CallContractFunctionAsync<BigInteger>("allowance", new string[] { ownerAddress, spenderAddress });

            ret = ret / new BigInteger(1e18);

            return ret.ToString();
        }

        public async Task<bool> approveAsync(string spenderAddress, string amount)
        {
            var hexAmount = BigInteger.Multiply(BigInteger.Parse(amount), 1000000000000000000).ToString("x");

            var contractFuncData = eth.GetFunctionData("approve", new string[] { spenderAddress, hexAmount});

            string txId = await eth.SendToNetworkAsync(contractFuncData);

            bool ret = !String.IsNullOrEmpty(txId);

            return ret;
        }
        public async Task<bool> transferAsync(string toAddress, string amount)
        {
            var hexAmount = BigInteger.Multiply(BigInteger.Parse(amount), 1000000000000000000).ToString("x");

            var contractFuncData = eth.GetFunctionData("transfer", new string[] { toAddress, hexAmount });
            string txId = await eth.SendToNetworkAsync(contractFuncData);

            bool ret = !String.IsNullOrEmpty(txId);

            return ret;
        }
        public async Task<bool> transferFromAsync(string fromAddress, string toAddress, string amount)
        {
            var hexAmount = BigInteger.Multiply(BigInteger.Parse(amount), 1000000000000000000).ToString("x");

            var contractFuncData = eth.GetFunctionData("transferFrom", new string[] { fromAddress, toAddress, hexAmount});
            string txId = await eth.SendToNetworkAsync(contractFuncData);

            bool ret = !String.IsNullOrEmpty(txId);

            return ret;
        }
        public async Task<bool> mintAsync(string toAddress, string amount)
        {
            var hexAmount = Util.GetBigInt(amount).ToString("x");

            var contractFuncData = eth.GetFunctionData("mint", new string[] { toAddress, hexAmount });
            string txId = await eth.SendToNetworkAsync(contractFuncData);

            bool ret = !String.IsNullOrEmpty(txId);

            return ret;
        }

        new public async Task<TransactionData> GetTxDataAsync(string txId)
        {
            return await eth.GetReceiptTxDataAsync(txId);//eth.GetTxDataAsync(txId);
        }

    }
}
