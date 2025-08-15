using API.Data;
using API.DTOs;
using API.Exceptions;
using API.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace API.Tests.Services;

public class ClientServiceTests(ITestOutputHelper testOutputHelper): AbstractDbTest(testOutputHelper)
{

    private async Task<IClientService> Setup(IUnitOfWork unitOfWork)
    {
        return new ClientService(unitOfWork, Substitute.For<ILogger<ClientService>>(), Substitute.For<ILocalizationService>());
    }

    [Fact]
    public async Task CreateClient_DuplicateCompanyNumber()
    {
        var (unitOfWork, context, _) = await CreateDatabase();
        var service = await Setup(unitOfWork);

        var dto = new ClientDto
        {
            CompanyNumber = "Something",
            Address = "",
            ContactEmail = "",
            ContactName = "",
            ContactNumber = "",
            InvoiceEmail = "",
            Name = "",
            New = true,
        };
        
        await service.CreateClient(dto);

        await Assert.ThrowsAsync<InOutException>(async () =>
        {
            await service.CreateClient(dto);
        });

    }
    
}