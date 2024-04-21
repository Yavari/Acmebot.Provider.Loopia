using Acmebot.Provider.Loopia.Exceptions;
using Acmebot.Provider.Loopia.Loopia;
using Acmebot.Provider.Loopia.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostBuilderContext, services) =>
    {
        var settings = hostBuilderContext.Configuration.GetSection(LoopiaOptions.Option).Get<List<LoopiaOptions>>();
        Validate(settings);
        services.AddSingleton(new Settings(settings!));
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddHttpClient();
        services.AddSingleton<LoopiaService>();
    })
    .Build();

host.Run();

void Validate(List<LoopiaOptions>? settings)
{
    if (settings == null)
        throw new CustomConfigurationException("Loopia array setting is not set");

    foreach (var setting in settings)
    {
        if (string.IsNullOrWhiteSpace(setting.Username))
            throw new CustomConfigurationException("Username is not set");
        if (string.IsNullOrWhiteSpace(setting.Password))
            throw new CustomConfigurationException("Password is not set");
    }
}
