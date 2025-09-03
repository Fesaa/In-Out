using System.Text.Json;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;
using API.Logging;
using Serilog.Events;

namespace API.Services;

public interface ISettingsService
{
    /// <summary>
    /// You will be required to specify the correct type, there is no compile time checks. Only run time!
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T> GetSettingsAsync<T>(ServerSettingKey key);
    Task<ServerSettingsDto> GetSettingsAsync();
    Task SaveSettingsAsync(ServerSettingsDto settings);
}

public class SettingsService(ILogger<SettingsService> logger, IUnitOfWork unitOfWork): ISettingsService
{
    public async Task<T> GetSettingsAsync<T>(ServerSettingKey key)
    {
        if (!ServerSettingTypeMap.KeyToType.TryGetValue(key, out var expectedType) || expectedType != typeof(T))
        {
            throw new ArgumentException($"Invalid type {typeof(T).Name} for key {key}. Expected {expectedType?.Name ?? "unknown"}");
        }
        
        var setting = await unitOfWork.SettingsRepository.GetSettingsAsync(key);
        return DeserializeSetting<T>(setting);
    }

    private static T DeserializeSetting<T>(ServerSetting setting)
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
    
    private static async Task<string> SerializeSetting(ServerSettingKey key, object setting)
    {
        return key switch
        {
            ServerSettingKey.CsvExportConfiguration => JsonSerializer.Serialize(setting),
            ServerSettingKey.LogLevel => setting.ToString(),
            _ => throw new ArgumentException($"No converter found for key {key}"),
        } ?? string.Empty;
    }

    public async Task<ServerSettingsDto> GetSettingsAsync()
    {
        var settings = await unitOfWork.SettingsRepository.GetSettingsAsync();
        var dto = new ServerSettingsDto();
        
        foreach (var serverSetting in settings)
        {
            switch (serverSetting.Key)
            {
                case ServerSettingKey.CsvExportConfiguration:
                    dto.CsvExportConfiguration = DeserializeSetting<CsvExportConfigurationDto>(serverSetting);
                    break;
                case ServerSettingKey.LogLevel:
                    dto.LogLevel = DeserializeSetting<LogEventLevel>(serverSetting);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serverSetting.Key), serverSetting.Key, "Unknown server settings key");
            }
        }
        
        return dto;
    }

    private async Task<bool> UpdateIfDifferent(ServerSetting setting, object value)
    {
        var serialized = await SerializeSetting(setting.Key, value);
        if (setting.Value != serialized)
        {
            setting.Value = serialized;
            unitOfWork.SettingsRepository.Update(setting);
            return true;
        }
        
        return false;
    }

    public async Task SaveSettingsAsync(ServerSettingsDto dto)
    {
        var settings = await unitOfWork.SettingsRepository.GetSettingsAsync();

        foreach (var serverSetting in settings)
        {
            object value = serverSetting.Key switch
            {
                ServerSettingKey.CsvExportConfiguration => dto.CsvExportConfiguration,
                ServerSettingKey.LogLevel => dto.LogLevel,
                _ => throw new ArgumentOutOfRangeException(nameof(serverSetting.Key), serverSetting.Key, "Unknown server settings key"),
            };
            
            var updated = await UpdateIfDifferent(serverSetting, value);

            if (updated && serverSetting.Key == ServerSettingKey.LogLevel)
            {
                LogLevelOptions.SwitchLogLevel(dto.LogLevel);
            }
        }

        if (unitOfWork.HasChanges())
        {
            await unitOfWork.CommitAsync();
        }
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