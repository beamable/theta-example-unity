using System;
using System.Security.Cryptography;
using System.Text;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class StringExtensions
    {
        public static string HmacSHA256(this string data, string key)
        {
            string ToHexString(byte[] array)
            {
                StringBuilder hex = new StringBuilder(array.Length * 2);
                foreach (byte b in array)
                {
                    hex.AppendFormat("{0:x2}", b);
                }
                return hex.ToString();
            }

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            using var hmac = new HMACSHA256(keyBytes);
            byte[] computedHash = hmac.ComputeHash(dataBytes);
            return ToHexString(computedHash);
        }

        public static ulong HexToUlong(this string value)
        {
            var hex = value;
            if (hex.StartsWith("0x"))
                hex = hex[2..];
            return Convert.ToUInt64(hex, 16);
        }

        public static object ToNumberOrString(this string value)
        {
            object res = value;
            if (double.TryParse(value, out var number)) res = number;
            return res;
        }

        public static string RemoveAddressPadding(this string address)
        {
            var result = address.RemoveHexPrefix();
            if (result.Length <= 40)
            {
                return address;
            }

            return result[^40..].EnsureHexPrefix();
        }
    }
}