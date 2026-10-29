using DEKODE.OptiFlow.Domain.Interfaces.Auth;
using Microsoft.EntityFrameworkCore;

namespace DEKODE.OptiFlow.DAL.Repository;

public class UserRepository : IUserRepository
{
    private readonly OptiFlowDbContext _context;

    public UserRepository(OptiFlowDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AuthUserWithCredentials?> GetByNormalizedUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedUsername))
            return null;

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.NormalizedUsername == normalizedUsername && u.IsActive, cancellationToken);

        if (user == null)
            return null;

        return new AuthUserWithCredentials
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role?.Code ?? "User",
            DisplayName = user.DisplayName,
            PasswordHash = user.PasswordHash,
            PasswordSalt = user.PasswordSalt,
            IsActive = user.IsActive,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LockoutEndUtc = user.LockoutEndUtc
        };
    }

    public async Task RecordSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null)
        {
            user.FailedLoginAttempts = 0;
            user.LastLoginAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RecordFailedLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user != null)
        {
            user.FailedLoginAttempts++;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

