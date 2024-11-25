using System.Diagnostics.CodeAnalysis;
using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.MsgConsumer;
using Blueprint.CustomerModule.Repository;
using Blueprint.CustomerModule.Services;
using Blueprint.CustomerModule.Validators;
using Blueprint.HealthCheck.MasConsumer;
using Blueprint.RecoveryMessage;
using Blueprint.RecoveryMessage.Services;

namespace Blueprint;

[ExcludeFromCodeCoverage]
internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.GetConfig();
        builder.ShowInfoApp();

        builder.SetupServiceSystem(queryLocation:"./Queries/blueprint.json");
        builder.SetupThirdPartyObjectByConfig();

        builder.Services.AddAutoMapper(typeof(CustomerMapper));
        builder.Services.AddTransient<ICustomerService, CustomerService>();
        builder.Services.AddTransient<ICustomerRuler, CustomerRuler>();
        builder.Services.AddTransient<ICustomerRepository, CustomerRepository>();
        builder.Services.AddTransient<IRecoveryMessageService, RecoveryMessageService>();

        //builder.Services.AddHostedService<SyncCustomer2>();
        //builder.Services.AddHostedService<HealthCheckListener>();

        var app = builder.Build();
        app.SetupApi(builder);
       
        app.Run();
    }
}