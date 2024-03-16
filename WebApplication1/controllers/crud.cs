using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebApplication1.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    private readonly IMongoDatabase _database;
    private readonly int maxRetryAttempts = 3;
    private readonly TimeSpan initialBackoffDelay = TimeSpan.FromSeconds(1);
    private readonly TimeSpan maxBackoffDelay = TimeSpan.FromSeconds(10);
    private readonly double backoffMultiplier = 2;
    private readonly ILogger<ValuesController> _logger;
    public ValuesController(IMongoDatabase database, ILogger<ValuesController> logger)
    {
        _database = database;
        _logger = logger;
    }

    [HttpPost("create-entity")]
    public async Task<IActionResult> CreateEntity([FromBody] JsonElement json)
    {
        try
        {
            _logger.LogInformation("Attempting to create a new entity");

            var Entities = _database.GetCollection<Entity>("entity");
            if (json.ValueKind != JsonValueKind.Object)
            {
                _logger.LogWarning("Invalid JSON input: Expected an object");
                return BadRequest("Invalid JSON input: Expected an object");
            }

            string gender = json.GetProperty("gender").GetString().ToLower();

            JsonElement namesArray = json.GetProperty("names");

            if (namesArray.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("Invalid JSON input: 'names' property is not an array");
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
            
            var result = await RetryAsync(async () =>{
                var newEntity = new Entity
                {
                    Addresses = addressList,
                    Dates = dateList,
                    Deceased = false,
                    Gender = gender,
                    Names = NamesList
                };
                
                Entities.InsertOne(newEntity);

                _logger.LogInformation("New entity created successfully");
                return Ok(newEntity);
            });

            return result;
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new entity");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
   
    [HttpGet("get-all-entities")]
    public async Task<IActionResult> GetAllEntities([FromQuery] string? pageSize, [FromQuery] string? page)
    {
        try
        {
            var Entities = _database.GetCollection<Entity>("entity");
            var count = Entities.EstimatedDocumentCount();

            var pageSizeInt = 10;
            var pageInt = 1;
           
            if (!string.IsNullOrEmpty(pageSize))
            {
                pageSizeInt = int.Parse(pageSize);
                if (pageSizeInt > 20)
                {
                    return StatusCode(300, $"the maximum page size cannot be more than 20 ");
                }
                
            }
            if (!string.IsNullOrEmpty(page))
            {
                pageInt = int.Parse(page);
                if (pageInt < 1)
                {
                    pageInt = 1;
                    return StatusCode(404, $"the page {page} number does not exist");
                }
            }
            var TotalPageCount= (int)Math.Ceiling((double)count / pageSizeInt);
            var entities = await RetryAsync(async () =>
           {
               return await _database.GetCollection<Entity>("entity")
                                   .Find(_ => true)
                                   .Skip(pageSizeInt * (pageInt - 1))
                                   .Limit(pageSizeInt)
                                   .SortBy(i => i.Names)
                                   .ToListAsync();
           });

            // await Entities.Find(_ => true).Skip(pageSizeInt*(pageInt-1)).Limit(pageSizeInt).SortBy(i=>i.Names).ToListAsync();
            if (entities == null || entities.Count == 0)
            {
                return NotFound($"No elements exist in page {page} ");
            }
            
            var result =new {
                data = entities,
                TotalPages= TotalPageCount,
                PageNumber = pageInt
            };

            _logger.LogInformation("All entities information fetched");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing GetAllEntities");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("get-entity-by-id/{id}")]
    public async Task<IActionResult> GetEntityById(string id)
    {
        try
        {
            _logger.LogInformation($"Attempting to retrieve entity with ID: {id}");

            var Entities = _database.GetCollection<Entity>("entity");
            var entity = await RetryAsync(async ()=> {
                var val = await Entities.Find(e => e.Id == id).FirstOrDefaultAsync();
                return val;

            });

            if (entity == null)
            {
                _logger.LogWarning($"Entity with ID: {id} not found");
                return NotFound();
            }

            _logger.LogInformation($"Successfully retrieved entity with ID: {id}");
            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving entity with ID: {id}");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    [HttpGet("search")]
    public async Task<IActionResult> SearchEntities([FromQuery] string search, [FromQuery] string? pageSize, [FromQuery] string? page)
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
            var count = Entities.CountDocuments(filter);
            var pageSizeInt = 10;
            var pageInt = 1;
            if (!string.IsNullOrEmpty(pageSize))
            {
                pageSizeInt = int.Parse(pageSize);
                if (pageSizeInt > 20)
                {
                    return StatusCode(300, $"the maximum page size cannot be more than 20 ");
                }
            }
            if (!string.IsNullOrEmpty(page))
            {
                pageInt = int.Parse(page);
                if (pageInt < 1)
                {
                    pageInt = 1;
                    return StatusCode(404, $"the page {page} number does not exist");
                }
            }
            var TotalPageCount= (int)Math.Ceiling((double)count / pageSizeInt);
            List<Entity> results = await RetryAsync(async () =>
            {
                return await Entities.Find(filter)
                                      .Skip(pageSizeInt * (pageInt - 1))
                                      .Limit(pageSizeInt)
                                      .SortBy(i => i.Names)
                                      .ToListAsync();
            });

            if (results == null || results.Count == 0)
            {
                _logger.LogWarning($"No elements found for search query: {search}, page: {page}");
                return NotFound($"No elements exist for search query: {search}, page: {page}");
            }
            var response =new {
                data = results,
                TotalPages= TotalPageCount,
                PageNumber = pageInt
            };
            _logger.LogInformation($"Search query: {search}, page: {page}, returned {results.Count} results.");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing SearchEntities");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    [HttpGet("filter")]
    public async Task<IActionResult> SearchEntities([FromQuery] string? gender, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? countries, [FromQuery] string? pageSize, [FromQuery] string? page)
    {
        try
        {
            var filterBuilder = Builders<Entity>.Filter;
            var filter = filterBuilder.Empty;
            var Entities = _database.GetCollection<Entity>("entity");
            
            var count = Entities.CountDocuments(filter);
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

            var pageSizeInt = 10;
            var pageInt = 1;
            if (!string.IsNullOrEmpty(pageSize))
            {
                pageSizeInt = int.Parse(pageSize);
                if (pageSizeInt > 20)
                {
                    return StatusCode(300, $"the maximum page size cannot be more than 20 ");
                }
            }

            var TotalPageCount= (int)Math.Ceiling((double)count / pageSizeInt);

            List<Entity> results = await RetryAsync(async () =>
            {
                return await Entities.Find(filter)
                                    .Skip(pageSizeInt * (pageInt - 1))
                                    .Limit(pageSizeInt)
                                    .SortBy(i => i.Names)
                                    .ToListAsync();
            });
            
            var response =new {
                data = results,
                TotalPages= TotalPageCount,
                PageNumber = pageInt
            };

            _logger.LogInformation("Information fetched with filter(s)");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing filtered results");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteEntity(string id)
    {
        try
        {
            
            var Entities = _database.GetCollection<Entity>("entity");
            var filter = Builders<Entity>.Filter.Eq(element => element.Id, id);
            
           
            var result = await RetryAsync(async () =>
            {
                return await Entities.DeleteOneAsync(filter);
  
                
            });
           if (result.DeletedCount >= 1)
            {
                _logger.LogWarning($"Entity {id} deleted");
                return Ok(result);
            }
            
            _logger.LogWarning($"Attempted deletion on entity {id} failed");
            return NotFound();
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting entity {id}");
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
                _logger.LogWarning($"Attempted updation of non-existing entity {entity.Id}");
                return NotFound("Entity not found");
            }

            var updateBuilder = Builders<Entity>.Update;

            if (entity.Names != null && entity.Names.Any())
            {
                var namesUpdate = updateBuilder.Set(e => e.Names, entity.Names);
                var namesUpdatd = await RetryAsync(async()=>{
                    return await Entities.UpdateOneAsync(filter, namesUpdate);

                });
                
            }

            if (entity.Dates != null && entity.Dates.Any())
            {
                var datesUpdate = updateBuilder.Set(e => e.Dates, entity.Dates);
                var datesUpdated = await RetryAsync(async()=>{
                    return await Entities.UpdateOneAsync(filter, datesUpdate);

                });
                
            }

            if (entity.Addresses != null && entity.Addresses.Any())
            {
                var addressesUpdate = updateBuilder.Set(e => e.Addresses, entity.Addresses);
                var addressUpdated = await RetryAsync(async()=>{
                    return await Entities.UpdateOneAsync(filter, addressesUpdate);

                });
            }

            if (!string.IsNullOrEmpty(entity.Gender))
            {
                var genderUpdate = updateBuilder.Set(e => e.Gender, entity.Gender);
                await Entities.UpdateOneAsync(filter, genderUpdate);
            }

            _logger.LogInformation($"{entity.Id} entity updated");
            return Ok($"Update Successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while Updating entity");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    private async Task<T> RetryAsync<T>(Func<Task<T>> func)
    {
        int retryAttempt = 0;
        TimeSpan backoffDelay = initialBackoffDelay;

        while (retryAttempt < maxRetryAttempts)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Retry attempt {RetryAttempt}. Retrying in {BackoffDelay} seconds.", retryAttempt + 1, backoffDelay.TotalSeconds);
                await Task.Delay(backoffDelay);
                retryAttempt++;
                backoffDelay = TimeSpan.FromSeconds(Math.Min(backoffDelay.TotalSeconds * backoffMultiplier, maxBackoffDelay.TotalSeconds));
            }
        }

        _logger.LogError("Max retry attempts reached. Operation failed permanently.");
        throw new Exception("Max retry attempts reached. Operation failed permanently.");
    }
}

