using System;
using System.IO;
using Kether;
using System.Threading.Tasks;
using System.Threading;

namespace KetherTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string ethAbi;
            dynamic config;

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

            Kether.ERC20 eth = new Kether.ERC20((string)config.contractAddress,
                                    ethAbi,
                                    (string)config.ownerAddress,
                                    (string)config.ownerPK,
                                    (string)config.node
                                   );

            var totalSupply = await eth.totalSupplyAsync();

            Console.WriteLine("Ethereum response:");
            Console.WriteLine($"totalSupply: {totalSupply}");
            Console.ReadLine();
        }
    }
}
