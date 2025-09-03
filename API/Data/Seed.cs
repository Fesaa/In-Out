using System.Text.Json;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;
using Serilog.Events;

namespace API.Data;

public static class Seed
{

    private static readonly IList<ServerSetting> DefaultServerSettings = [
        new() { Key = ServerSettingKey.CsvExportConfiguration, Value = JsonSerializer.Serialize(new CsvExportConfigurationDto()) },
        new () { Key = ServerSettingKey.LogLevel, Value = nameof(LogEventLevel.Information) }
    ];
    
    public static async Task Run(DataContext ctx)
    {
        foreach (var defaultServerSetting in DefaultServerSettings)
        {
            var existing = ctx.ServerSettings.FirstOrDefault(s => s.Key == defaultServerSetting.Key);
            if (existing == null)
            {
                ctx.ServerSettings.Add(defaultServerSetting);
            }
        }


        await ctx.SaveChangesAsync();
    }
    
}