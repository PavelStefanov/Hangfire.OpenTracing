using Microsoft.Extensions.Logging;
using System.Threading;

namespace WebApi.Jobs
{
    public class ExampleJob
    {
        private readonly ILogger<ExampleJob> _logger;

        public ExampleJob(ILogger<ExampleJob> logger)
        {
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("Start ExampleJob");
            Thread.Sleep(1000);
        }
    }
}