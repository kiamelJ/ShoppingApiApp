using MongoDB.Driver;
using ShoppingApi.Models;

public class ShoppingContext
{
    private readonly IMongoDatabase _database;

    public ShoppingContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<ShoppingItem> ShoppingItems => _database.GetCollection<ShoppingItem>("ShoppingItems");
}

