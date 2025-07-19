using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public interface IClientRepository
{
    Task<Client?> GetClientById(int id);
    Task<ClientDto?> GetClientByCompanyNumber(string companyNumber);
    Task<ClientDto?> GetClientDtoById(int id);
    Task<IList<Client>> GetClients();
    Task<IList<ClientDto>> GetClientDtos();
    Task<IList<ClientDto>> SearchClients(string search);
    
    void Add(Client client);
    void Update(Client client);
    void Delete(Client client);
}

public class ClientRepository(DataContext ctx, IMapper mapper): IClientRepository
{

    public async Task<Client?> GetClientById(int id)
    {
        return await ctx.Clients
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<ClientDto?> GetClientByCompanyNumber(string companyNumber)
    {
        return mapper.Map<ClientDto>(await ctx.Clients
            .Where(c => c.CompanyNumber == companyNumber)
            .FirstOrDefaultAsync());
    }

    public async Task<ClientDto?> GetClientDtoById(int id)
    {
        return await ctx.Clients
            .Where(c => c.Id == id)
            .ProjectTo<ClientDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<IList<Client>> GetClients()
    {
        return await ctx.Clients.ToListAsync();
    }

    public async Task<IList<ClientDto>> GetClientDtos()
    {
        return await ctx.Clients
            .ProjectTo<ClientDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IList<ClientDto>> SearchClients(string search)
    {
        var normalizedSearch = search.ToNormalized();
        
        return await ctx.Clients
            .Where(c => EF.Functions.Like(c.NormalizedName, $"%{normalizedSearch}%") ||
                        EF.Functions.Like(c.ContactName, $"%{search}%") ||
                        EF.Functions.Like(c.ContactEmail, $"%{search}%"))
            .ProjectTo<ClientDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public void Add(Client client)
    {
        ctx.Clients.Add(client).State = EntityState.Added;
    }

    public void Update(Client client)
    {
        ctx.Clients.Update(client).State = EntityState.Modified;
    }

    public void Delete(Client client)
    {
        ctx.Clients.Remove(client).State = EntityState.Deleted;
    }
}