using ApiCubosAzure.Helpers;
using ApiCubosAzure.Data;
using ApiCubosAzure.Repositories;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using NSwag;
using NSwag.Generation.Processors.Security;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient
    (builder.Configuration.GetSection("KeyVault"));
});

SecretClient secretClient = builder.Services.BuildServiceProvider().GetService<SecretClient>();

KeyVaultSecret connectionStringSecret = await secretClient.GetSecretAsync("connectionstringsmc");
KeyVaultSecret issuerSecret = await secretClient.GetSecretAsync("issuersmc");
KeyVaultSecret audienceSecret = await secretClient.GetSecretAsync("audiencesmc");
KeyVaultSecret secretKeySecret = await secretClient.GetSecretAsync("secretkeysmc");
KeyVaultSecret blobSecret = await secretClient.GetSecretAsync("blobsmc");

string connectionString = connectionStringSecret.Value;
string issuer = issuerSecret.Value;
string audience = audienceSecret.Value;
string secretKey = secretKeySecret.Value;
string blob = blobSecret.Value;

HelperActionServicesOAuth helper = new HelperActionServicesOAuth(issuer, audience, secretKey);

builder.Services.AddSingleton<HelperActionServicesOAuth>(helper);

builder.Services.AddAuthentication
    (helper.GetAuthenticateSchema())
    .AddJwtBearer(helper.GetJwtBearerOptions());


builder.Services.AddTransient<RepositoryCubos>();
builder.Services.AddDbContext<CubosContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "Api OAuth Cubis";
    document.Description = "Api con seguridad 2024";
    document.AddSecurity("JWT", Enumerable.Empty<string>(),
        new NSwag.OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = "Authorization",
            In = OpenApiSecurityApiKeyLocation.Header,
            Description = "Copia y pega el Token en el campo 'Value:' as�: Bearer {Token JWT}."
        }
    );
    document.OperationProcessors.Add(
    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});

var app = builder.Build();
app.UseOpenApi();
//app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.InjectStylesheet("/css/monokai_theme.css");
    //options.InjectStylesheet("/css/material3x.css");
    options.SwaggerEndpoint(url: "/swagger/v1/swagger.json"
        , name: "Api OAuth Cubo");
    options.RoutePrefix = "";
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();