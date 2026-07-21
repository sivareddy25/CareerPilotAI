using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CareerPilot.Infrastructure.Persistence;

/// <summary>
/// Infrastructure implementation of <see cref="IUnitOfWork"/> wrapping EF Core SaveChanges.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    /// <summary>PostgreSQL <c>unique_violation</c>.</summary>
    private const string UniqueViolation = "23505";

    private const string UserEmailIndex = "ix_users_normalized_email";

    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
                  {
                      SqlState: UniqueViolation,
                      ConstraintName: UserEmailIndex,
                  })
        {
            // Two registrations for the same address raced past the handler's
            // pre-check. The unique index is the actual guarantee; this translates its
            // violation into the same 409 the pre-check would have produced, so the
            // loser of the race gets a coherent answer instead of a 500.
            throw new DuplicateEmailException();
        }
    }
}
