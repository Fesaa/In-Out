using API.Constants;
using API.Data;
using API.DTOs;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ClientController(IUnitOfWork unitOfWork, IClientService clientService, IMapper mapper) : BaseApiController
{
    
    [HttpPost("by-id")]
    public async Task<ActionResult<IList<ClientDto>>> GetClientDtosByIds(IList<int> ids)
    {
        return Ok(await unitOfWork.ClientRepository.GetClientDtosByIds(ids));
    }

    /// <summary>
    /// Get all clients
    /// </summary>
    [HttpGet]
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
    /// Create a new client
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateClient(ClientDto dto)
    {
        await clientService.CreateClient(dto);
        return Ok();
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    [HttpPut]
    [Authorize(Policy = PolicyConstants.ManageClients)]
    public async Task<IActionResult> UpdateClient(ClientDto dto)
    {
        await clientService.UpdateClient(dto);
        return Ok();
    }

    /// <summary>
    /// Delete a client by ID
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = PolicyConstants.ManageClients)]
    public async Task<IActionResult> DeleteClient(int id)
    {
        await clientService.DeleteClient(id);
        return Ok();
    }
}
