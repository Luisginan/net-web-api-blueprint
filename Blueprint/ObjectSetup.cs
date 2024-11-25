using System.Diagnostics.CodeAnalysis;
using System.Net;
using Blueprint.CustomerModule.Rpc;
using Blueprint.CustomerModule.Rpc.Services;
using Blueprint.CustomerModule.Services;
using Confluent.Kafka.Extensions.OpenTelemetry;
using Core.Base;
using Core.CExceptions;
using Core.Config;
using Core.Systems;
using Core.Utils.DB;
using Core.Utils.Messaging;
using Core.Utils.Security;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;

namespace Blueprint;
[ExcludeFromCodeCoverage]
public static class ObjectSetup
{
    public static void GetConfig(this WebApplicationBuilder builder)
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        logger.Information(File.Exists("configurations/appsettings.json")
            ? "Config : configurations/appsettings.json"
            : "Config : Using appsettings.json");
        
        builder.Configuration.AddJsonFile(
            File.Exists("configurations/appsettings.json") ? "configurations/appsettings.json" : "appsettings.json",
            optional: false, reloadOnChange: true);
        
        builder.Services.Configure<MessagingConfig>(builder.Configuration.GetSection("Messaging"));
        builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection("Database"));
        builder.Services.Configure<CacheConfig>(builder.Configuration.GetSection("Cache"));
        builder.Services.Configure<LogDbConfig>(builder.Configuration.GetSection("LogDB"));
        builder.Services.Configure<FileStorageConfig>(builder.Configuration.GetSection("FileStorage"));
        builder.Services.Configure<SecretManagerConfig>(builder.Configuration.GetSection("SecretManager"));
        builder.Services.Configure<TracerConfig>(builder.Configuration.GetSection("Tracer"));
        builder.Services.Configure<TracerConfig>(builder.Configuration.GetSection("Api"));
        builder.Services.Configure<SchedulerConfig>(builder.Configuration.GetSection("Scheduler"));
        
    }

    private static void SetupApp(this WebApplication app, string protocol, string pathHealthCheck = "/healthchecks")
    {
        if (protocol.Equals("grpc", StringComparison.CurrentCultureIgnoreCase))
        {
            app.UseSerilogRequestLogging();
            app.UseHttpMetrics();
            app.UseMetricServer();
        }
        else if (protocol.Equals("rest", StringComparison.CurrentCultureIgnoreCase))
        {
            app.UseHttpLogging();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseMiddleware<LoggingMiddleware>();
            app.UseSerilogRequestLogging();
            app.UseHealthChecks(pathHealthCheck);
            app.UseHttpMetrics();
            app.UseMetricServer();
            app.UseAuthorization();
            app.MapControllers();  
        }
        else
        {
            throw new ConfigNotImplementedException(protocol, "Protocol");
        }
        
    }
    public static void ShowInfoApp(this WebApplicationBuilder builder)
    {
        // get logger
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        
        var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        logger.Information($"App Version: {appVersion}");
        var  ip = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();
        logger.Information($"App IP: {ip}");
    }

    private static readonly string[] Tags = ["database"];

    public static void SetupApi(this WebApplication app, WebApplicationBuilder builder)
    {
        var apiConfig = builder.Configuration.GetSection("Api").Get<ApiConfig>();
        var protocol = "rest";
        if (apiConfig != null) protocol = apiConfig.Protocol;
        
        if (protocol.Equals("grpc", StringComparison.CurrentCultureIgnoreCase))
        {
            app.MapGrpcService<CustomerRpc>();
            app.MapGet("/",
                () =>
                    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        }
        
        app.SetupApp(protocol: protocol, pathHealthCheck:"/blueprint/healthchecks");
        
      
    }
    public static void SetupServiceSystem(this WebApplicationBuilder builder, string queryLocation = "./Queries/blueprint.json")
    {
        var apiConfig = builder.Configuration.GetSection("Api").Get<ApiConfig>() ?? new ApiConfig(){Protocol = "rest"};
        if (apiConfig.Protocol.Equals("grpc", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(lo => lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
            });
            builder.Services.AddGrpc();
        }
     
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
            logging.RequestHeaders.Add("sec-ch-ua");
            logging.ResponseHeaders.Add("MyResponseHeader");
            logging.MediaTypeOptions.AddText("application/javascript");
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
            logging.CombineLogs = true;
        });

        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database Check", tags: Tags);
        builder.UseOpenTelemetry();
        builder.Services.AddHttpLoggingInterceptor<LoggingInterceptor>();
        builder.Services.AddControllers(o =>
        {
            o.Filters.Add<ResponseFilterAttribute>();
            o.Filters.Add<ErrorFilterAttribute>();
        });
        

        builder.Services.AddHttpClient();
        builder.SetQueryLocation(queryLocation);
       
        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
        });

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true; 
        });


    }
    public static void SetupThirdPartyObjectByConfig(this WebApplicationBuilder builder)
    {
        var provider = GetProvider(builder);
        SetupObjectSecretManager(builder, provider);
        SetupObjectCache(builder, provider);
        SetupObjectDatabase(builder, provider);
        SetupObjectSurroundingApi(builder, provider);
        SetupObjectLogDb(builder, provider);
        SetupObjectMessaging(builder, provider);
        SetupObjectFileStorage(builder, provider);
    }

    private static ProviderConfig GetProvider(WebApplicationBuilder builder)
    {
        var databaseConfig = builder.Configuration.GetSection("Database").Get<DatabaseConfig>();
        var logDbConfig = builder.Configuration.GetSection("LogDb").Get<LogDbConfig>();
        var cacheConfig = builder.Configuration.GetSection("Cache").Get<CacheConfig>();
        var messagingConfig = builder.Configuration.GetSection("Messaging").Get<MessagingConfig>();
        var fileStorageConfig = builder.Configuration.GetSection("FileStorage").Get<FileStorageConfig>();
        var secretManagerConfig = builder.Configuration.GetSection("SecretManager").Get<SecretManagerConfig>();
        var surroundingApiConfig = builder.Configuration.GetSection("SurroundingApi").Get<SurroundingApiConfig>();
        return new ProviderConfig
        {
            Messaging = messagingConfig?.Provider ?? "",
            LogDb = logDbConfig?.Provider ?? "",
            Cache = cacheConfig?.Provider ?? "",
            Database = databaseConfig?.Provider ?? "",
            FileStorage = fileStorageConfig?.Provider ?? "",
            SecretManager = secretManagerConfig?.Provider ?? "" ,
            SurroundingApi = surroundingApiConfig?.Provider ?? ""
        };
    }

    private static void SetupObjectFileStorage(IHostApplicationBuilder builder, ProviderConfig provider)
    {
        if (provider.FileStorage == "") return;
        
        if (provider.FileStorage.Equals("google", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddScoped<IFileCloudStorage, FileCloudStorageMock>();
        }
        else if (provider.FileStorage.Equals("mock", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddScoped<IFileCloudStorage, FileCloudStorageMock>();
        }
        else
        {
            throw new ConfigNotImplementedException(provider.FileStorage, "FileStorage");
        }
    }
    private static void SetupObjectMessaging(IHostApplicationBuilder builder, ProviderConfig provider)
    {
        if (provider.Messaging == "") return;
        
        if (provider.Messaging.Equals("pubsub", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<IMessagingProducer, MessagingProducerPubsub>();
            builder.Services.AddSingleton<ISystemProducer, MessagingProducerPubsub>();
            builder.Services.AddTransient<IMessagingConsumer, MessagingConsumerPubsub>();
            builder.Services.AddTransient<IMessagingConsumerDlq, MessagingConsumerPubsub>();
            builder.Services.AddTransient<IConsumerTopicManager, ConsumerTopicManager>();
            builder.Services.AddTransient<IProducerTopicManager, ProducerTopicManager>();
            builder.Services.AddTransient<IProducerLog, ProducerLogPostgres>();
            builder.Services.AddTransient<IConsumerLog, ConsumerLogPostgres>();
        }
        else if (provider.Messaging.Equals("kafka", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<IMessagingProducer, MessagingProducer>();
            builder.Services.AddSingleton<ISystemProducer, MessagingProducer>();
            builder.Services.AddTransient<IMessagingConsumer, MessagingConsumerKafka>();
            builder.Services.AddTransient<IMessagingConsumerDlq, MessagingConsumerKafka>();
            builder.Services.AddTransient<IConsumerTopicManager, ConsumerTopicManager>();
            builder.Services.AddTransient<IProducerTopicManager, ProducerTopicManager>();
        }
        else if (provider.Messaging.Equals("mock", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<IMessagingProducer, MessagingProducerMock>();
            builder.Services.AddSingleton<ISystemProducer, MessagingProducerMock>();
            builder.Services.AddTransient<IMessagingConsumer, MessagingConsumerMock>();
            builder.Services.AddTransient<IConsumerTopicManager, ConsumerTopicManager>();
            builder.Services.AddTransient<IProducerTopicManager, ProducerTopicManager>();
            builder.Services.AddTransient<IMessagingConsumerDlq, MessagingConsumerMock>();
        }
        else
        {
            throw new ConfigNotImplementedException(provider.Messaging, nameof(provider.Messaging));
        }
    }
    private static void SetupObjectLogDb(IHostApplicationBuilder builder, ProviderConfig provider)
    {
        if (provider.LogDb == "") return;
        
        if (provider.LogDb.Equals("postgres", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<ILogDb, LogPostgres>();
            builder.Services.AddTransient<IConsumerLog, ConsumerLogPostgres>();
            builder.Services.AddTransient<IProducerLog, ProducerLogPostgres>();
        }
        else if (provider.LogDb.Equals("mongodb", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<IConsumerLog, ConsumerLog>();
            builder.Services.AddTransient<IProducerLog, ProducerLog>();
        }
        else if (provider.LogDb.Equals("mock", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<IConsumerLog, ConsumerLogMock>();
            builder.Services.AddTransient<IProducerLog, ProducerLogMock>();
            builder.Services.AddTransient<ILogDb, LogDbMock>();
        }
        else
        {
            throw new ConfigNotImplementedException(provider.LogDb, "LogDb");
        }
    }
    private static void SetupObjectSurroundingApi(IHostApplicationBuilder builder, ProviderConfig provider)
    {
        if (provider.SurroundingApi == "") return;
        
        if (provider.SurroundingApi.Equals("real", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<IProductExternal, ProductExternal>();
        }
        else if (provider.SurroundingApi.Equals("mock", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<IProductExternal, ProductExternalMock>();
        }
        else
        {
            throw new ConfigNotImplementedException(provider.SurroundingApi, "SurroundingApi");
        }
    }
    private static void SetupObjectDatabase(IHostApplicationBuilder builder, ProviderConfig provider)
    {
        if (provider.Database == "") return;
        
        if (provider.Database.Equals("postgres", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<INawaDaoRepository, NawaDaoRepository>();
            builder.Services.AddTransient<IQueryBuilderRepository, QueryBuilderRepository>();
            builder.Services.AddScoped<IConnection, Connection>();
           
        }
        else
        {
            throw new ConfigNotImplementedException(provider.Database, "Database");
        }
    }
    
    private static void SetupObjectCache(IHostApplicationBuilder builder, ProviderConfig provider)
    {
        if (provider.Cache.Equals("redis", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<ICache, RedisMemory>();
        }
        else if (provider.Cache.Equals("redis-distributed", StringComparison.CurrentCultureIgnoreCase))
        {
            var cacheConfig = builder.Configuration.GetSection("Cache").Get<CacheConfig>();
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration =
                    $"{cacheConfig?.Server}:{cacheConfig?.Port},defaultDatabase = {cacheConfig?.Database}";
                options.InstanceName = "blueprint";
            });
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddTransient<ICache, Cache>();
        }
        else if (provider.Cache.Equals("local", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
            builder.Services.AddTransient<ICache, LocalCache>();
        }
        else if (provider.Cache.Equals("none", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<ICache, NoneCache>();
        }
        else if (provider.Cache.Equals("mock", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<ICache, CacheMock>();
        }
        else
        {
            throw new ConfigNotImplementedException(provider.Cache, "Cache");
        }
    }
    private static void SetupObjectSecretManager(IHostApplicationBuilder builder, ProviderConfig provider)
    {
        if (provider.SecretManager.Equals("google", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<IVault, VaultManager>();
            builder.Services.AddTransient<ISecretManager, GoogleSecretManager>();
            builder.Services.AddSingleton<IMetrics, MetricsPrometheus>();
        }
        else if (provider.SecretManager.Equals("mock", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Services.AddTransient<ISecretManager, NoneSecretManager>();
            builder.Services.AddSingleton<IMetrics, MetricsMock>();
            builder.Services.AddTransient<IVault, VaultManager>();
        }
        else
        {
            throw new ConfigNotImplementedException(provider.SecretManager, "SecretManager");
        }
    }

    private static void UseOpenTelemetry(this WebApplicationBuilder builder)
    {
        var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        
        var otel = builder.Services.AddOpenTelemetry();
        var tracerConfig = builder.Configuration.GetSection("Tracer").Get<TracerConfig>();
        
        var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        otel.ConfigureResource(resource => resource
            .AddService(serviceName: builder.Environment.ApplicationName,
                serviceVersion: appVersion?.ToString()));
        
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation(options =>
            {
                options.EnrichWithHttpRequest = (activity, request) =>
                {
                    activity.AddTag("http.flavor", Common.GetHttpFlavour(request.Protocol));
                    activity.AddTag("http.scheme", request.Scheme);
                    activity.AddTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress);
                    activity.AddTag("http.request_content_length", request.ContentLength);
                    activity.AddTag("http.request_content_type", request.ContentType);
                };
        
                options.EnrichWithHttpResponse = (activity, response) =>
                {
                    activity.AddTag("http.response_content_length", response.ContentLength);
                    activity.AddTag("http.response_content_type", response.ContentType);
                };
        
                options.EnrichWithException = (activity, exception) =>
                {
                    activity.SetTag("Exception", exception.Message);
                };
                options.RecordException = true;
            });
            tracing.AddHttpClientInstrumentation();
            tracing.AddNpgsql();
            tracing.AddRedisInstrumentation();
            tracing.AddConfluentKafkaInstrumentation();
            
            tracing.AddSource("api","messaging.consumer","messaging.producer","file.storage","cache");
            
            if (tracerConfig == null)
               logger.Warning("TracerConfig is not found");
            else
            {
                if (tracerConfig.ExportToConsole)
                {
                    tracing.AddConsoleExporter();
                }
            
                if (tracerConfig.Exporter.Equals("jaeger", StringComparison.CurrentCultureIgnoreCase))
                {
                    tracing.AddJaegerExporter(jaeger =>
                    {
                        jaeger.AgentHost = tracerConfig.ExporterHost;
                        jaeger.AgentPort = Convert.ToInt32(tracerConfig.ExporterPort);
                    });
                }
            }
        });
    }
    
}

