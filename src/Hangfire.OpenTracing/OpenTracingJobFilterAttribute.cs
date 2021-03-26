using Hangfire.Common;
using Hangfire.Server;
using OpenTracing;
using OpenTracing.Tag;
using OpenTracing.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangfire.OpenTracing
{
    public class OpenTracingJobFilterAttribute : JobFilterAttribute, IServerFilter
    {
        private readonly HangfireTracingOptions _options;

        public OpenTracingJobFilterAttribute(HangfireTracingOptions options = null)
        {
            _options = options ?? new HangfireTracingOptions();
        }

        public void OnPerforming(PerformingContext context)
        {
            if (IgnoreEvent(context))
            {
                return;
            }

            var operationName = _options.OperationNameResolver(context);

            GlobalTracer.Instance.BuildSpan(operationName)
                .WithTag(Tags.SpanKind, Tags.SpanKindServer)
                .WithTag(Tags.Component, _options.ComponentName)
                .WithTag(TracingHeaders.JobId, context.BackgroundJob.Id)
                .WithTag(TracingHeaders.CreatedAt, context.BackgroundJob.CreatedAt.ToString())
                .StartActive();
        }

        public void OnPerformed(PerformedContext context)
        {
            var scope = GlobalTracer.Instance.ScopeManager.Active;
            if (scope == null)
            {
                return;
            }

            if (context.Exception != null)
            {
                SetSpanException(scope.Span, context.Exception);
            }

            scope.Dispose();
        }

        private bool IgnoreEvent(PerformingContext context)
        {
            return _options.IgnorePatterns.Any(ignore => ignore(context));
        }

        private void SetSpanException(ISpan span, Exception exception)
        {
            span.SetTag(Tags.Error, true);

            span.Log(new Dictionary<string, object>(3)
            {
                {LogFields.Event, Tags.Error.Key},
                {LogFields.ErrorKind, exception.GetType().Name},
                {
                    LogFields.ErrorObject, _options.IncludeExceptionDetails
                        ? exception
                        : (object) $"{nameof(HangfireTracingOptions.IncludeExceptionDetails)} is disabled"
                }
            });
        }
    }
}