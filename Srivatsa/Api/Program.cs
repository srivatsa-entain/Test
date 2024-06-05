
using Grpc.Net.Client;
using Newtonsoft.Json;
using System.Text.Json;
using Racing;
using System.Numerics;

namespace Api;
public class Filter
{
    public bool visible { get; set; }
    public long id { get; set; }
    public long pageSize { get; set; }
}

public class Root
{
    public Filter filter { get; set; } = new Filter();
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        GrpcChannel channel = GrpcChannel.ForAddress("http://localhost:5058");
        var client = new Racing.Racing.RacingClient(channel);

        //Get all the races. Can also pass in the filter to list only the visible races...
        app.MapPost("/list-races", async (HttpContext httpContext) =>
        {
            var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
            var request = new Racing.ListRacesRequest();
            try
            {
                var c = JsonConvert.DeserializeObject<Root>(requestBody);
                if (c != null)
                {
                    request.Filter = new Racing.ListRacesRequestFilter() 
                    { 
                        Visible = c?.filter?.visible ?? true,
                        PageSize = c?.filter?.pageSize > 0 ? c.filter.pageSize : 20
                    };
                }
                var allRaces = client.ListRaces(request);
                return allRaces;
            }
            catch (Exception e)
            {
                app.Logger.LogWarning(e.Message);
                return null;
            }
        })
        .WithName("ListRaces")
        .WithOpenApi();

        //Get Race by its ID
        app.MapPost("/list-raceid", async (HttpContext httpContext) =>
        {
            var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
            var request = new Racing.GetRacebyIdRequest();
            try
            {
                var c = JsonConvert.DeserializeObject<Root>(requestBody);
                if (c != null)
                {
                    request.Filter = new Racing.GetRaceByRequestFilter { Id = c.filter.id };
                }
                var allRaces = client.GetRaceById(request);
                return allRaces;
            }
            catch (Exception e)
            {
                app.Logger.LogWarning(e.Message);
                return null;
            }
        })
           .WithName("GetRaceById")
           .WithOpenApi();

        app.Run();
    }
}
