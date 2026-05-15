using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EzSCIM.UnitTests
{
    /// <summary>
    /// Classe utilitaire pour configurer l'authentification dans les tests
    /// </summary>
    public static class AuthenticationTestHelper
    {
        /// <summary>
        /// Configure le controller avec un user authentifié
        /// </summary>
        public static void SetupAuthenticatedContext(ControllerBase controller)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim(ClaimTypes.Name, "Test User")
            };
            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}
