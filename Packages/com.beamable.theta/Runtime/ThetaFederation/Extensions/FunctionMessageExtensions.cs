using System;
using Beamable.Microservices.ThetaFederation.Caching;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class FunctionMessageExtensions
    {
        private static readonly Func<Type, string> FunctionNameMemo = Memoizer.Memoize((Type type) =>
        {
            var attribute = FunctionAttribute.GetAttribute(type);
            return attribute.Name;
        });

        public static string GetFunctionName(this FunctionMessage functionMessage)
        {
            return FunctionNameMemo(functionMessage.GetType());
        }
    }
}