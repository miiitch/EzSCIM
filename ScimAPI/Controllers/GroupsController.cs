using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScimAPI.Filtering;
using ScimAPI.Filtering.AST;
using ScimAPI.Models;
using ScimAPI.Repositories;

namespace ScimAPI.Controllers
{
    [ApiController]
    [Route("scim/[controller]")]
    [Produces("application/scim+json")]
    [Authorize]
    public class GroupsController(IScimRepository repository, ILogger<GroupsController> logger)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetGroups([FromQuery] string? filter, [FromQuery] int startIndex = 1, [FromQuery] int count = 100)
        {
            try
            {
                logger.LogInformation("GetGroups - Filter: {Filter}", filter);

                FilterExpression? filterExpression = null;
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var parseResult = new FilterParser().Parse(filter);
                    if (parseResult.IsError)
                    {
                        var errorDetails = string.Join("; ", parseResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        logger.LogWarning("GetGroups - Invalid filter: {Filter}. Errors: {Errors}", filter, errorDetails);
                        return BadRequest(new ScimError
                        {
                            Detail = $"Invalid filter: {parseResult.FirstError.Description}",
                            Status = 400
                        });
                    }
                    filterExpression = parseResult.Value;
                }

                var response = await repository.GetGroupsAsync(filterExpression, startIndex, count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetGroups");
                return StatusCode(500, new ScimError { Detail = "Internal server error", Status = 500 });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(string id)
        {
            var group = await repository.GetGroupAsync(id);
            if (group == null)
                return NotFound(new ScimError { Detail = $"Groupe {id} non trouvé", Status = 404 });
            return Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] ScimGroup group)
        {
            try
            {
                var existing = await repository.GetGroupByDisplayNameAsync(group.DisplayName);
                if (existing != null)
                    return Conflict(new ScimError { Detail = "Groupe existe déjà", Status = 409 });

                var createdGroup = await repository.CreateGroupAsync(group);
                return CreatedAtAction(nameof(GetGroup), new { id = createdGroup.Id }, createdGroup);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur CreateGroup");
                return StatusCode(500, new ScimError { Detail = "Erreur interne", Status = 500 });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] ScimGroup group)
        {
            var updatedGroup = await repository.UpdateGroupAsync(id, group);
            if (updatedGroup == null)
                return NotFound(new ScimError { Detail = $"Groupe {id} non trouvé", Status = 404 });
            return Ok(updatedGroup);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchGroup(string id, [FromBody] ScimPatchRequest patchRequest)
        {
            var patchedGroup = await repository.PatchGroupAsync(id, patchRequest);
            if (patchedGroup == null)
                return NotFound(new ScimError { Detail = $"Groupe {id} non trouvé", Status = 404 });
            return Ok(patchedGroup);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(string id)
        {
            var deleted = await repository.DeleteGroupAsync(id);
            if (!deleted)
                return NotFound(new ScimError { Detail = $"Groupe {id} non trouvé", Status = 404 });
            return NoContent();
        }
    }
}
