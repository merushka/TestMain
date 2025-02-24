using System;
using System.Collections.Generic;
using WebApplication.Models;
using LinqToDB;
using WebApplication.Data;
using LinqToDB.Data;
using System.Data;

namespace WebApplication.Data
{
    public static class DatabaseSeeder

    {
        public static void SeedAll(DatabaseContext db)
        {
            Console.WriteLine("🔄 Seeding database...");
            db.Execute("ALTER SESSION SET CURRENT_SCHEMA = MYUSER");

            Console.WriteLine("✅ Установлена схема MYUSER");


            using (var tran = db.BeginTransaction())
            {
                try
                {
                    ClearDatabase(db);
                    ResetIdentity(db);
                    SeedCustomers(db);
                    SeedProducts(db);

                    // Добавляем COMMIT после вставки продуктов
                    db.Execute("COMMIT");
                    Console.WriteLine("✅ Данные о продуктах зафиксированы в БД!");

                    SeedOrders(db);
                    SeedOrderItems(db);

                    tran.Commit();
                    Console.WriteLine("✅ Database seeding completed!");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Console.WriteLine($"❌ Ошибка сидирования данных: {ex.Message}");
                }
            }
        }


        private static void ClearDatabase(DatabaseContext db)
        {
            Console.WriteLine("🗑️ Очистка базы данных...");
            db.Execute("DELETE FROM ORDERITEMS");
            db.Execute("DELETE FROM ORDERS");
            db.Execute("DELETE FROM PRODUCTS");
            db.Execute("DELETE FROM CUSTOMERS");

            db.Execute("COMMIT");
            Console.WriteLine("✅ Очистка завершена!");
        }

        private static void ResetIdentity(DatabaseContext db)
        {
            Console.WriteLine("🔄 Сброс идентификаторов...");
            db.Execute("ALTER TABLE CUSTOMERS MODIFY ID GENERATED ALWAYS AS IDENTITY (RESTART START WITH 1)");
            db.Execute("ALTER TABLE PRODUCTS MODIFY ID GENERATED ALWAYS AS IDENTITY (RESTART START WITH 1)");
            db.Execute("ALTER TABLE ORDERS MODIFY ID GENERATED ALWAYS AS IDENTITY (RESTART START WITH 1)");
            db.Execute("ALTER TABLE ORDERITEMS MODIFY ID GENERATED ALWAYS AS IDENTITY (RESTART START WITH 1)");
        }

        private static void SeedCustomers(DatabaseContext db)
        {
            Console.WriteLine("🔄 Seeding Customers...");

            for (int i = 1; i <= 100; i++)
            {
                var customer = new Customer
                {
                    Name = $"User{i}",
                    Email = $"user{i}@example.com"
                };

                var newId = db.InsertWithInt32Identity(customer);
                customer.Id = newId;

                Console.WriteLine($"✅ Добавлено: Name={customer.Name}, Email={customer.Email}, ID={customer.Id}");
            }
        }

        private static void SeedProducts(DatabaseContext db)
        {
            Console.WriteLine("🔄 Seeding Products...");

            var rand = new Random();
            for (int i = 1; i <= 100; i++)
            {
                var product = new Product
                {
                    Name = $"Product{i}",
                    Price = rand.Next(10, 500),
                    Quantity = rand.Next(1, 100)
                };

                var newId = db.InsertWithInt32Identity(product);

                if (newId > 0)
                {
                    Console.WriteLine($"✅ Вставлено: Name={product.Name}, Price={product.Price}, Quantity={product.Quantity}, ID={newId}");
                }
                else
                {
                    Console.WriteLine($"❌ Ошибка вставки: {product.Name}");
                }
            }

            // Явный коммит после вставки продуктов
            db.Execute("COMMIT");
            Console.WriteLine("✅ Данные о продуктах зафиксированы в БД!");
        }


        private static void SeedOrders(DatabaseContext db)
        {
            Console.WriteLine("🔄 Seeding Orders...");

            var allCustIds = db.Customers.Select(c => c.Id).ToList();
            if (allCustIds.Count == 0)
            {
                Console.WriteLine("⚠ Ошибка: Нет клиентов для заказов!");
                return;
            }

            var rand = new Random();
            for (int i = 1; i <= 50; i++)
            {
                var customerId = allCustIds[rand.Next(allCustIds.Count)];

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now.AddDays(-i)
                };

                var newId = db.InsertWithInt32Identity(order);
                order.Id = newId;

                Console.WriteLine($"✅ Добавлено: OrderID={order.Id}, CustomerID={order.CustomerId}, Date={order.OrderDate}");
            }
        }

        private static void SeedOrderItems(DatabaseContext db)
        {
            Console.WriteLine("🔄 Seeding Order Items...");

            var allOrderIds = db.Orders.Select(o => o.Id).ToList();
            var allProductIds = db.Products.Select(p => p.Id).ToList();

            if (allOrderIds.Count == 0 || allProductIds.Count == 0)
            {
                Console.WriteLine("⚠ Ошибка: Нет заказов или товаров!");
                return;
            }

            var rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                int orderId = allOrderIds[rand.Next(allOrderIds.Count)];
                int productId = allProductIds[rand.Next(allProductIds.Count)];
                int quantity = rand.Next(1, 5);

                // 📌 Получаем цену товара из БД (а не из запроса!)
                decimal price = db.Products
                    .Where(p => p.Id == productId)
                    .Select(p => p.Price)
                    .FirstOrDefault();

                Console.WriteLine($"📌 Проверка перед INSERT: OrderID={orderId}, ProductID={productId}, Quantity={quantity}, Price={price}");

                // Создаем объект заказа
                var item = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = price // Используем цену из БД
                };

                var newId = db.InsertWithInt32Identity(item);

                Console.WriteLine($"✅ Добавлено: OrderID={orderId}, ProductID={productId}, Quantity={quantity}, Price={price}, ID={newId}");
            }
        }

    }
}

