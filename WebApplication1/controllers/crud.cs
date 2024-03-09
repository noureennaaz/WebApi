using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebApplication1.Models;
using System.Text.Json;

using MongoDB.Bson;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IMongoDatabase _database;

        public ValuesController(IMongoDatabase database)
        {
            _database = database;
        }

        [HttpPost("create-entity")]
        public async Task<IActionResult> CreateEntity([FromBody] JsonElement json)
        {

            try
            {

                var Entities = _database.GetCollection<Entity>("entity");
                if (json.ValueKind != JsonValueKind.Object)
                {
                    return BadRequest("Invalid JSON input: Expected an object");
                }

                string gender = json.GetProperty("gender").GetString().ToLower();

                JsonElement namesArray = json.GetProperty("names");

                if (namesArray.ValueKind != JsonValueKind.Array)
                {
                    return BadRequest("Invalid JSON input: 'names' property is not an array");
                }
                var NamesList = new List<Name>();
                foreach (JsonElement nameElement in namesArray.EnumerateArray())
                {
                    // Access properties of each name object
                    string firstName = nameElement.GetProperty("firstName").GetString();
                    string middleName = nameElement.GetProperty("middleName").GetString();
                    string surname = nameElement.GetProperty("surname").GetString();
                    var newName = new Name
                    {
                        FirstName = firstName,
                        MiddleName = middleName,
                        Surname = surname
                    };

                    NamesList.Add(newName);
                }

                JsonElement addressArray = json.GetProperty("addresses");
                var addressList = new List<Address>();
                foreach (JsonElement addressElement in addressArray.EnumerateArray())
                {
                    // Access properties of each name object
                    string addressline = addressElement.GetProperty("addressLine").GetString();
                    string city = addressElement.GetProperty("city").GetString();
                    string country = addressElement.GetProperty("country").GetString();
                    var newAddress = new Address
                    {
                        AddressLine = addressline,
                        City = city,
                        Country = country
                    };

                    addressList.Add(newAddress);
                }

                JsonElement dateArray = json.GetProperty("dates");
                var dateList = new List<Date>();
                var currentDate = new Date
                {
                    DateOnly = DateTime.Now,
                    DateType = "registered on:"
                };
                dateList.Add(currentDate);
                foreach (JsonElement dateElement in dateArray.EnumerateArray())
                {
                    // Access properties of each name object
                    string dateType = dateElement.GetProperty("dateType").GetString();
                    DateTime dateOnly = dateElement.GetProperty("dateOnly").GetDateTime();

                    var newDate = new Date
                    {
                        DateOnly = dateOnly,
                        DateType = dateType
                    };

                    dateList.Add(newDate);
                }

                var newEntity = new Entity
                {
                    Addresses = addressList,
                    Dates = dateList,
                    Deceased = false,
                    Gender = gender,
                    Names = NamesList
                };

                Entities.InsertOne(newEntity);
                return Ok(newEntity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("get-all-entities")]
        public async Task<IActionResult> GetAllEntities()
        {
            try
            {
                var Entities = _database.GetCollection<Entity>("entity");

                var entities = await Entities.Find(_ => true).ToListAsync();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("get-entity-by-id/{id}")]
        public async Task<IActionResult> GetEntityById(string id)
        {
            try
            {
                var Entities = _database.GetCollection<Entity>("entity");
                // ObjectId objectId = ObjectId.Parse(id);
                var entity = await Entities.Find(e => e.Id == id).FirstOrDefaultAsync();
                if (entity == null)
                {
                    return NotFound();
                }
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchEntities([FromQuery] string search)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(search))
                {
                    return BadRequest("Search query cannot be empty");
                }
                var searchStr = search.ToLower();
                var Entities = _database.GetCollection<Entity>("entity");
                var filter = Builders<Entity>.Filter.Or(
                    Builders<Entity>.Filter.ElemMatch(entity => entity.Names,
                        name => name.FirstName.ToLower().Contains(searchStr) || name.MiddleName.ToLower().Contains(searchStr) || name.Surname.ToLower().Contains(searchStr)),
                    Builders<Entity>.Filter.ElemMatch(entity => entity.Addresses,
                        address => address.AddressLine.ToLower().Contains(searchStr) || address.City.ToLower().Contains(searchStr) || address.Country.ToLower().Contains(searchStr))
                );

                // Execute the search query
                List<Entity> results = await Entities.Find(filter).ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> SearchEntities([FromQuery] string? gender, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? countries)
        {
            try
            {
                var filterBuilder = Builders<Entity>.Filter;
                var filter = filterBuilder.Empty;
                var Entities = _database.GetCollection<Entity>("entity");

                if (!string.IsNullOrEmpty(gender))
                {
                    string Gender = gender.ToLower().Trim(' ');
                    filter &= filterBuilder.Eq(entity => entity.Gender, Gender);
                }

                if (startDate.HasValue && endDate.HasValue)
                {
                    filter &= filterBuilder.Where(entity => entity.Dates.Any(date => date.DateOnly >= startDate.Value.Date && date.DateOnly <= endDate.Value.Date));
                }
                else if (startDate.HasValue)
                {
                    DateTime endDateMidnight = endDate ?? DateTime.MaxValue;
                    filter &= filterBuilder.Where(entity => entity.Dates.Any(date => date.DateOnly >= startDate.Value.Date && date.DateOnly <= endDateMidnight.Date));
                }
                else if (endDate.HasValue)
                {
                    filter &= filterBuilder.Where(entity => entity.Dates.Any(date => date.DateOnly <= endDate.Value.Date));
                }


                if (!string.IsNullOrEmpty(countries))
                {
                    var countryArray = countries.Split(',').Select(country => country.Trim());
                    filter &= filterBuilder.In(entity => entity.Addresses.First().Country, countryArray);
                }

                List<Entity> results = await Entities.Find(filter).ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteEntity(string id)
        {

            try
            {
                var Entities = _database.GetCollection<Entity>("entity");
                // ObjectId id1 = ObjectId.Parse(id);
                var filter = Builders<Entity>.Filter.Eq(element => element.Id, id);
                var result = await Entities.DeleteOneAsync(filter);

                Console.WriteLine(result);
                if (result.DeletedCount >= 1)
                {
                    return Ok(result);
                }

                return NotFound();


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateEntity([FromBody] Entity entity)
        {
            try
            {
                var Entities = _database.GetCollection<Entity>("entity");

                var filter = Builders<Entity>.Filter.Eq(e => e.Id, entity.Id);

                var existingEntity = await Entities.Find(filter).FirstOrDefaultAsync();
                if (existingEntity == null)
                {
                    return NotFound("Entity not found");
                }

                var updateBuilder = Builders<Entity>.Update;

                if (entity.Names != null && entity.Names.Any())
                {
                    var namesUpdate = updateBuilder.Set(e => e.Names, entity.Names);
                    await Entities.UpdateOneAsync(filter, namesUpdate);
                }

                if (entity.Dates != null && entity.Dates.Any())
                {
                    var datesUpdate = updateBuilder.Set(e => e.Dates, entity.Dates);
                    await Entities.UpdateOneAsync(filter, datesUpdate);
                }

                if (entity.Addresses != null && entity.Addresses.Any())
                {
                    var addressesUpdate = updateBuilder.Set(e => e.Addresses, entity.Addresses);
                    await Entities.UpdateOneAsync(filter, addressesUpdate);
                }

                if (!string.IsNullOrEmpty(entity.Gender))
                {
                    var genderUpdate = updateBuilder.Set(e => e.Gender, entity.Gender);
                    await Entities.UpdateOneAsync(filter, genderUpdate);
                }

                return Ok($"Update Successful");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
