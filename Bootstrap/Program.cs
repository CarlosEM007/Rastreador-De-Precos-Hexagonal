using Adapters.Messaging;
using Adapters.Notification;
using Adapters.Persistence;
using Adapters.Rest;
using Application;

var builder = WebApplication.CreateBuilder(args);

// Criar ExceptionHandler
ErrorHandlingDependency.AddExceptionHandling(builder.Services);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Injeção de Dependência
AdaptersRestDependency.AddAdaptersRest(builder.Services);
ApplicationModuleDependency.AddApplicationModule(builder.Services);
AdaptersPersistenceDependency.AddAdaptersPersistence(builder.Services, builder.Configuration);
AdaptersMessagingDependency.AddAdaptersMessaging(builder.Services, builder.Configuration);

//AdapterGatewayDependency.AddAdapterGateway(builder.Services);
AdaptersNotificationDependency.AddAdaptersNotification(builder.Services);

var app = builder.Build();

app.UseExceptionHandler();

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
