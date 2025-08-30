using System.Text.Json;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;
using Serilog.Events;

namespace API.Services;

public interface IServerSettingsService
{
    /// <summary>
    /// You will be required to specify the correct type, there is no compile time checks. Only run time!
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> GetSettingsAsync<T>(ServerSettingKey key);
    Task<ServerSettingDto> GetSettingsAsync();
}

public class ServerSettingsService(ILogger<ServerSettingsService> logger, IUnitOfWork unitOfWork): IServerSettingsService
{
    public async Task<T> GetSettingsAsync<T>(ServerSettingKey key)
    {
        if (!ServerSettingTypeMap.KeyToType.TryGetValue(key, out var expectedType) || expectedType != typeof(T))
        {
            throw new ArgumentException($"Invalid type {typeof(T).Name} for key {key}. Expected {expectedType?.Name ?? "unknown"}");
        }
        
        var setting = await unitOfWork.SettingsRepository.GetSettingsAsync(key);
        return ConvertSetting<T>(setting);
    }

    private static T ConvertSetting<T>(ServerSetting setting)
    {
        object? result = setting.Key switch
        {
            ServerSettingKey.CsvExportConfiguration => JsonSerializer.Deserialize<CsvExportConfigurationDto>(setting.Value),
            ServerSettingKey.LogLevel => Enum.Parse<LogEventLevel>(setting.Value),
            _ => default(T),
        };

        return result switch
        {
            null => throw new ArgumentException($"No converter found for key {setting.Key}"),
            T typedResult => typedResult,
            _ => throw new ArgumentException($"Failed to convert {setting.Key} - {setting.Value} to type {typeof(T).Name}")
        };
    }

    public async Task<ServerSettingDto> GetSettingsAsync()
    {
        var settings = await unitOfWork.SettingsRepository.GetSettingsAsync();
        var dto = new ServerSettingDto();
        
        foreach (var serverSetting in settings)
        {
            switch (serverSetting.Key)
            {
                case ServerSettingKey.CsvExportConfiguration:
                    dto.CsvExportConfiguration = ConvertSetting<CsvExportConfigurationDto>(serverSetting);
                    break;
                case ServerSettingKey.LogLevel:
                    dto.LogLevel = ConvertSetting<LogEventLevel>(serverSetting);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serverSetting.Key), serverSetting.Key, "Unknown server settings key");
            }
        }
        
        return dto;
    }
    
    private static class ServerSettingTypeMap
    {
        public static readonly Dictionary<ServerSettingKey, Type> KeyToType = new()
        {
            { ServerSettingKey.CsvExportConfiguration, typeof(CsvExportConfigurationDto) },
            { ServerSettingKey.LogLevel, typeof(LogEventLevel) },
        };
    }
}