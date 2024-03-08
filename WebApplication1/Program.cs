using MongoDB.Driver;
using WebApplication1.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        const string connectionUri = "mongodb+srv://noureennaaz09:passwordh@cluster0.f8k7rwv.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";

        // Register MongoDB client
        builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionUri));

        // Register MongoDB database
        builder.Services.AddScoped(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase("WebApplication1");
        });
        // mongodb+srv://noureennaaz09:<password>@cluster0.f8k7rwv.mongodb.net/

        // user :::  noureennaaz09
        //  password :::  IYv1cWZpULDkvNbA
        // var mongoURL = new MongoUrl(connectionUri);
        // var client = new MongoClient(mongoURL);

        // var databases = client.GetDatabase("WebApplication1");
        // var NameDB = databases.GetCollection<Name>("name");

        // var newName = new Name{
        //     FirstName = "Linus" ,
        //     MiddleName= "Torvalds" ,
        //     Surname = "Linux" 

        // };

        // NameDB.InsertOne(newName);
        // Console.WriteLine("The list of databases on this server is: ");
        // foreach (var db in dbList)
        // {
        //     Console.WriteLine(db);
        // }
        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // var summaries = new[]
        // {
        //     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        // };

        // app.MapGet("/weatherforecast", () =>
        // {
        //     var forecast =  Enumerable.Range(1, 5).Select(index =>
        //         new WeatherForecast
        //         (
        //             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //             Random.Shared.Next(-20, 55),
        //             summaries[Random.Shared.Next(summaries.Length)]
        //         ))
        //         .ToArray();
        //     return forecast;
        // })
        // .WithName("GetWeatherForecast")
        // .WithOpenApi();
        // app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
