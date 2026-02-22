﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EzSCIM.Filtering;
using EzSCIM.Filtering.AST;
using EzSCIM.Helpers;
using EzSCIM.Models;
using EzSCIM.Repositories;

namespace EzSCIM.Controllers
{
    [ApiController]
    [Route("scim/Users")]
    [Produces("application/scim+json")]
    [Authorize]
    public class ScimUsersController(IScimRepository repository, ILogger<ScimUsersController> logger)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? filter, [FromQuery] int startIndex = 1, [FromQuery] int count = 100, [FromQuery] string? excludedAttributes = null)
        {
            try
            {
                logger.LogInformation("GetUsers - Filter: {Filter}", filter);

                FilterExpression? filterExpression = null;
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var parseResult = new FilterParser().Parse(filter);
                    if (parseResult.IsError)
                    {
                        var errorDetails = string.Join("; ", parseResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        logger.LogWarning("GetUsers - Invalid filter: {Filter}. Errors: {Errors}", filter, errorDetails);
                        return BadRequest(new ScimError
                        {
                            Detail = $"Invalid filter: {parseResult.FirstError.Description}",
                            Status = 400
                        });
                    }
                    filterExpression = parseResult.Value;
                }

                var response = await repository.GetUsersAsync(filterExpression, startIndex, count);
                
                if (!string.IsNullOrWhiteSpace(excludedAttributes))
                {
                    var excludeSet = AttributeFilterHelper.ParseAttributeList(excludedAttributes);
                    response.Resources = response.Resources
                        .Select(u => AttributeFilterHelper.FilterUserAttributes(u, excludeSet))
                        .ToList();
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetUsers");
                return StatusCode(500, new ScimError { Detail = "Internal server error", Status = 500 });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id, [FromQuery] string? excludedAttributes = null)
        {
            var user = await repository.GetUserAsync(id);
            if (user == null)
                return NotFound(new ScimError { Detail = $"User {id} not found", Status = 404 });
            
            if (!string.IsNullOrWhiteSpace(excludedAttributes))
            {
                var excludeSet = AttributeFilterHelper.ParseAttributeList(excludedAttributes);
                user = AttributeFilterHelper.FilterUserAttributes(user, excludeSet);
            }
            
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] ScimUser user)
        {
            try
            {
                var existing = await repository.GetUserByUserNameAsync(user.UserName);
                if (existing != null)
                    return Conflict(new ScimError { Detail = "User already exists", Status = 409 });

                var createdUser = await repository.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CreateUser");
                return StatusCode(500, new ScimError { Detail = "Internal server error", Status = 500 });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] ScimUser user)
        {
            var updatedUser = await repository.UpdateUserAsync(id, user);
            if (updatedUser == null)
                return NotFound(new ScimError { Detail = $"User {id} not found", Status = 404 });
            return Ok(updatedUser);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(string id, [FromBody] ScimPatchRequest patchRequest)
        {
            var patchedUser = await repository.PatchUserAsync(id, patchRequest);
            if (patchedUser == null)
                return NotFound(new ScimError { Detail = $"User {id} not found", Status = 404 });
            return Ok(patchedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var deleted = await repository.DeleteUserAsync(id);
            if (!deleted)
                return NotFound(new ScimError { Detail = $"User {id} not found", Status = 404 });
            return NoContent();
        }

    }
}

