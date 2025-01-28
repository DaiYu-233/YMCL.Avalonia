using System.Net.Http;

namespace YMCL.Public.Module.Init.SubModule;

public class TranslateToken
{
    public static async void RefreshToken()
    {
        await Task.Delay(200);
        while (true)
        {
            try
            {
                var handler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("User-Agent", "Apifox/1.0.0 (https://apifox.com)");
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("Host", "edge.microsoft.com");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                HttpResponseMessage response =
                    await client.GetAsync("https://edge.microsoft.com/translate/auth");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Const.Data.TranslateToken = responseBody;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}