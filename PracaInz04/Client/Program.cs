using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PracaInz04.Client;
using PracaInz04.Client.Services;
using Blazored.LocalStorage;
using DnetIndexedDb;
using static PracaInz04.Client.IndexedDBModels.IndexedDBModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<StateService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddIndexedDbDatabase<IndexedDbContext>(options =>
{
    var indexedDbDatabaseModel = GetGridColumnDatabaseModelAttributeBased();
    options.UseDatabase(indexedDbDatabaseModel);
});

var host = builder.Build();
var xstateService = host.Services.GetRequiredService<StateService>();
await xstateService.GetFromLocalStorage();

//await builder.Build().RunAsync();
await host.RunAsync();
