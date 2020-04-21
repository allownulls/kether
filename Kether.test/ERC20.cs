using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Kether.Test
{
    public class ERC20
    {
        private readonly ITestOutputHelper output;

        string ethAbi;
        dynamic config;

        public ERC20(ITestOutputHelper output)
        {
            this.output = output;

            using (StreamReader r = new StreamReader("follow-abi.json"))
            {
                ethAbi = r.ReadToEnd();
            }

            string json;

            using (StreamReader r = new StreamReader("follow-config.json"))
            {
                json = r.ReadToEnd();
            }

            config = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        }

        [Fact]
        public async void totalSupplyAsync()
        {            
            Kether.ERC20 eth = new Kether.ERC20((string)config.contractAddress,
                                                ethAbi,
                                                (string)config.ownerAddress,
                                                (string)config.ownerPK,
                                                (string)config.node
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
            Kether.ERC20 eth = new Kether.ERC20((string)config.contractAddress,
                                                ethAbi,
                                                (string)config.userAddress,
                                                (string)config.userPK,
                                                (string)config.node
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
            Kether.ERC20 ethOwner = new Kether.ERC20((string)config.contractAddress,
                                                 ethAbi,
                                                 (string)config.ownerAddress,
                                                 (string)config.ownerPK,
                                                 (string)config.node);

            Kether.ERC20 ethUser = new Kether.ERC20((string)config.contractAddress,
                                                 ethAbi,
                                                 (string)config.userAddress,
                                                 (string)config.userPK,
                                                 (string)config.node);

            string amount = "101";

            bool ok = false;
            ok = await ethOwner.transferAsync((string)config.userAddress, amount);
            Assert.True(ok);
            ok = await ethUser.transferAsync((string)config.ownerAddress, amount);
            Assert.True(ok);


            output.WriteLine($"transfered: {amount}");
            output.WriteLine($" (from: {(string)config.ownerAddress})");
            output.WriteLine($" (to: {(string)config.userAddress})");          
        }

        [Fact]
        public async void approveAsync()
        {
            Kether.ERC20 eth = new Kether.ERC20((string)config.contractAddress,
                                                 ethAbi,
                                                 (string)config.userAddress,
                                                 (string)config.userPK,
                                                 (string)config.node
                                               );
            
            string amount = "101";
            bool ok = false;


            var allowanceBefore = await eth.allowanceAsync((string)config.userAddress, (string)config.ownerAddress);

            ok = await eth.approveAsync((string)config.ownerAddress, amount);
            Assert.True(ok);

            var allowanceAfter = await eth.allowanceAsync((string)config.userAddress, (string)config.ownerAddress);

            

            Assert.True(!String.IsNullOrEmpty(allowanceBefore));
            Assert.True(!String.IsNullOrEmpty(allowanceAfter));
            Assert.True(allowanceAfter == amount);

            ok = await eth.approveAsync((string)config.ownerAddress, "0");
            Assert.True(ok);

            output.WriteLine($"allowanceAfter: {allowanceAfter}");
            output.WriteLine($"amount: {amount}");
            output.WriteLine($"approved: {allowanceAfter}");
            output.WriteLine($" (holder: {(string)config.userAddress})");
            output.WriteLine($" (recipient: {(string)config.ownerAddress})");            
        }

        [Fact]
        public async void allowanceAsync()
        {
            Kether.ERC20 eth = new Kether.ERC20((string)config.contractAddress,
                                                 ethAbi,
                                                 (string)config.userAddress,
                                                 (string)config.userPK,
                                                 (string)config.node
                                               );

            var allowance = await eth.allowanceAsync((string)config.userAddress, (string)config.ownerAddress);

            output.WriteLine($"allowance: {allowance}");
            output.WriteLine($" (holder: {(string)config.userAddress})");
            output.WriteLine($" (recipient: {(string)config.ownerAddress})");

            Assert.True(!String.IsNullOrEmpty(allowance));            
        }

        [Fact]
        public async void transferFromAsync()
        {
            Kether.ERC20 ethUser = new Kether.ERC20((string)config.contractAddress,
                                                 ethAbi,
                                                 (string)config.userAddress,
                                                 (string)config.userPK,
                                                 (string)config.node
                                   );

            Kether.ERC20 ethOwner = new Kether.ERC20((string)config.contractAddress,
                                                 ethAbi,
                                                 (string)config.ownerAddress,
                                                 (string)config.ownerPK,
                                                 (string)config.node
                                               );

            string amount = "101";

            bool ok = false;

            ok = await ethOwner.transferAsync((string)config.ownerAddress, amount);
            Assert.True(ok);

            ok = await ethUser.approveAsync((string)config.ownerAddress, amount);
            Assert.True(ok);

            ok = await ethOwner.transferFromAsync((string)config.userAddress, (string)config.ownerAddress, amount);
            Assert.True(ok);

            output.WriteLine($"transferFrom: {amount}");
            output.WriteLine($" (from: {(string)config.userAddress})");
            output.WriteLine($" (to: {(string)config.ownerAddress})");                        
        }

    }
}
