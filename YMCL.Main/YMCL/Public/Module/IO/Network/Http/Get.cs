using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.IO.Network.Http;

public class Get
{
    public static async Task<string?> GetStringAsync(string url)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Dispatcher.UIThread.Invoke(() =>
            {
                Notice(MainLang.NetWorkError, NotificationType.Error);
            });
            return null!;
        }
    }
}