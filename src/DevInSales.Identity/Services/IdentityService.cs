using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;

using DevInSales.Identity.Config;
using DevInSales.Application.Dtos;
using DevInSales.Application.Interfaces.Services;

namespace DevInSales.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly JwtOptions _jwtOptions;

        public IdentityService(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IOptions<JwtOptions> jwtOptions) 
            {
                this._signInManager = signInManager;
                this._userManager = userManager;
                this._jwtOptions = jwtOptions.Value;
            }
        
        public async Task<RegisterUserResponse> RegisterUser(RegisterUserRequest userData) {

            var identityUser = new IdentityUser {

                UserName = userData.Email,
                Email = userData.Email,
                EmailConfirmed = true
            };

            var response = await this._userManager.CreateAsync(identityUser, userData.Password);
            if (response.Succeeded) await this._userManager.SetLockoutEnabledAsync(identityUser, false);

            var registerUserResponse = new RegisterUserResponse(response.Succeeded);
            if (!response.Succeeded && response.Errors.Count() > 0)
                Console.Write(response.Errors);
                registerUserResponse.AddErrors(response.Errors.Select(r => r.Description));

            return registerUserResponse;
        }

        public async Task<LoginResponse> Login(LoginRequest login) {

            var response = await this._signInManager.PasswordSignInAsync(login.Email, login.Password, false, true);
            if (response.Succeeded)
                return await GenerateCrendentials(login.Email);

            var loginResponse = new LoginResponse();
            if (!response.Succeeded) {
                if (response.IsLockedOut)
                    loginResponse.AddNewError("This account is blocked.");
                else if (response.IsNotAllowed)
                    loginResponse.AddNewError("This account does not have permission.");
                else if (response.RequiresTwoFactor)
                    loginResponse.AddNewError("Second factor athentitcation required.");
                else 
                    loginResponse.AddNewError("Wrong username or password");
            }

            return loginResponse;
        }



        public async Task<LoginResponse> GenerateCrendentials(string email) {

            var user = await _userManager.FindByEmailAsync(email);
            var accessTokenClaims = await this.GetClaims(user, addUserClaim: true);
            var refreshTokenClaims = await this.GetClaims(user, addUserClaim: false);

            var tokenAccessExpirationDate = DateTime.Now.AddSeconds(_jwtOptions.AccessTokenExpiration);
            var refreshTokenAccessExpirationDate = DateTime.Now.AddSeconds(_jwtOptions.RefreshTokenExpiration);

            var accessToken = GenerateToken(accessTokenClaims, tokenAccessExpirationDate);
            var refreshToken = GenerateToken(refreshTokenClaims, refreshTokenAccessExpirationDate);

            return new LoginResponse
            (
                success: true,
                accessToken: accessToken,
                refreshToken: refreshToken

            );
        }
        private string GenerateToken(IEnumerable<Claim> claims, DateTime expirationDate) {

           var jwt = new JwtSecurityToken(
                issuer: this._jwtOptions.Issuer,
                audience: this._jwtOptions.Audience,
                claims: claims,
                notBefore: DateTime.Now,
                expires: expirationDate,
                signingCredentials: this._jwtOptions.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public async Task<IList<Claim>> GetClaims(IdentityUser user, bool addUserClaim) {
            
            var claims = new List<Claim>();

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, DateTime.Now.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()));

            if (addUserClaim) {
                var userClaims = await this._userManager.GetClaimsAsync(user);
                var roles = await this._userManager.GetRolesAsync(user);

                claims.AddRange(userClaims);

                foreach (var role in roles)
                    claims.Add(new Claim("role", role));
            }

            return claims;
        }
    }
}