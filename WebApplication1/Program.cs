using MongoDB.Driver;
using WebApplication1.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        const string connectionUri = "mongodb+srv://noureennaaz09:passwordh@cluster0.f8k7rwv.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";

    
        builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionUri));

    
        builder.Services.AddScoped(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase("WebApplication1");
        });
       
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole(); // Add console logger
        });
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        
        app.MapControllers();
        app.Run();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application started.");


        logger.LogInformation("Application is off");
        
    }
    
}

