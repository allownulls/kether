using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using System.Numerics;

namespace Kether.Test
{
    public class Util
    {
        [Fact]
        public async void GetBigInt()
        {
            string amount = "1.1234";
            string proper = "123400000000000000";

            BigInteger result = Kether.Util.GetBigInt(amount);
            Assert.True(result.ToString() == proper);
        }
    }
}
