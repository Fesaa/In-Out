using API.Data;
using API.DTOs;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UserController(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper): BaseApiController
{

    [HttpGet]
    public async Task<ActionResult<UserDto>> CurrentUser()
    {
        var user = await userService.GetUser(User);
        return Ok(mapper.Map<UserDto>(user));
    }

    [HttpGet("all")]
    public async Task<ActionResult<IList<UserDto>>> GetAll()
    {
        return Ok(await unitOfWork.UsersRepository.GetAll());
    }

    [HttpGet("search")]
    public async Task<ActionResult<IList<UserDto>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Ok(new List<UserDto>());
        
        return Ok(await unitOfWork.UsersRepository.Search(query));
    }
    
}