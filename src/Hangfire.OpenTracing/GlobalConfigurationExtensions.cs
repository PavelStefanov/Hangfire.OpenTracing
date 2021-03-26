using System;

namespace Hangfire.OpenTracing
{
    public static class GlobalConfigurationExtensions
    {
        /// <summary>
        /// Adds OpenTracing instrumentation for Hangfire
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IGlobalConfiguration<OpenTracingJobFilterAttribute> UseOpenTracingFilter(
            this IGlobalConfiguration configuration, Action<HangfireTracingOptions> configure = null)
        {
            var options = new HangfireTracingOptions();

            configure?.Invoke(options);

            return configuration.UseFilter(new OpenTracingJobFilterAttribute(options));
        }
    }
}