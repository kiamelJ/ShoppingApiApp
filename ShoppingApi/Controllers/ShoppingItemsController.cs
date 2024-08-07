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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShoppingItem>>> GetShoppingItems()
        {
            var items = await _shoppingItemsCollection.Find(item => true).ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id:length(24)}", Name = "GetShoppingItem")]
        public async Task<ActionResult<ShoppingItem>> GetShoppingItem(string id)
        {
            var item = await _shoppingItemsCollection.Find<ShoppingItem>(item => item.Id == id).FirstOrDefaultAsync();
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

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

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateShoppingItem(string id, [FromBody] ShoppingItemDTO itemDto)
        {
            if (itemDto == null)
            {
                return BadRequest("Item cannot be null.");
            }

            var updatedItem = new ShoppingItem
            {
                Id = id,
                Name = itemDto.Name,
                IsComplete = itemDto.IsComplete,
                Position = itemDto.Position
            };

            var result = await _shoppingItemsCollection.ReplaceOneAsync(item => item.Id == id, updatedItem);
            if (result.MatchedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }

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
    }
}
