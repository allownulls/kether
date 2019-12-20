using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Kether
{
    public class Util
    {
        public static BigInteger GetBigInt(string amount)
        {            
            int multiplierDigits = 18; //token decimals by default

            if (amount.Contains('.'))
            {
                multiplierDigits -= amount.Substring(amount.IndexOf(".") + 1).Length;
                amount = amount.Replace(".", String.Empty);
            }                       
            
            BigInteger multiplierAmount = BigInteger.Parse("1" + new String('0', multiplierDigits));
            BigInteger amountBigInt = BigInteger.Multiply(BigInteger.Parse(amount), multiplierAmount);
            return amountBigInt;            
        }
    }
}
