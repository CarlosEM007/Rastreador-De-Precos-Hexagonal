using Adapters.Agendador;
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

// É por aqui que o compose (e o Kong) descobrem que a API já está de pé.
builder.Services.AddHealthChecks();

// Injeção de dependência
AdaptersRestDependency.AddAdaptersRest(builder.Services);
ApplicationModuleDependency.AddApplicationModule(builder.Services);
AdaptersPersistenceDependency.AddAdaptersPersistence(builder.Services, builder.Configuration);
AdaptersMessagingDependency.AddAdaptersMessaging(builder.Services, builder.Configuration);
AgendadorModuleDependency.AddAgendador(builder.Services, builder.Configuration);

// Classes legadas, substituidas pelo messaging
//AdapterGatewayDependency.AddAdapterGateway(builder.Services);
//AdaptersNotificationDependency.AddAdaptersNotification(builder.Services);

var app = builder.Build();

// Antes de atender qualquer requisição: garante que o schema existe.
await app.Services.AplicarMigrations();

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

app.MapHealthChecks("/health");

app.Run();
