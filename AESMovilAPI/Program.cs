using ivraes;
using System.ServiceModel;

var builder = WebApplication.CreateBuilder(args);

// Load ConnectedService.json
var connectedServiceConfig = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Connected Services/ivraes/ConnectedService.json", false, true)
    .Build();

// Extract the endpoint address
var endpointUrls = connectedServiceConfig.GetSection("ExtendedData:inputs").Get<string[]>();
// Use the first endpoint for simplicity, or apply your own logic to select the desired endpoint
var endpointUrl = endpointUrls?.FirstOrDefault() ?? "";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar el servicio con una vida útil específica (Singleton, Scoped, Transient)
builder.Services.AddSingleton<VRAESELSALVADORSoapClient>(provider =>
{
    var binding = new BasicHttpBinding();
    var endpoint = new EndpointAddress(endpointUrl);
    return new VRAESELSALVADORSoapClient(binding, endpoint);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
