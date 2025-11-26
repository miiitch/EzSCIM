using Microsoft.AspNetCore.Mvc;
using ScimAPI.Models;
using ScimAPI.Repositories;

namespace ScimAPI.Controllers
{
    [ApiController]
    [Route("scim/[controller]")]
    [Produces("application/scim+json")]
    public class GroupsController : ControllerBase
    {
        private readonly IScimRepository _repository;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IScimRepository repository, ILogger<GroupsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups([FromQuery] string? filter, [FromQuery] int startIndex = 1, [FromQuery] int count = 100)
        {
            try
            {
                _logger.LogInformation("GetGroups - Filter: {Filter}", filter);
                var response = await _repository.GetGroupsAsync(filter, startIndex, count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetGroups");
                return StatusCode(500, new ScimError { Detail = "Erreur interne", Status = 500 });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(string id)
        {
            var group = await _repository.GetGroupAsync(id);
            if (group == null)
                return NotFound(new ScimError { Detail = $"Groupe {id} non trouvé", Status = 404 });
            return Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] ScimGroup group)
        {
            try
            {
                var existing = await _repository.GetGroupByDisplayNameAsync(group.DisplayName);
                if (existing != null)
                    return Conflict(new ScimError { Detail = "Groupe existe déjà", Status = 409 });

                var createdGroup = await _repository.CreateGroupAsync(group);
                return CreatedAtAction(nameof(GetGroup), new { id = createdGroup.Id }, createdGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur CreateGroup");
                return StatusCode(500, new ScimError { Detail = "Erreur interne", Status = 500 });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] ScimGroup group)
        {
            var updatedGroup = await _repository.UpdateGroupAsync(id, group);
            if (updatedGroup == null)
                return NotFound(new ScimError { Detail = $"Groupe {id} non trouvé", Status = 404 });
            return Ok(updatedGroup);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchGroup(string id, [FromBody] ScimPatchRequest patchRequest)
        {
            var patchedGroup = await _repository.PatchGroupAsync(id, patchRequest);
            if (patchedGroup == null)
                return NotFound(new ScimError { Detail = $"Groupe {id} non trouvé", Status = 404 });
            return Ok(patchedGroup);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(string id)
        {
            var deleted = await _repository.DeleteGroupAsync(id);
            if (!deleted)
                return NotFound(new ScimError { Detail = $"Groupe {id} non trouvé", Status = 404 });
            return NoContent();
        }
    }
}

