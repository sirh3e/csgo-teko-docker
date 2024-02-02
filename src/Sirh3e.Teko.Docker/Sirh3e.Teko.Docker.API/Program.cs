using Sirh3e.Teko.Docker.API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddTransient<RpcClient>(configuration => new RpcClient("A"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapGet("/api/fib/{number}", async (ulong number) =>
    {
        var client = new RpcClient();
        var message = await client.CallAsync(number.ToString());

        return ulong.Parse(message);
    })
    .WithName("GetFib")
    .WithOpenApi();

app.Run();