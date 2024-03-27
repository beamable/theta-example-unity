using System;
using Beamable.Common.Runtime.Collections;

namespace Beamable.Microservices.ThetaFederation.Caching
{
    public static class Memoizer
    {
        public static Func<TInput, TResult> Memoize<TInput, TResult>(this Func<TInput, TResult> func) where TInput : notnull
        {
            var memo = new ConcurrentDictionary<TInput, TResult>();
            return input => memo.GetOrAdd(input, func);
        }
    }
}