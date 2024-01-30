using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarehouseManagement
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class FileManager<T>
    {
        protected string FilePath;

        public FileManager(string filePath)
        {
            FilePath = filePath;
        }

        protected virtual string Serialize(T entity) => string.Empty;
        protected virtual T Deserialize(string line) => default;

        public List<T> ReadFromFile()
        {
            if (File.Exists(FilePath))
            {
                return File.ReadAllLines(FilePath)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(Deserialize)
                    .ToList();
            }
            return new List<T>();
        }

        public void WriteToFile(List<T> entities)
        {
            var lines = entities.Select(Serialize);
            File.WriteAllLines(FilePath, lines);
        }
    }

    public class ProductFileManager : FileManager<Product>
    {
        public ProductFileManager(string filePath) : base(filePath) { }

        protected override string Serialize(Product product)
        {
            return $"{product.Id},{product.Name},{product.ExpiryDate:yyyy-MM-dd}";
        }

        protected override Product Deserialize(string line)
        {
            var productDetails = line.Split(',');
            if (productDetails.Length == 3 &&
                int.TryParse(productDetails[0], out int id) &&
                DateTime.TryParse(productDetails[2], out DateTime expiryDate))
            {
                return new Product
                {
                    Id = id,
                    Name = productDetails[1],
                    ExpiryDate = expiryDate
                };
            }
            return null;
        }
    }

    public class UserManager
    {
        private List<User> users = new List<User>();
        private FileManager<User> userFileManager;

        public UserManager(string filePath)
        {
            userFileManager = new FileManager<User>(filePath);
        }

        public void LoadData()
        {
            users = userFileManager.ReadFromFile();
        }

        public void SaveData()
        {
            userFileManager.WriteToFile(users);
        }

        public void CreateUser(User user)
        {
            user.Id = users.Count + 1;
            users.Add(user);
            SaveData();
        }
    }

    public class WarehouseManager
    {
        private List<Product> products = new List<Product>();
        private List<Category> categories = new List<Category>();
        private ProductFileManager productFileManager;

        public WarehouseManager(string filePath)
        {
            productFileManager = new ProductFileManager(filePath);
        }

        public void LoadData()
        {
            products = productFileManager.ReadFromFile();
        }

        public void SaveData()
        {
            productFileManager.WriteToFile(products);
        }

        public void CreateProduct(Product product)
        {
            product.Id = products.Count + 1;
            products.Add(product);
            SaveData();
        }

        public Product ReadProduct(int id)
        {
            return products.Find(p => p.Id == id);
        }

        public void UpdateProduct(Product updatedProduct)
        {
            var existingProduct = products.Find(p => p.Id == updatedProduct.Id);
            if (existingProduct != null)
            {
                existingProduct.Name = updatedProduct.Name;
                existingProduct.ExpiryDate = updatedProduct.ExpiryDate;
                SaveData();
            }
        }

        public void DeleteProduct(int id)
        {
            products.RemoveAll(p => p.Id == id);
            SaveData();
        }

        public void CreateCategory(Category category)
        {
            category.Id = categories.Count + 1;
            categories.Add(category);
        }
    }

    internal class Program
    {
        static UserManager userManager;
        static WarehouseManager warehouseManager;

        static void Main(string[] args)
        {
            userManager = new UserManager("D:\\Programming\\ProgrammingBasics\\ProjectsC#\\projectOOP_3.0_Vitalik\\users.txt");
            userManager.LoadData();

            warehouseManager = new WarehouseManager("D:\\Programming\\ProgrammingBasics\\ProjectsC#\\projectOOP_3.0_Vitalik\\products.txt");
            warehouseManager.LoadData();

            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Create Product");
                Console.WriteLine("2. Read Product");
                Console.WriteLine("3. Update Product");
                Console.WriteLine("4. Delete Product");
                Console.WriteLine("5. Create User");
                Console.WriteLine("0. Exit");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateProduct();
                        break;

                    case "2":
                        ReadProduct();
                        break;

                    case "3":
                        UpdateProduct();
                        break;

                    case "4":
                        DeleteProduct();
                        break;

                    case "5":
                        CreateUser();
                        break;

                    case "0":
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please enter a valid option.");
                        break;
                }
            }
        }

        static void CreateProduct()
        {
            Console.WriteLine("Enter product name:");
            var name = Console.ReadLine();

            Console.WriteLine("Enter expiry date (yyyy-mm-dd):");
            if (DateTime.TryParse(Console.ReadLine(), out var expiryDate))
            {
                var newProduct = new Product
                {
                    Name = name,
                    ExpiryDate = expiryDate
                };

                warehouseManager.CreateProduct(newProduct);
                Console.WriteLine("Product created successfully.");
            }
            else
            {
                Console.WriteLine("Invalid date format. Product creation failed.");
            }
        }

        static void ReadProduct()
        {
            Console.WriteLine("Enter product ID:");
            if (int.TryParse(Console.ReadLine(), out var productId))
            {
                var retrievedProduct = warehouseManager.ReadProduct(productId);
                if (retrievedProduct != null)
                {
                    Console.WriteLine($"Retrieved Product: {retrievedProduct.Name}, Expiry Date: {retrievedProduct.ExpiryDate:yyyy-MM-dd}");
                }
                else
                {
                    Console.WriteLine("Product not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid product ID.");
            }
        }

        static void UpdateProduct()
        {
            Console.WriteLine("Enter product ID to update:");
            if (int.TryParse(Console.ReadLine(), out var productId))
            {
                var existingProduct = warehouseManager.ReadProduct(productId);
                if (existingProduct != null)
                {
                    Console.WriteLine("Enter updated product name:");
                    var updatedName = Console.ReadLine();

                    Console.WriteLine("Enter updated expiry date (yyyy-mm-dd):");
                    if (DateTime.TryParse(Console.ReadLine(), out var updatedExpiryDate))
                    {
                        var updatedProduct = new Product
                        {
                            Id = productId,
                            Name = updatedName,
                            ExpiryDate = updatedExpiryDate
                        };

                        warehouseManager.UpdateProduct(updatedProduct);
                        Console.WriteLine("Product updated successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid date format. Product update failed.");
                    }
                }
                else
                {
                    Console.WriteLine("Product not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid product ID.");
            }
        }

        static void DeleteProduct()
        {
            Console.WriteLine("Enter product ID to delete:");
            if (int.TryParse(Console.ReadLine(), out var productId))
            {
                warehouseManager.DeleteProduct(productId);
                Console.WriteLine("Product deleted successfully.");
            }
            else
            {
                Console.WriteLine("Invalid product ID.");
            }
        }

        static void CreateUser()
        {
            Console.WriteLine("Enter user name:");
            var userName = Console.ReadLine();

            var newUser = new User { Name = userName };
            userManager.CreateUser(newUser);
            Console.WriteLine("User created successfully.");
        }
    }
}

