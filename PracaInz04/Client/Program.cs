using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PracaInz04.Client;
using PracaInz04.Client.Services;
using Blazored.LocalStorage;
using DnetIndexedDb;
using static PracaInz04.Client.IndexedDbClasses.IndexedDBModels;
using PracaInz04.Client.IndexedDbClasses;
using PracaInz04.Client.ImageProcessingClasses;
using PracaInz04.Client.LocalStorageClasses;
using Blazored.Modal;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<StateService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddIndexedDbDatabase<IndexedDbContext>(options =>
{
    var indexedDbDatabaseModel = GetIndexedDbDatabaseModelAttributeBased();
    options.UseDatabase(indexedDbDatabaseModel);
});
builder.Services.AddScoped<IndexedDbManager>();
builder.Services.AddScoped<ImageProcessing>();
builder.Services.AddScoped<LocalStorageManager>();
builder.Services.AddBlazoredModal();

// load data from local storage - old
//var host = builder.Build();
//var xstateService = host.Services.GetRequiredService<StateService>();
//await xstateService.GetFromLocalStorage();

// load data from local storage - new
var host = builder.Build();
var LSManager = host.Services.GetRequiredService<LocalStorageManager>();
await LSManager.GetFromLocalStorage();

//await builder.Build().RunAsync();
await host.RunAsync();
