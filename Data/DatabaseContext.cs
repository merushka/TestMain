using LinqToDB;
using LinqToDB.Data;
using WebApplication.Models;

namespace WebApplication.Data
{
    public class DatabaseContext : DataConnection
    {
        public DatabaseContext(string connectionString)
            : base(ProviderName.OracleManaged, connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("❌ Ошибка: строка подключения не задана!");
            }
        }

        public ITable<Customer> Customers => this.GetTable<Customer>();
        public ITable<Product> Products => this.GetTable<Product>();
        public ITable<Order> Orders => this.GetTable<Order>();
        public ITable<OrderItem> OrderItems => this.GetTable<OrderItem>();
    }
}