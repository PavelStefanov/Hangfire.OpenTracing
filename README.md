# Hangfire.OpenTracing

Hangfire.OpenTracing provides integration with [OpenTracing](https://opentracing.io/).

## Installation

To install Hangfire.OpenTracing, run the following command in the Nuget Package Manager Console:

```
Install-Package Hangfire.OpenTracing
```

## Using

You can add OpenTracing integration by invoking `UseOpenTracingFilter` extension method on `IGlobalConfiguration`.

### Example

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // make sure you configure OpenTracing before 
    services.AddHangfire(c =>
    {
        // ...

        // Add OpenTracing integration
        c.UseOpenTracingFilter(options => 
        {
            // these are the defaults
            options.ComponentName = "Hangfire";
            options.IncludeExceptionDetails = false;
        });
    });
    services.AddHangfireServer();
}
```
