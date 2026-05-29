using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tests_and_Interviews_API.Data;
using Tests_and_Interviews_API.DTOs;
using Tests_and_Interviews_API.Models;
using Tests_and_Interviews_API.Models.Core;
using Tests_and_Interviews_API.Repositories.Interfaces;
using Tests_and_Interviews_API.Services.Interfaces;

namespace Tests_and_Interviews_API.Services
{
    public class AuthService : IAuthService
    {
        private const string SecretKey = "O_CHEIE_SECRET_FOARTE_LUNGA_SI_SIGURA_AICI_12345!";
        private const string Issuer = "UBB-SE-2026";
        private const string Audience = "UBB-SE-Client";

        private readonly ICompanyRepo companyRepository;
        private readonly AppDbContext dbContext;

        public AuthService(ICompanyRepo companyRepository, AppDbContext dbContext)
        {
            this.companyRepository = companyRepository;
            this.dbContext = dbContext;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await this.dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null) return null;

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed) return null;

            var role = await this.ResolveRoleAsync(user.Id);
            user.Role = role;

            return new AuthResponseDto
            {
                Token = this.GenerateJwt(user),
                Role = role,
                Name = user.Name,
                UserId = user.Id,
            };
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            // In the merged setup, PussyCats API owns the Users table and always creates
            // the user before calling this endpoint. We never INSERT into Users here.
            var user = await this.dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null) return null;

            if (dto.Role == "Recruiter")
            {
                if (!dto.CompanyId.HasValue) return null;

                var company = this.companyRepository.GetById(dto.CompanyId.Value);
                if (company == null) return null;

                bool alreadyRecruiter = await this.dbContext.Recruiters
                    .AnyAsync(r => r.UserId == user.Id);
                if (!alreadyRecruiter)
                {
                    this.dbContext.Recruiters.Add(new Recruiter
                    {
                        CompanyId = company.CompanyId,
                        UserId = user.Id,
                        CompanyName = company.Name,
                        Company = company,
                    });
                    await this.dbContext.SaveChangesAsync();
                }
            }

            user.Role = dto.Role;
            return new AuthResponseDto
            {
                Token = this.GenerateJwt(user),
                Role = dto.Role,
                Name = user.Name,
                UserId = user.Id,
            };
        }

        private async Task<string> ResolveRoleAsync(int userId)
        {
            bool isRecruiter = await this.dbContext.Recruiters
                .AnyAsync(r => r.UserId == userId);
            return isRecruiter ? "Recruiter" : "Candidate";
        }

        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
            };
            var token = new JwtSecurityToken(
                issuer: Issuer, audience: Audience, claims: claims,
                expires: DateTime.UtcNow.AddHours(8), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
