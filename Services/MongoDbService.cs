using MongoDB.Driver;
using SmartMiniStore.Models;

namespace SmartMiniStore.Services
{
    /// <summary>
    /// Central MongoDB connection. Everything that talks to MongoDB
    /// goes through this single class. There is exactly ONE connection string
    /// in the entire project, and it lives here.
    /// </summary>
    public class MongoDbService
    {
        // The one and only place where the connection string is defined.
        private const string ConnectionString = "mongodb://localhost:27017";
        private const string DatabaseName = "SmartMiniStoreDB";

        private readonly IMongoDatabase _database;

        public bool IsConnected { get; private set; }
        public string? ConnectionError { get; private set; }

        public MongoDbService()
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(ConnectionString);

                // Fail fast if the server is not reachable.
                settings.ServerSelectionTimeout = System.TimeSpan.FromSeconds(3);

                var client = new MongoClient(settings);

                // Force a real server call so we know if Mongo is running.
                client.ListDatabaseNames().ToList();

                _database = client.GetDatabase(DatabaseName);
                IsConnected = true;
            }
            catch (System.Exception ex)
            {
                IsConnected = false;
                ConnectionError = ex.Message;
                // We still set _database to a dummy to satisfy the compiler.
                // It will never be used because IsConnected is false.
                _database = null!;
            }
        }

        public IMongoCollection<Product> Products
        {
            get
            {
                EnsureConnected();
                return _database.GetCollection<Product>("Products");
            }
        }

        public IMongoCollection<Category> Categories
        {
            get
            {
                EnsureConnected();
                return _database.GetCollection<Category>("Categories");
            }
        }

        public IMongoCollection<Sale> Sales
        {
            get
            {
                EnsureConnected();
                return _database.GetCollection<Sale>("Sales");
            }
        }

        private void EnsureConnected()
        {
            if (!IsConnected)
                throw new System.InvalidOperationException(
                    "Unable to connect to MongoDB. Please make sure MongoDB is running.");
        }
    }
}