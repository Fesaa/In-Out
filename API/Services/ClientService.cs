using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;

namespace API.Services;

public interface IClientService
{
    Task CreateClient(ClientDto dto);
    Task UpdateClient(ClientDto dto);
    Task DeleteClient(int id);
}

public class ClientService(IUnitOfWork unitOfWork, ILogger<ClientService> logger, ILocalizationService localizationService): IClientService
{

    public async Task CreateClient(ClientDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.CompanyNumber))
        {
            var others = await unitOfWork.ClientRepository.GetClientByCompanyNumber(dto.CompanyNumber.Trim());
            if (others != null)
                throw new ApplicationException("errors.client-already-exists");
        }

        var client = new Client
        {
            Name = dto.Name,
            NormalizedName = dto.Name.ToNormalized(),
            CompanyNumber = dto.CompanyNumber.Trim(),
            InvoiceEmail = dto.InvoiceEmail.Trim(),
            ContactName = dto.ContactName.Trim(),
            ContactNumber = dto.ContactNumber.Trim(),
            ContactEmail = dto.ContactEmail.Trim(),
            Address = dto.Address.Trim(),
        };
        
        unitOfWork.ClientRepository.Add(client);
        await unitOfWork.CommitAsync();
    }

    public async Task UpdateClient(ClientDto dto)
    {
        var client = await unitOfWork.ClientRepository.GetClientById(dto.Id);
        if (client == null)
            throw new ApplicationException("errors.client-not-found");

        var systemNotes = new List<string>();
        
        if (client.NormalizedName != dto.Name.ToNormalized())
        {
            client.Name = dto.Name;
            client.NormalizedName = dto.Name.ToNormalized();
        }

        if (client.CompanyNumber != dto.CompanyNumber)
        {
            if (!string.IsNullOrWhiteSpace(dto.CompanyNumber))
            {
                var others = await unitOfWork.ClientRepository.GetClientByCompanyNumber(dto.CompanyNumber.Trim());
                if (others != null)
                    throw new ApplicationException("errors.client-already-exists");
            }
            
            logger.LogDebug("Updating CompanyNumber for {ClientId} - {ClientName}, adding note to outgoing deliveries", client.Id, client.Name);
            systemNotes.Add(await localizationService.Translate("client-update-company-number-note", client.CompanyNumber, dto.CompanyNumber.Trim()));

            client.CompanyNumber = dto.CompanyNumber;
        }

        if (client.Address != dto.Address.Trim())
        {
            logger.LogDebug("Updating Address for {ClientId} - {ClientName}, adding note to outgoing deliveries", client.Id, client.Name);
            systemNotes.Add(await localizationService.Translate("client-update-address-note", client.Address, dto.Address.Trim()));
            client.Address = dto.Address.Trim();
        }

        if (client.InvoiceEmail != dto.InvoiceEmail.Trim())
        {
            logger.LogDebug("Updating invoice email for {ClientId} - {ClientName}, adding note to outgoing deliveries", client.Id, client.Name);
            systemNotes.Add(await localizationService.Translate("client-update-invoice-email",  client.InvoiceEmail, dto.InvoiceEmail.Trim()));
            client.InvoiceEmail = dto.InvoiceEmail.Trim();
        }
        
        client.ContactName = dto.ContactName.Trim();
        client.ContactNumber = dto.ContactNumber.Trim();
        client.ContactEmail = dto.ContactEmail.Trim();
        
        // TODO: Add systemNotes to outgoing deliveries
        if (systemNotes.Count > 0)
        {
            
        }
        
        unitOfWork.ClientRepository.Update(client);
        await unitOfWork.CommitAsync();
    }
    
    public async Task DeleteClient(int id)
    {
        var client = await unitOfWork.ClientRepository.GetClientById(id);
        if  (client == null)
            throw new ApplicationException("errors.no-client-found");
        
        unitOfWork.ClientRepository.Delete(client);
        await unitOfWork.CommitAsync();
    }
}