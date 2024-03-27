using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Beamable.Microservices.ThetaFederation.Caching;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class ReflectionExtensions
    {
        private static readonly Func<Assembly, Type[]> AssemblyTypesMemo = Memoizer.Memoize((Assembly assembly) => assembly.GetTypes());

        public static IEnumerable<Type> GetDerivedTypes<TBase>(this Assembly assembly)
        {
            return AssemblyTypesMemo(assembly).Where(type => typeof(TBase).IsAssignableFrom(type) && !type.IsInterface);
        }
    }
}