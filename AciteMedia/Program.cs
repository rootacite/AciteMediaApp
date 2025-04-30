using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using AciteMedia.Components;
using MudBlazor.Services;

List<IPAddress> addresses = new();
foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
{
    // 筛选已启用且非回环的接口
    if (networkInterface.OperationalStatus == OperationalStatus.Up &&
        networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
    {
        // 获取接口的IP属性
        var ipProperties = networkInterface.GetIPProperties();
            
        // 获取所有IPv4单播地址
        foreach (var unicastAddress in ipProperties.UnicastAddresses)
        {
            if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
            {
                addresses.Add(unicastAddress.Address);
            }
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();
var addr = $"https://{addresses.First(x => x.ToString().Contains("192.168"))}";

builder.Services.AddScoped(sp => new HttpClient(
    new HttpClientHandler { ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true  })
{
    BaseAddress = new Uri(addr)
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();
app.Run();