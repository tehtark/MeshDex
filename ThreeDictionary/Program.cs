using ThreeDictionary.Application;
using ThreeDictionary.Components;
using ThreeDictionary.Extensions;
using ThreeDictionary.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddPresentation();

var app = builder.Build();

await app.Services.InitialiseDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", true);
    // https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSerilogMiddleware();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();


await app.RunAsync();