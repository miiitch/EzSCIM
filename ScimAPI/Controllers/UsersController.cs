using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScimAPI.Models;
using ScimAPI.Repositories;

namespace ScimAPI.Controllers
{
    [ApiController]
    [Route("scim/[controller]")]
    [Produces("application/scim+json")]
    [Authorize]
    public class UsersController(IScimRepository repository, ILogger<UsersController> logger)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? filter, [FromQuery] int startIndex = 1, [FromQuery] int count = 100)
        {
            try
            {
                logger.LogInformation("GetUsers - Filter: {Filter}", filter);
                var response = await repository.GetUsersAsync(filter, startIndex, count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur GetUsers");
                return StatusCode(500, new ScimError { Detail = "Erreur interne", Status = 500 });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await repository.GetUserAsync(id);
            if (user == null)
                return NotFound(new ScimError { Detail = $"Utilisateur {id} non trouvé", Status = 404 });
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] ScimUser user)
        {
            try
            {
                var existing = await repository.GetUserByUserNameAsync(user.UserName);
                if (existing != null)
                    return Conflict(new ScimError { Detail = "Utilisateur existe déjà", Status = 409 });

                var createdUser = await repository.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur CreateUser");
                return StatusCode(500, new ScimError { Detail = "Erreur interne", Status = 500 });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] ScimUser user)
        {
            var updatedUser = await repository.UpdateUserAsync(id, user);
            if (updatedUser == null)
                return NotFound(new ScimError { Detail = $"Utilisateur {id} non trouvé", Status = 404 });
            return Ok(updatedUser);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(string id, [FromBody] ScimPatchRequest patchRequest)
        {
            var patchedUser = await repository.PatchUserAsync(id, patchRequest);
            if (patchedUser == null)
                return NotFound(new ScimError { Detail = $"Utilisateur {id} non trouvé", Status = 404 });
            return Ok(patchedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var deleted = await repository.DeleteUserAsync(id);
            if (!deleted)
                return NotFound(new ScimError { Detail = $"Utilisateur {id} non trouvé", Status = 404 });
            return NoContent();
        }
    }
}
