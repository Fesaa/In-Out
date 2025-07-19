using API.Data;
using API.DTOs;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ClientController(IUnitOfWork unitOfWork, IClientService clientService, IMapper mapper) : BaseApiController
{

    /// <summary>
    /// Get all clients
    /// </summary>
    [HttpGet("dto")]
    public async Task<ActionResult<IList<ClientDto>>> GetClientDtos()
    {
        return Ok(await unitOfWork.ClientRepository.GetClientDtos());
    }

    [HttpGet("search")]
    public async Task<ActionResult<IList<ClientDto>>> Search([FromQuery] string query)
    {
        return Ok(await unitOfWork.ClientRepository.SearchClients(query));
    }

    /// <summary>
    /// Get a single client by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto?>> GetClientById(int id)
    {
        var client = await unitOfWork.ClientRepository.GetClientDtoById(id);
        return client is null ? NotFound() : Ok(client);
    }

    /// <summary>
    /// Get a client by company number
    /// </summary>
    [HttpGet("company/{companyNumber}")]
    public async Task<ActionResult<ClientDto?>> GetClientByCompanyNumber(string companyNumber)
    {
        var client = await unitOfWork.ClientRepository.GetClientByCompanyNumber(companyNumber);
        return client is null ? NotFound() : Ok(client);
    }

    /// <summary>
    /// Create a new client
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateClient(ClientDto dto)
    {
        try
        {
            await clientService.CreateClient(dto);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok();
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateClient(ClientDto dto)
    {
        try
        {
            await clientService.UpdateClient(dto);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok();
    }

    /// <summary>
    /// Delete a client by ID
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        try
        {
            await clientService.DeleteClient(id);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok();
    }
}
