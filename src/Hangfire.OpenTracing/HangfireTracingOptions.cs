using Hangfire.Server;
using System;
using System.Collections.Generic;

namespace Hangfire.OpenTracing
{
    public class HangfireTracingOptions
    {
        private string _componentName = "Hangfire";
        private Func<PerformingContext, string> _operationNameResolver = job => $"Job {job.BackgroundJob.Job}";

        /// <summary>
        /// Allows changing the "component" tag of created spans.
        /// </summary>
        public string ComponentName
        {
            get => _componentName;
            set => _componentName = value ?? throw new ArgumentNullException(nameof(ComponentName));
        }

        /// <summary>
        /// A list of delegates that define whether or not a given job should be ignored.
        /// <para/>
        /// If any delegate in the list returns <c>true</c>, the job will be ignored.
        /// </summary>
        public List<Func<PerformingContext, bool>> IgnorePatterns { get; } = new List<Func<PerformingContext, bool>>();

        /// <summary>
        /// A delegate that returns the OpenTracing "operation name" for the given job.
        /// </summary>
        public Func<PerformingContext, string> OperationNameResolver
        {
            get => _operationNameResolver;
            set => _operationNameResolver = value ?? throw new ArgumentNullException(nameof(OperationNameResolver));
        }

        /// <summary>
        /// Whether to add exception details to logs. Defaults to false as they may contain
        /// Personally Identifiable Information (PII), passwords or usernames.
        /// </summary>
        public bool IncludeExceptionDetails { get; set; }
    }
}