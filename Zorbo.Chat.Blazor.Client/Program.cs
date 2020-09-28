using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Zorbo.Chat.Blazor.Client.Classes;

namespace Zorbo.Chat.Blazor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("app");

            builder.Services
                .AddHttpClient(
                    "Zorbo.Chat.Blazor.ServerAPI",
                    client => { 
                        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress); 
                    })
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services
                   .AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Zorbo.Chat.Blazor.ServerAPI"))
                   .AddTransient(sp => ChatClient.Self ?? new ChatClient());

            builder.Services.AddApiAuthorization();

            await builder.Build().RunAsync();
        }
    }
}
