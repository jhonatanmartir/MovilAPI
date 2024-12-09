using AESMovilAPI.Filters;
using AESMovilAPI.Models;
using ivraes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using Swashbuckle.AspNetCore.Filters;
using System.Globalization;
using System.ServiceModel;
using System.Text;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Iniciando aplicación...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    // Configura la zona horaria y cultura predeterminada para El Salvador
    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
    var cultureInfo = new CultureInfo("es-SV");

    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

    // Configurar NLog como el proveedor de logging
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // Configurar el pool de DbContext
    builder.Services.AddDbContextPool<SAPSGCDbContext>(options =>
        options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection"))
        );

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
                ValidIssuer = builder.Configuration.GetValue<string>("Security:Iss"),
                ValidAudience = builder.Configuration.GetValue<string>("Security:Aud"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6e7a10f083b54c551425112f0d0180da5c9bc2fe18daedd8dd1e338444ec29db"))
            };
        });
    builder.Services.AddAuthorization();

    // Add services to the container.
    // Registrar el filtro en el contenedor de servicios
    builder.Services.AddScoped<ActionExecutionFilter>();

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "AESMovil API",
            Version = "v1",
            Description = @"Documentación para realizar integraciones con los <strong>Canales Digitales</strong> de AES El Salvador.<br/><br/>
                            Contactar a TI AES El Salvador para obtener Key de autenticación.<br/>
                            Con el Key proporcionada utilice en endpoint de <code>Auth</code> para generar el token de autorización.<br/><br/>
                            Los endpoints que aparecen en este documento estan disponibles para pruebas y no seran modificados a excepción en caso de hacer correciones o mejoras.<br/>
                            <h4>Consideraciones</h4>
                            <ul>
                                <li>A medida que se desarrolla el servicio se iran habilitando mas endpoints.</li>
                                <li>En ambiente desarrollo los token de autorización tienen validez por 48 horas.</li>
                                <li>El texto de este documento puede ir cambiando para dar mas claridad en las especificaciones del API.</li>
                                <li>No se recomienda utilizar los endpoints legacy, revisar la descripción.</li>
                            </ul>",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Jhonatan Mártir",
                Email = "creativa.jmartir.c@aes.com?subject=Me%20gustaría%20solicitar%20una%20API%20key%20para%AESMovil%20API"
            }
        });
        // Remover status code 200 by default
        c.OperationFilter<RemoveDefaultResponsesOperationFilter>();
        // Opcional: agregar comentarios XML
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);

        // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Digite 'Bearer' espacio y el token.",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        c.ExampleFilters(); // Habilitar filtros de ejemplos
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
    //builder.Services.AddSingleton(new HttpClient(new HttpClientHandler
    //{
    //    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    //}));
    // Registrar los filtros de ejemplos
    builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

    // Configurar CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
    });

    // Register memory cache service
    builder.Services.AddMemoryCache();

    var app = builder.Build();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI(
            c =>
            {
                //c.SwaggerEndpoint("/swagger/v1/swagger.json", "AESMovil API V1");

                // Custom UI settings
                c.InjectStylesheet("/swagger-ui/custom.css");
                c.InjectJavascript("/swagger-ui/custom.js");
                c.DocumentTitle = "AESMovil API Doc";
                //c.RoutePrefix = string.Empty; // Serve Swagger UI at application root
            });
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseCors("AllowAll");    // Aplicar la política de CORS

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Se detuvo la aplicación. " + ex.Message);
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}