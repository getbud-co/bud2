using Bud.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Bud.Infrastructure.UnitTests.Infrastructure.Persistence;

public sealed class EfUnitOfWorkTests
{
    [Fact]
    public async Task CommitAsync_WhenEntitiesHaveDomainEvents_DispatchesAndClearsEvents()
    {
        await using var connection = await CreateOpenConnectionAsync();
        var options = CreateOptions(connection);

        await using var dbContext = await CreateContextAsync(options);
        var dispatcherMock = new Mock<IDomainEventDispatcher>();
        IReadOnlyCollection<IDomainEvent>? dispatchedEvents = null;
        dispatcherMock
            .Setup(d => d.DispatchAsync(It.IsAny<IReadOnlyCollection<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<IDomainEvent>, CancellationToken>((events, _) => dispatchedEvents = events)
            .Returns(Task.CompletedTask);

        var organizationId = Guid.NewGuid();
        dbContext.Organizations.Add(new Organization
        {
            Id = organizationId,
            Name = "org-evento.com"
        });

        var mission = Mission.Create(
            Guid.NewGuid(),
            organizationId,
            "Missão com evento",
            "Descrição",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            MissionStatus.Planned);

        dbContext.Missions.Add(mission);
        var unitOfWork = new EfUnitOfWork(dbContext, dispatcherMock.Object);

        await unitOfWork.CommitAsync(CancellationToken.None);

        dispatcherMock.Verify(
            d => d.DispatchAsync(It.IsAny<IReadOnlyCollection<IDomainEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        dispatchedEvents.Should().NotBeNull();
        var dispatchedEvent = dispatchedEvents!.Should().ContainSingle().Subject;
        var created = dispatchedEvent.Should().BeOfType<MissionCreatedDomainEvent>().Subject;
        created.MissionId.Should().Be(mission.Id);
        created.OrganizationId.Should().Be(organizationId);
        mission.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task CommitAsync_WhenDispatcherFails_RollsBackPersistedChanges()
    {
        await using var connection = await CreateOpenConnectionAsync();
        var options = CreateOptions(connection);

        await using (var dbContext = await CreateContextAsync(options))
        {
            var dispatcherMock = new Mock<IDomainEventDispatcher>();
            dispatcherMock
                .Setup(d => d.DispatchAsync(It.IsAny<IReadOnlyCollection<IDomainEvent>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Falha no handler."));

            var organizationId = Guid.NewGuid();
            dbContext.Organizations.Add(new Organization
            {
                Id = organizationId,
                Name = "org-rollback.com"
            });

            var mission = Mission.Create(
                Guid.NewGuid(),
                organizationId,
                "Missão com rollback",
                "Descrição",
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                MissionStatus.Planned);

            dbContext.Missions.Add(mission);
            var unitOfWork = new EfUnitOfWork(dbContext, dispatcherMock.Object);

            var act = () => unitOfWork.CommitAsync(CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Falha no handler.");
        }

        await using var verificationContext = await CreateContextAsync(options);
        (await verificationContext.Missions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_WhenHandlerPersistsNotifications_UsesSameTransactionWithoutRedispatch()
    {
        await using var connection = await CreateOpenConnectionAsync();
        var options = CreateOptions(connection);

        await using var dbContext = await CreateContextAsync(options);
        var organizationId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        dbContext.Organizations.Add(new Organization
        {
            Id = organizationId,
            Name = "org-transacao.com"
        });
        dbContext.Employees.Add(new Employee { Id = employeeId, FullName = "Colaborador", Email = "colaborador@org-transacao.com" });
        dbContext.Memberships.Add(new Membership
        {
            EmployeeId = employeeId,
            OrganizationId = organizationId,
            Role = EmployeeRole.Contributor
        });
        await dbContext.SaveChangesAsync();

        EfUnitOfWork? unitOfWork = null;
        var dispatcher = new NestedSaveDispatcher(
            dbContext,
            employeeId,
            organizationId,
            () => unitOfWork!);

        var mission = Mission.Create(
            Guid.NewGuid(),
            organizationId,
            "Missão com notificação",
            "Descrição",
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            MissionStatus.Planned);

        dbContext.Missions.Add(mission);
        unitOfWork = new EfUnitOfWork(dbContext, dispatcher);

        await unitOfWork.CommitAsync(CancellationToken.None);

        dispatcher.DispatchCount.Should().Be(1);
        (await dbContext.Missions.CountAsync()).Should().Be(1);
        var notifications = await dbContext.Notifications.ToListAsync();
        notifications.Should().ContainSingle();
        notifications[0].RecipientEmployeeId.Should().Be(employeeId);
        notifications[0].OrganizationId.Should().Be(organizationId);
    }

    private static DbContextOptions<ApplicationDbContext> CreateOptions(SqliteConnection connection)
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

    private static async Task<SqliteConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<ApplicationDbContext> CreateContextAsync(DbContextOptions<ApplicationDbContext> options)
    {
        var dbContext = new ApplicationDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        return dbContext;
    }

    private sealed class NestedSaveDispatcher(
        ApplicationDbContext dbContext,
        Guid recipientEmployeeId,
        Guid organizationId,
        Func<EfUnitOfWork> unitOfWorkAccessor) : IDomainEventDispatcher
    {
        public int DispatchCount { get; private set; }

        public async Task DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default)
        {
            DispatchCount++;
            domainEvents.Should().ContainSingle();

            dbContext.Notifications.Add(Notification.Create(
                Guid.NewGuid(),
                recipientEmployeeId,
                organizationId,
                "Nova meta criada",
                "Uma nova notificação foi gerada.",
                NotificationType.MissionCreated,
                DateTime.UtcNow,
                relatedEntityType: "Mission"));

            await unitOfWorkAccessor().CommitAsync(cancellationToken);
        }
    }
}
