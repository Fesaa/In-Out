using API.Data;
using API.DTOs;
using API.Extensions;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UserController(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper): BaseApiController
{

    [HttpPost("by-id")]
    public async Task<ActionResult<IList<UserDto>>> GetByIds(IList<int> ids)
    {
        return Ok(await unitOfWork.UsersRepository.GetByIds(ids));
    }
    
    [AllowAnonymous]
    [HttpGet("has-cookie")]
    public ActionResult<bool> HasCookie()
    {
        return Ok(Request.Cookies.ContainsKey(OidcService.CookieName));
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> CurrentUser()
    {
        var user = await userService.GetUser(User);
        var dto = mapper.Map<UserDto>(user);

        dto.Roles = HttpContext.User.GetRoles();
        return Ok(dto);
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