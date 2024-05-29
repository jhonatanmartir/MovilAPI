using AESMovilAPI.Models;
using ivraes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ServiceModel;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar el pool de DbContext
builder.Services.AddDbContextPool<SAPSGCDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Load ConnectedService.json
var connectedServiceConfig = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Connected Services/ivraes/ConnectedService.json", false, true)
    .Build();

// Extract the endpoint address
var endpointUrls = connectedServiceConfig.GetSection("ExtendedData:inputs").Get<string[]>();
// Use the first endpoint for simplicity, or apply your own logic to select the desired endpoint
var endpointUrl = endpointUrls?.FirstOrDefault() ?? "";

// Configurar autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "www.movilaesweb.com",
            ValidAudience = "www.movilaesweb.com",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6e7a10f083b54c551425112f0d0180da5c9bc2fe18daedd8dd1e338444ec29db"))
        };
    });
builder.Services.AddAuthorization();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "AESMovil API",
            Version = "v1",
            Description = "Documentación para realizar integraciones con los Canales Digitales de AES El Salvador. Contactar para obtener APIKey",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Jhonatan Mártir",
                Email = "creativa.jmartir.c@aes.com"
            }
        });

        // Opcional: agregar comentarios XML
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });

// Registrar el servicio con una vida útil específica (Singleton, Scoped, Transient)
builder.Services.AddSingleton<VRAESELSALVADORSoapClient>(provider =>
{
    var binding = new BasicHttpBinding();
    var endpoint = new EndpointAddress(endpointUrl);
    return new VRAESELSALVADORSoapClient(binding, endpoint);
});

// Make sure the configuration from appsettings.json is added.
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
// Register the HttpClientFactory
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
