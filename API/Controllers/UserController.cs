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

    /// <summary>
    /// Get users by ids
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPost("by-id")]
    public async Task<ActionResult<IList<UserDto>>> GetByIds(IList<int> ids)
    {
        return Ok(await unitOfWork.UsersRepository.GetByIds(ids));
    }
    
    /// <summary>
    /// Returns true if the auth cookie is present, does not say anything about its validity
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("has-cookie")]
    public ActionResult<bool> HasCookie()
    {
        return Ok(Request.Cookies.ContainsKey(OidcService.CookieName));
    }

    /// <summary>
    /// Returns the current authenticated user, and its roles
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<UserDto>> CurrentUser()
    {
        var user = await userService.GetUser(User);
        var dto = mapper.Map<UserDto>(user);

        dto.Roles = HttpContext.User.GetRoles();
        return Ok(dto);
    }

    /// <summary>
    /// Returns all users
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    public async Task<ActionResult<IList<UserDto>>> GetAll()
    {
        return Ok(await unitOfWork.UsersRepository.GetAll());
    }

    /// <summary>
    /// Search for a specific user
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet("search")]
    public async Task<ActionResult<IList<UserDto>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Ok(new List<UserDto>());
        
        return Ok(await unitOfWork.UsersRepository.Search(query));
    }

    /// <summary>
    /// Update non OIDC synced attributes of a user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<UserDto>> UpdateUser(UserDto user)
    {
        var updated = await userService.Update(User, user);

        var dto = mapper.Map<UserDto>(updated);
        dto.Roles = HttpContext.User.GetRoles();
        return Ok(dto);
    }
    
}