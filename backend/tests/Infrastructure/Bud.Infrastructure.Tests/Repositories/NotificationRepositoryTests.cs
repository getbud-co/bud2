using Bud.Infrastructure.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace Bud.Infrastructure.Tests.Repositories;

public sealed class NotificationRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _recipientId = Guid.NewGuid();

    public NotificationRepositoryTests()
    {
        var provider = new TestTenantProvider { TenantId = _tenantId };
        var (ctx, conn) = SqliteDbContextFactory.Create(provider);
        _connection = conn;

        // Seed required FK entities
        ctx.Organizations.Add(Organization.Create(_tenantId, OrganizationDomainName.Create("empresa.com")));
        ctx.Employees.Add(Employee.Create(_recipientId, _tenantId,
            EmployeeName.Create("Recipient"), EmailAddress.Create("recipient@bud.co"), EmployeeRole.IndividualContributor));
        ctx.SaveChanges();
        ctx.Dispose();
    }

    private ApplicationDbContext CreateContext() =>
        SqliteDbContextFactory.CreateWithConnection(_connection,
            new TestTenantProvider { TenantId = _tenantId });

    private Notification MakeNotification(bool read = false, string title = "Teste")
    {
        var n = Notification.Create(
            Guid.NewGuid(), _recipientId, _tenantId,
            title, "Mensagem de teste", "system.info", DateTime.UtcNow);
        if (read)
        {
            n.MarkAsRead(DateTime.UtcNow);
        }

        return n;
    }

    [Fact]
    public async Task AddRangeAsync_And_GetByIdAsync_ShouldPersistAndRetrieve()
    {
        var notification = MakeNotification();

        await using (var ctx = CreateContext())
        {
            var repo = new NotificationRepository(ctx);
            await repo.AddRangeAsync([notification]);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var repo = new NotificationRepository(ctx);
            var found = await repo.GetByIdAsync(notification.Id);

            found.Should().NotBeNull();
            found!.Title.Should().Be("Teste");
            found.IsRead.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetByRecipientAsync_ShouldReturnPaged()
    {
        await using (var ctx = CreateContext())
        {
            var repo = new NotificationRepository(ctx);
            await repo.AddRangeAsync([
                MakeNotification(title: "A"),
                MakeNotification(title: "B"),
                MakeNotification(title: "C")
            ]);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var repo = new NotificationRepository(ctx);
            var result = await repo.GetByRecipientAsync(_recipientId, null, 1, 2);

            result.Total.Should().Be(3);
            result.Items.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetByRecipientAsync_WithIsReadFilter_ShouldFilterCorrectly()
    {
        await using (var ctx = CreateContext())
        {
            var repo = new NotificationRepository(ctx);
            await repo.AddRangeAsync([
                MakeNotification(read: false, title: "Unread"),
                MakeNotification(read: true, title: "Read")
            ]);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var repo = new NotificationRepository(ctx);

            var unread = await repo.GetByRecipientAsync(_recipientId, false, 1, 10);
            unread.Items.Should().OnlyContain(n => !n.IsRead);

            var read = await repo.GetByRecipientAsync(_recipientId, true, 1, 10);
            read.Items.Should().OnlyContain(n => n.IsRead);
        }
    }

    [Fact]
    public async Task GetUnreadByRecipientAsync_ShouldReturnOnlyUnread()
    {
        await using (var ctx = CreateContext())
        {
            var repo = new NotificationRepository(ctx);
            await repo.AddRangeAsync([
                MakeNotification(read: false, title: "Unread"),
                MakeNotification(read: true, title: "Read")
            ]);
            await repo.SaveChangesAsync();
        }

        await using (var ctx = CreateContext())
        {
            var repo = new NotificationRepository(ctx);
            var result = await repo.GetUnreadByRecipientAsync(_recipientId);

            result.Should().HaveCount(1);
            result.Should().OnlyContain(n => !n.IsRead);
        }
    }

    public void Dispose() => _connection.Dispose();
}
