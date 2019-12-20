using System;
using Xunit;
using Xunit.Abstractions;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors;

using System.IO;

namespace Kether.Test
{
    //0xc844EF8252481408Be0bcFd096754Ff262B50B4e
    //0xd7c2c73a5c36bb61cbbf7c02a9bee6bf8df8a129

    public class ERC20
    {
        private readonly ITestOutputHelper output;

        const string ethContractAddress = "0x9bE49E6FE0bd2aca0bE21143D318b7E1fA97E48a";

        const string ethOwnerAddress = "0xd7c2c73a5c36bb61cbbf7c02a9bee6bf8df8a129";
        const string ethOwnerPK = "9114fe5123b1a685cb20550ddfeb66be71cf267299225e82e19c9909c5e31d7e";

        const string ethUserAddress = "0x949A6d7D69A72795301472199eB0255a030B0462";
        const string ethUserPK = "42e301d528240956beb9801af5be6e1fc7836630f86046700e7912585b24969d";


        const string ethNode = "https://ropsten.infura.io/ALJyYuZ7YioSxeuzglYz";

        string ethAbi;

        public ERC20(ITestOutputHelper output)
        {
            this.output = output;

            using (StreamReader r = new StreamReader("follow-abi.json"))
            {
                ethAbi = r.ReadToEnd();
            }

        }

        [Fact]
        public async void totalSupplyAsync()
        {            
            Kether.ERC20 eth = new Kether.ERC20(ethContractAddress,
                                                      ethAbi,
                                                      ethOwnerAddress,
                                                      ethOwnerPK,
                                                      ethNode
                                               );

            var totalSupply = await eth.totalSupplyAsync();

            output.WriteLine("Ethereum response:");
            output.WriteLine($"totalSupply: {totalSupply}");

            Assert.True(!String.IsNullOrEmpty(totalSupply));
            Assert.True(totalSupply == "5000000");

        }

        [Fact]
        public async void balanceOfAsync()
        {
            Kether.ERC20 eth = new Kether.ERC20(ethContractAddress,
                                                      ethAbi,
                                                      ethUserAddress,
                                                      ethUserPK,
                                                      ethNode
                                               );

            string holder = "0xd7c2c73a5c36bb61cbbf7c02a9bee6bf8df8a129";

            var balanceOf = long.Parse(await eth.balanceOfAsync(holder));

            Assert.True(balanceOf >= 0);
            output.WriteLine($"balanceOf: {balanceOf}");

            var totalSupply = long.Parse(await eth.totalSupplyAsync());

            Assert.True(totalSupply > 0);

            Assert.True(balanceOf <= totalSupply);
        }

        [Fact]
        public async void transferAsync()
        {
            Kether.ERC20 ethOwner = new Kether.ERC20(ethContractAddress,
                                                      ethAbi,
                                                      ethOwnerAddress,
                                                      ethOwnerPK,
                                                      ethNode );

            Kether.ERC20 ethUser = new Kether.ERC20(ethContractAddress,
                                                      ethAbi,
                                                      ethUserAddress,
                                                      ethUserPK,
                                                      ethNode);

            string amount = "101";

            bool ok = false;
            ok = await ethOwner.transferAsync(ethUserAddress, amount);
            Assert.True(ok);
            ok = await ethUser.transferAsync(ethOwnerAddress, amount);
            Assert.True(ok);


            output.WriteLine($"transfered: {amount}");
            output.WriteLine($" (from: {ethOwnerAddress})");
            output.WriteLine($" (to: {ethUserAddress})");          
        }

        [Fact]
        public async void approveAsync()
        {
            Kether.ERC20 eth = new Kether.ERC20(ethContractAddress,
                                                      ethAbi,
                                                      ethUserAddress,
                                                      ethUserPK,
                                                      ethNode
                                               );
            
            string amount = "101";
            bool ok = false;


            var allowanceBefore = await eth.allowanceAsync(ethUserAddress, ethOwnerAddress);

            ok = await eth.approveAsync(ethOwnerAddress, amount);
            Assert.True(ok);

            var allowanceAfter = await eth.allowanceAsync(ethUserAddress, ethOwnerAddress);

            

            Assert.True(!String.IsNullOrEmpty(allowanceBefore));
            Assert.True(!String.IsNullOrEmpty(allowanceAfter));
            Assert.True(allowanceAfter == amount);

            ok = await eth.approveAsync(ethOwnerAddress, "0");
            Assert.True(ok);

            output.WriteLine($"allowanceAfter: {allowanceAfter}");
            output.WriteLine($"amount: {amount}");
            output.WriteLine($"approved: {allowanceAfter}");
            output.WriteLine($" (holder: {ethUserAddress})");
            output.WriteLine($" (recipient: {ethOwnerAddress})");            
        }

        [Fact]
        public async void allowanceAsync()
        {
            Kether.ERC20 eth = new Kether.ERC20(ethContractAddress,
                                                      ethAbi,
                                                      ethUserAddress,
                                                      ethUserPK,
                                                      ethNode
                                               );

            var allowance = await eth.allowanceAsync(ethUserAddress, ethOwnerAddress);

            output.WriteLine($"allowance: {allowance}");
            output.WriteLine($" (holder: {ethUserAddress})");
            output.WriteLine($" (recipient: {ethOwnerAddress})");

            Assert.True(!String.IsNullOrEmpty(allowance));            
        }

        [Fact]
        public async void transferFromAsync()
        {
            Kether.ERC20 ethUser = new Kether.ERC20(ethContractAddress,
                                          ethAbi,
                                          ethUserAddress,
                                          ethUserPK,
                                          ethNode
                                   );

            Kether.ERC20 ethOwner = new Kether.ERC20(ethContractAddress,
                                                      ethAbi,
                                                      ethOwnerAddress,
                                                      ethOwnerPK,
                                                      ethNode
                                               );

            string amount = "101";

            bool ok = false;

            ok = await ethOwner.transferAsync(ethOwnerAddress, amount);
            Assert.True(ok);

            ok = await ethUser.approveAsync(ethOwnerAddress, amount);
            Assert.True(ok);

            ok = await ethOwner.transferFromAsync(ethUserAddress, ethOwnerAddress, amount);
            Assert.True(ok);

            output.WriteLine($"transferFrom: {amount}");
            output.WriteLine($" (from: {ethUserAddress})");
            output.WriteLine($" (to: {ethOwnerAddress})");                        
        }

    }
}
