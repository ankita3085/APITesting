using TeachingAPI.Dtos;
using TeachingAPI.Services;

namespace TeachingAPI.Tests;

public class ProductServiceTests
{
    private static ProductRequest Req(string name = "Widget", decimal price = 9.99m, int stock = 3) =>
        new() { Name = name, Description = "desc", Price = price, Stock = stock };

    [Fact]
    public void Constructor_SeedsTwoProducts()
    {
        var svc = new InMemoryProductService();
        Assert.Equal(2, svc.GetAll().Count());
    }

    [Fact]
    public void GetAll_ReturnsProductsOrderedById()
    {
        var svc = new InMemoryProductService();
        svc.Create(Req(), "tester");
        var ids = svc.GetAll().Select(p => p.Id).ToList();
        Assert.Equal(ids.OrderBy(i => i), ids);
    }

    [Fact]
    public void Create_AssignsIncrementingIdsAndCopiesFields()
    {
        var svc = new InMemoryProductService();
        var a = svc.Create(Req("A"), "alice");
        var b = svc.Create(Req("B"), "bob");

        Assert.Equal(a.Id + 1, b.Id);
        Assert.Equal("A", a.Name);
        Assert.Equal("alice", a.CreatedBy);
        Assert.Equal(9.99m, a.Price);
        Assert.Equal(3, a.Stock);
    }

    [Fact]
    public void GetById_ReturnsProduct_WhenExists()
    {
        var svc = new InMemoryProductService();
        var created = svc.Create(Req(), "x");
        Assert.Same(created, svc.GetById(created.Id));
    }

    [Fact]
    public void GetById_ReturnsNull_WhenMissing()
    {
        Assert.Null(new InMemoryProductService().GetById(9999));
    }

    [Fact]
    public void Update_ChangesFields_WhenExists()
    {
        var svc = new InMemoryProductService();
        var created = svc.Create(Req(), "x");

        var updated = svc.Update(created.Id, Req("New", 1.5m, 7));

        Assert.NotNull(updated);
        Assert.Equal("New", updated!.Name);
        Assert.Equal(1.5m, updated.Price);
        Assert.Equal(7, updated.Stock);
        Assert.Equal("x", updated.CreatedBy);
    }

    [Fact]
    public void Update_ReturnsNull_WhenMissing()
    {
        Assert.Null(new InMemoryProductService().Update(9999, Req()));
    }

    [Fact]
    public void Delete_RemovesProduct_AndReturnsTrue()
    {
        var svc = new InMemoryProductService();
        var created = svc.Create(Req(), "x");

        Assert.True(svc.Delete(created.Id));
        Assert.Null(svc.GetById(created.Id));
    }

    [Fact]
    public void Delete_ReturnsFalse_WhenMissing()
    {
        Assert.False(new InMemoryProductService().Delete(9999));
    }

    [Fact]
    public void Create_ParallelCalls_ProduceUniqueIds()
    {
        var svc = new InMemoryProductService();
        var ids = new System.Collections.Concurrent.ConcurrentBag<int>();

        Parallel.For(0, 200, _ => ids.Add(svc.Create(Req(), "p").Id));

        Assert.Equal(200, ids.Distinct().Count());
    }
}
