using TeachingAPI.Services;

namespace TeachingAPI.Tests;

public class UserServiceTests
{
    [Fact]
    public void Register_CreatesUserWithDefaultRole_AndHashesPassword()
    {
        var svc = new InMemoryUserService();
        var user = svc.Register("alice", "secret1");

        Assert.NotNull(user);
        Assert.Equal("alice", user!.Username);
        Assert.Equal("User", user.Role);
        Assert.NotEqual("secret1", user.PasswordHash);
        Assert.False(string.IsNullOrEmpty(user.PasswordHash));
    }

    [Fact]
    public void Register_ReturnsNull_ForDuplicateUsername_CaseInsensitive()
    {
        var svc = new InMemoryUserService();
        Assert.NotNull(svc.Register("Bob", "secret1"));
        Assert.Null(svc.Register("bob", "other123"));
    }

    [Fact]
    public void Register_AssignsDistinctIds()
    {
        var svc = new InMemoryUserService();
        var a = svc.Register("aaa", "secret1")!;
        var b = svc.Register("bbb", "secret1")!;
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void ValidateCredentials_ReturnsUser_ForCorrectPassword()
    {
        var svc = new InMemoryUserService();
        svc.Register("carol", "secret1");
        Assert.Equal("carol", svc.ValidateCredentials("carol", "secret1")?.Username);
    }

    [Fact]
    public void ValidateCredentials_IsCaseInsensitiveOnUsername()
    {
        var svc = new InMemoryUserService();
        svc.Register("carol", "secret1");
        Assert.NotNull(svc.ValidateCredentials("CAROL", "secret1"));
    }

    [Fact]
    public void ValidateCredentials_ReturnsNull_ForWrongPassword()
    {
        var svc = new InMemoryUserService();
        svc.Register("dave", "secret1");
        Assert.Null(svc.ValidateCredentials("dave", "wrong-pass"));
    }

    [Fact]
    public void ValidateCredentials_ReturnsNull_ForUnknownUser()
    {
        Assert.Null(new InMemoryUserService().ValidateCredentials("nobody", "secret1"));
    }

    [Fact]
    public void SeededAdmin_CanLogIn_WithAdminRole()
    {
        var admin = new InMemoryUserService().ValidateCredentials("admin", "Admin123!");
        Assert.Equal("Admin", admin?.Role);
    }
}
