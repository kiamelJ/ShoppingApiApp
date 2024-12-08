using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShoppingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingItemsController : ControllerBase
    {
        private readonly IMongoCollection<ShoppingItem> _shoppingItemsCollection;

        public ShoppingItemsController(IMongoCollection<ShoppingItem> shoppingItemsCollection)
        {
            _shoppingItemsCollection = shoppingItemsCollection;
        }

        // GET: api/shoppingitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShoppingItem>>> GetShoppingItems()
        {
            var items = await _shoppingItemsCollection
                .Find(item => true)
                .SortBy(item => item.Position)
                .ToListAsync();
            return Ok(items);
        }

        // GET: api/shoppingitems/{id}
        [HttpGet("{id:length(24)}", Name = "GetShoppingItem")]
        public async Task<ActionResult<ShoppingItem>> GetShoppingItem(string id)
        {
            var item = await _shoppingItemsCollection.Find(item => item.Id == id).FirstOrDefaultAsync();
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        // POST: api/shoppingitems
        [HttpPost]
        public async Task<ActionResult<ShoppingItem>> CreateShoppingItem([FromBody] ShoppingItemDTO itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest("Item cannot be null.");
            }

            var item = new ShoppingItem
            {
                Name = itemDto.Name,
                IsComplete = itemDto.IsComplete,
                Position = itemDto.Position
            };

            await _shoppingItemsCollection.InsertOneAsync(item);
            return CreatedAtRoute("GetShoppingItem", new { id = item.Id }, item);
        }

        // PUT: api/shoppingitems/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateShoppingItem(string id, [FromBody] ShoppingItemDTO itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest("Item cannot be null.");
            }

            var updatedItem = new ShoppingItem
            {
                Id = id, // Use string ID directly if Id is a string
                Name = itemDto.Name,
                IsComplete = itemDto.IsComplete,
                Position = itemDto.Position
            };

            var result = await _shoppingItemsCollection.ReplaceOneAsync(item => item.Id == id, updatedItem);

            if (result.MatchedCount == 0)
            {
                return NotFound();
            }

            return Ok(updatedItem);
        }

        // DELETE: api/shoppingitems/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeleteShoppingItem(string id)
        {
            var result = await _shoppingItemsCollection.DeleteOneAsync(item => item.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }

        // PUT: api/shoppingitems/updateOrder
        [HttpPut("updateOrder")]
        public async Task<IActionResult> UpdateOrder([FromBody] List<ShoppingItem> reorderedItems)
        {
            if (reorderedItems == null || !reorderedItems.Any())
            {
                return BadRequest("Invalid item list.");
            }

            var bulkOps = new List<WriteModel<ShoppingItem>>();

            foreach (var item in reorderedItems)
            {
                var filter = Builders<ShoppingItem>.Filter.Eq(x => x.Id, item.Id);
                var update = Builders<ShoppingItem>.Update.Set(x => x.Position, item.Position);
                var updateOne = new UpdateOneModel<ShoppingItem>(filter, update);
                bulkOps.Add(updateOne);
            }

            if (bulkOps.Any())
            {
                await _shoppingItemsCollection.BulkWriteAsync(bulkOps);
            }

            return NoContent();
        }
    }
}
