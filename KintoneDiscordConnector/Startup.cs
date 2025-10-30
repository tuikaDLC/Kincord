using KintoneDiscordConnector.Controllers;
using KintoneDiscordConnector.Models;
using KintoneDiscordConnector.Services;

namespace KintoneDiscordConnector;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddHttpClient();

        // AppSettingsをシングルトンとして登録
        var settings = AppSettings.Load();
        services.AddSingleton(settings);

        // サービスの登録
        services.AddSingleton<IDiscordService, DiscordService>();
        services.AddSingleton<DiagnosticService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
