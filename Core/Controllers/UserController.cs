using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using System.Security.Claims;
using AspNet.Security.OpenIdConnect.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics;
//using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using OpenIddict.Core;
using System.Linq;
using System.Collections.Generic;

namespace Core.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IOptions<IdentityOptions> _identityOptions;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<IdentityOptions> identityOptions
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _identityOptions = identityOptions;
        }


		[HttpPost("~/connect/token"), Produces("application/json")]
		public async Task<IActionResult> Exchange(OpenIdConnectRequest request)
		{
			Debug.Assert(request.IsTokenRequest(),
				"The OpenIddict binder for ASP.NET Core MVC is not registered. " +
				"Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

			if (request.IsPasswordGrantType())
			{
				var user = await _userManager.FindByNameAsync(request.Username);
				if (user == null)
				{
					return BadRequest(new OpenIdConnectResponse
					{
						Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid (non-existing user)."
					});
				}

				// Ensure the user is allowed to sign in.
				if (!await _signInManager.CanSignInAsync(user))
				{
					return BadRequest(new OpenIdConnectResponse
					{
						Error = OpenIdConnectConstants.Errors.InvalidGrant,
						ErrorDescription = "The specified user is not allowed to sign in."
					});
				}

				// Reject the token request if two-factor authentication has been enabled by the user.
				if (_userManager.SupportsUserTwoFactor && await _userManager.GetTwoFactorEnabledAsync(user))
				{
					return BadRequest(new OpenIdConnectResponse
					{
						Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The specified user is not allowed to sign in (two-factor needs to be disabled)."
					});
				}

				// Ensure the user is not already locked out.
				if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
				{
					return BadRequest(new OpenIdConnectResponse
					{
						Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The username/password couple is invalid (user locked out)."
					});
				}

				// Ensure the password is valid.
				if (!await _userManager.CheckPasswordAsync(user, request.Password))
				{
					if (_userManager.SupportsUserLockout)
					{
						await _userManager.AccessFailedAsync(user);
					}

					return BadRequest(new OpenIdConnectResponse
					{
						Error = OpenIdConnectConstants.Errors.InvalidGrant,
						ErrorDescription = "The username/password couple is invalid."
					});
				}

				if (_userManager.SupportsUserLockout)
				{
					await _userManager.ResetAccessFailedCountAsync(user);
				}

				// Create a new authentication ticket.
				var ticket = await CreateTicketAsync(request, user);

				return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
			}

			return BadRequest(new OpenIdConnectResponse
			{
				Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
				ErrorDescription = "The specified grant type is not supported."
			});
		}

		private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, ApplicationUser user)
		{
			// Create a new ClaimsPrincipal containing the claims that
			// will be used to create an id_token, a token or a code.
			var principal = await _signInManager.CreateUserPrincipalAsync(user);

			// Create a new authentication ticket holding the user identity.
			var ticket = new AuthenticationTicket(principal,
				new AuthenticationProperties(),
				OpenIdConnectServerDefaults.AuthenticationScheme);

			// Set the list of scopes granted to the client application.
			ticket.SetScopes(new[]
			{
				OpenIdConnectConstants.Scopes.OpenId,
				OpenIdConnectConstants.Scopes.Email,
				OpenIdConnectConstants.Scopes.Profile,
				OpenIddictConstants.Scopes.Roles
			}.Intersect(request.GetScopes()));

			ticket.SetResources("resource-server");

			// Note: by default, claims are NOT automatically included in the access and identity tokens.
			// To allow OpenIddict to serialize them, you must attach them a destination, that specifies
			// whether they should be included in access tokens, in identity tokens or in both.

			foreach (var claim in ticket.Principal.Claims)
			{
				// Never include the security stamp in the access and identity tokens, as it's a secret value.
				if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
				{
					continue;
				}

				var destinations = new List<string>
				{
					OpenIdConnectConstants.Destinations.AccessToken
				};

				// Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
				// The other claims will only be added to the access_token, which is encrypted when using the default format.
				if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
					(claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
					(claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
				{
					destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
				}

				claim.SetDestinations(destinations);
			}

			return ticket;
		}


        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me(){
            ApplicationUser me = await _userManager.GetUserAsync(User);
            return new ObjectResult(me.Email);
        }



        [HttpPost("~/api/authalicetest")]
		public IActionResult Exchange2(OpenIdConnectRequest request)
		{
			if (request.IsPasswordGrantType())
			{
				// Validate the user credentials.
				// Note: to mitigate brute force attacks, you SHOULD strongly consider
				// applying a key derivation function like PBKDF2 to slow down
				// the password validation process. You SHOULD also consider
				// using a time-constant comparer to prevent timing attacks.
				if (request.Username != "alice@wonderland.com" ||
					request.Password != "P@ssw0rd")
				{
					return Forbid(OpenIdConnectServerDefaults.AuthenticationScheme);
				}
				// Create a new ClaimsIdentity holding the user identity.
				var identity = new ClaimsIdentity(
					OpenIdConnectServerDefaults.AuthenticationScheme,
					OpenIdConnectConstants.Claims.Name,
					OpenIdConnectConstants.Claims.Role);
				// Add a "sub" claim containing the user identifier, and attach
				// the "access_token" destination to allow OpenIddict to store it
				// in the access token, so it can be retrieved from your controllers.
				identity.AddClaim(OpenIdConnectConstants.Claims.Subject,
					"71346D62-9BA5-4B6D-9ECA-755574D628D8",
					OpenIdConnectConstants.Destinations.AccessToken);
				identity.AddClaim(OpenIdConnectConstants.Claims.Name, "Alice",
					OpenIdConnectConstants.Destinations.AccessToken);
				// ... add other claims, if necessary.
				var principal = new ClaimsPrincipal(identity);
				// Ask OpenIddict to generate a new token and return an OAuth2 token response.
				return SignIn(principal, OpenIdConnectServerDefaults.AuthenticationScheme);
			}
			throw new InvalidOperationException("The specified grant type is not supported.");
		}
	


	    [HttpPost("new")]
        public async Task<IActionResult> Register([FromBody]OrgUserRegistrationData _regData)
        {
            var user = new ApplicationUser
            {
                UserName = _regData.Email,
                Email = _regData.Email,

            };
            var result = await _userManager.CreateAsync(user, _regData.Password);
            if (result.Succeeded){
                return new OkResult();
            }
            else{
                return new ObjectResult(result.Errors);
            }
        }
    }
}
