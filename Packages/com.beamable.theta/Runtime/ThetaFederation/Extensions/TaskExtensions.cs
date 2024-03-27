using System;
using System.Threading.Tasks;
using Beamable.Common;

namespace Beamable.Microservices.ThetaFederation.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> OnErrorAsync<T>(this Task<T> task, Func<Exception, Task> onError)
        {
            try
            {
                return await task;
            }
            catch (Exception e)
            {
                await onError(e);
                throw;
            }
        }

        public static async Promise<T> OnErrorAsync<T>(this Promise<T> task, Func<Exception, Task> onError)
        {
            try
            {
                return await task;
            }
            catch (Exception e)
            {
                await onError(e);
                throw;
            }
        }

        public static async Task<T> WithRetry<T>(this Func<Task<T>> taskFactory, int maxRetry, int timeoutMs = 100, bool useExponentialBackoff = false)
        {
            if (maxRetry < 1)
                throw new ArgumentOutOfRangeException(nameof(maxRetry), "Max retry must be at least 1.");
            if (timeoutMs < 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Timeout must be non-negative.");

            int retryCount = 0;
            while (true)
            {
                try
                {
                    return await taskFactory();
                }
                catch (Exception)
                {
                    retryCount++;
                    if (retryCount > maxRetry)
                    {
                        throw;
                    }
                    int delay = useExponentialBackoff ? timeoutMs * (int)Math.Pow(2, retryCount - 1) : timeoutMs;
                    await Task.Delay(delay);
                }
            }
        }
    }
}