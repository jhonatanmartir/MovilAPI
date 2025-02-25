using AESMovilAPI.Filters;
using AESMovilAPI.Models;
using AESMovilAPI.Services;
using AESMovilAPI.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using Polly;
using Polly.Extensions.Http;
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
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
    builder.Host.UseNLog();

    // Inicializar ApiEndpoints con la configuración
    ApiEndpoint.Initialize(builder.Configuration);

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

    // Load ConnectedService.json IVRADMS
    var connectedServiceConfigIVRADMS = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("Connected Services/ivradms/ConnectedService.json", false, true)
        .Build();
    var endpointUrlsIVRADMS = connectedServiceConfigIVRADMS.GetSection("ExtendedData:inputs").Get<string[]>();
    var endpointUrlIVRADMS = endpointUrlsIVRADMS?.FirstOrDefault() ?? "";

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
    // Agregar LoggerService como servicio genérico
    builder.Services.AddScoped(typeof(LoggerService<>));

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "AESMovil API",
            Version = "v1",
            Description = @$"Documentación para realizar integraciones con los <strong>Canales Digitales</strong> de AES El Salvador.<br/><br/>
                            Contactar a TI AES El Salvador para obtener Key de autenticación.<br/>
                            Con el Key proporcionada utilice en endpoint de <code>Auth</code> para generar el token de autorización.<br/><br/>
                            Solamente puede utilizar este API si ha recibido un KEY aprobado por TI AES El Salvador<br/>
                            <h4>Consideraciones</h4>
                            <ul>
                                <li>Cada token de autorización tiene validez por {builder.Configuration.GetValue<int>("Security:Exp") * 24} horas.</li>
                                <li>No se recomienda utilizar los endpoints legacy, revisar la descripción.</li>
                            </ul>",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Jhonatan Mártir",
                Email = "creativa.jmartir.c@aes.com?subject=Solicitando%20una%20API%20KEY%20para%20AESMovilAPI"
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
    builder.Services.AddSingleton<ivraes.VRAESELSALVADORSoapClient>(provider =>
    {
        var binding = new BasicHttpBinding();
        var endpoint = new EndpointAddress(endpointUrl);
        return new ivraes.VRAESELSALVADORSoapClient(binding, endpoint);
    });

    // Registrar el servicio con una vida útil específica (Singleton, Scoped, Transient) IVRADMS
    builder.Services.AddSingleton<ivradms.VRAESELSALVADORSoapClient>(provider =>
    {
        var binding = new BasicHttpBinding();
        var endpoint = new EndpointAddress(endpointUrlIVRADMS);
        return new ivradms.VRAESELSALVADORSoapClient(binding, endpoint);
    });

    // Make sure the configuration from appsettings.json is added.
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    // Register the HttpClientFactory
    // builder.Services.AddHttpClient();
    builder.Services.AddHttpClient(Constants.HTTP_CLIENT_NAME)
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10))) // Timeout
        .AddPolicyHandler(HttpPolicyExtensions
                      .HandleTransientHttpError()
                      .RetryAsync(3)) // Retry en errores transitorios
        .AddPolicyHandler(HttpPolicyExtensions
                      .HandleTransientHttpError()
                      .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))); // Circuit Breaker

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

    // Registrar ImageCache como servicio
    builder.Services.AddSingleton<FilesCache>();
    builder.Services.AddTransient<PDFBuilder>();

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxConcurrentConnections = 5000;
        options.Limits.MaxConcurrentUpgradedConnections = 5000;
        options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
    });

    // Configuración del Pool de Hilos
    ThreadPool.SetMinThreads(workerThreads: 32, completionPortThreads: 32);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI(
            c =>
            {
                //c.SwaggerEndpoint("/swagger/v1/swagger.json", "AESMovil API V1");
                var pathBase = app.Services.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request.PathBase.ToString() ?? string.Empty;

                // Custom UI settings
                //c.InjectStylesheet(Path.Combine(AppContext.BaseDirectory, "/swagger-ui/custom.css"));
                c.InjectStylesheet($"{pathBase}/swagger-ui/custom.css");
                c.InjectJavascript("/swagger-ui/custom.js");
                c.DocumentTitle = "AESMovil API Doc";
                //c.RoutePrefix = string.Empty; // Serve Swagger UI at application root
                app.UsePathBase(pathBase);
            });
    }

    // Precargar las imágenes después de construir el contenedor
    using (var scope = app.Services.CreateScope())
    {
        var imageCache = scope.ServiceProvider.GetRequiredService<FilesCache>();
        imageCache.PreloadResources();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseCors("AllowAll");    // Aplicar la política de CORS

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();

        // Endpoint para ver logs de un día específico (por defecto el de hoy)
        endpoints.MapGet("/logs/{date?}", async (context) =>
        {
            // Obtener el parámetro de fecha (si se envía)
            var dateParam = context.Request.RouteValues["date"]?.ToString();
            DateTime date;

            // Si no se envía una fecha, usar la fecha actual
            if (string.IsNullOrEmpty(dateParam) || !DateTime.TryParseExact(dateParam, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                date = DateTime.UtcNow;
            }

            // Obtener la ruta absoluta de logs
            var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
            var logFilePath = Path.Combine(logDirectory, $"{date:yyyy-MM-dd}.log");

            if (!File.Exists(logFilePath))
            {
                await context.Response.WriteAsync($"No hay logs para {date:yyyy-MM-dd}.");
                return;
            }

            try
            {
                // Leer el archivo sin bloquearlo
                using (var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(fileStream))
                {
                    var logs = await reader.ReadToEndAsync();
                    await context.Response.WriteAsync(logs);
                }
            }
            catch (Exception ex)
            {
                await context.Response.WriteAsync($"Error al leer el log: {ex.Message}");
            }
        })
        .RequireAuthorization(); // Protege el endpoint con JWT;
    });

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