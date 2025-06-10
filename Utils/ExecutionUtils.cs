using System;

namespace NuciWeb.Utils
{
    internal static class ExecutionUtils
    {
        public static T RetryUntilTheResultIsNotNull<T>(
            WebProcessor webProcessor,
            Func<T> action,
            TimeSpan timeout) where T : class
        {
            T result = null;
            int tries = 1;

            DateTime endTime = DateTime.Now.Add(timeout);

            while (result is null)
            {
                try
                {
                    result = action();
                }
                catch
                {
                    webProcessor.Wait();

                    if (DateTime.Now > endTime)
                    {
                        throw;
                    }
                }
                finally
                {
                    tries += 1;
                }
            }

            return result;
        }
    }
}
