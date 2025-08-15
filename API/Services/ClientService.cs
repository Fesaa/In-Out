using API.Data;
using API.DTOs;
using API.Entities;
using API.Entities.Enums;
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

        var systemNotes = new List<SystemMessage>();

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
            AddSystemNote(await localizationService.Translate("client-update-company-number-note", client.CompanyNumber, dto.CompanyNumber.Trim()));

            client.CompanyNumber = dto.CompanyNumber;
        }

        if (client.Address != dto.Address.Trim())
        {
            logger.LogDebug("Updating Address for {ClientId} - {ClientName}, adding note to outgoing deliveries", client.Id, client.Name);
            AddSystemNote(await localizationService.Translate("client-update-address-note", client.Address, dto.Address.Trim()));

            client.Address = dto.Address.Trim();
        }

        if (client.InvoiceEmail != dto.InvoiceEmail.Trim())
        {
            logger.LogDebug("Updating invoice email for {ClientId} - {ClientName}, adding note to outgoing deliveries", client.Id, client.Name);
            AddSystemNote(await localizationService.Translate("client-update-invoice-email",  client.InvoiceEmail, dto.InvoiceEmail.Trim()));

            client.InvoiceEmail = dto.InvoiceEmail.Trim();
        }
        
        client.ContactName = dto.ContactName.Trim();
        client.ContactNumber = dto.ContactNumber.Trim();
        client.ContactEmail = dto.ContactEmail.Trim();

        if (systemNotes.Count > 0)
        {
            var deliveries = await unitOfWork.DeliveryRepository.GetDeliveriesForClient(client.Id, [DeliveryState.InProgress, DeliveryState.Completed]);
            foreach (var delivery in deliveries)
            {
                foreach (var systemNote in systemNotes) delivery.SystemMessages.Add(systemNote);
                unitOfWork.DeliveryRepository.Update(delivery);
            }
        }
        
        unitOfWork.ClientRepository.Update(client);
        await unitOfWork.CommitAsync();
        return;

        void AddSystemNote(string s) => systemNotes.Add(s.ToSystemMessage());
    }
    
    public async Task DeleteClient(int id)
    {
        var client = await unitOfWork.ClientRepository.GetClientById(id);
        if  (client == null)
            throw new ApplicationException("errors.client-not-found");

        var deliveries = await unitOfWork.DeliveryRepository.GetDeliveriesForClient(client.Id, [DeliveryState.InProgress, DeliveryState.Completed]);
        if (deliveries.Count > 0)
            throw new ApplicationException("errors.unfinished-deliveries");
        
        unitOfWork.ClientRepository.Delete(client);
        await unitOfWork.CommitAsync();
    }
}