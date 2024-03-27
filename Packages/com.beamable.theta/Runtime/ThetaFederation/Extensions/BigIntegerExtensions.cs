using System;
using System.Numerics;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class BigIntegerExtensions
    {
        public static uint ToUInt(this BigInteger bigInt)
        {
            if (bigInt < uint.MinValue || bigInt > uint.MaxValue)
            {
                throw new OverflowException("BigInteger is outside the range of a uint.");
            }

            return (uint)bigInt;
        }
    }
}