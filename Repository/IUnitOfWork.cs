namespace InventoryManagementSystem.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        Task<int> SaveChangesAsync();
    }
}
