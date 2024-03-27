using System;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class ExceptionExtensions
    {
        public static string ToLogFormat(this Exception exception)
        {
            return $"[Exception] {exception?.GetType()} {exception?.Message} {exception?.StackTrace}";
        }
    }
}