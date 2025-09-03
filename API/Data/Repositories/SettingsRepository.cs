using API.DTOs;
using API.Entities;
using API.Entities.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public interface ISettingsRepository
{
    void Update(ServerSetting settings);
    void Remove(ServerSetting setting);

    Task<ServerSetting> GetSettingsAsync(ServerSettingKey key);
    Task<IList<ServerSetting>> GetSettingsAsync();
}

public class SettingsRepository(DataContext ctx, IMapper mapper): ISettingsRepository
{

    public void Update(ServerSetting settings)
    {
        ctx.Entry(settings).State = EntityState.Modified;
    }

    public void Remove(ServerSetting setting)
    {
        ctx.Remove(setting);
    }

    public async Task<ServerSetting> GetSettingsAsync(ServerSettingKey key)
    {
        return await ctx.ServerSettings
            .Where(x => x.Key == key)
            .FirstAsync();
    }

    public async Task<IList<ServerSetting>> GetSettingsAsync()
    {
        return await ctx.ServerSettings.ToListAsync();
    }
}