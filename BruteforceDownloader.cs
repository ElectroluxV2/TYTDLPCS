using System.Text;

namespace TYTDLPCS;

static class BruteforceDownloader {

    private const string crlf = "\r\n";
    private static string boundary = Guid.NewGuid().ToString();
    static private string token = Environment.GetEnvironmentVariable("TLGRM_BOT_TOKEN") ?? throw new ArgumentNullException("Missing API Token!");

    // static private List<IDownloader> Downloaders = new List<IDownloader>{new YtDlP()};

    static public async Task ExtremeDownload() {

        var client = new HttpClient();

        var data = new PushStreamContent((stream, httpContent, transportContext) =>
        {
            // BetterContent.EncodeStringToStreamAsync(stream, "--" + boundary + crlf);
            BetterContent.EncodeStringToStreamAsync(stream, crlf + "--" + boundary + crlf);

            var headers = new StringBuilder();
            headers.Append("Content-Disposition: form-data; name=\"video\"; filename=\"video\" filename*=utf-8" + crlf);
            // headers.Append("Content-Type: video/mp4" + crlf);
            headers.Append(crlf); // Extra CRLF to end headers (even if there are no headers)
            BetterContent.EncodeStringToStreamAsync(stream, headers.ToString());

            stream.Write(File.ReadAllBytes("a.mp4"));

            BetterContent.EncodeStringToStreamAsync(stream, crlf + "--" + boundary + "--" + crlf);
            stream.Close();
        });

        // var data = new MultipartFormDataContent();
        // data.Add(new ByteArrayContent(File.ReadAllBytes("a.mp4")), "video", "video");

        // Console.WriteLine(new StreamReader(await data.ReadAsStreamAsync()).ReadToEnd());
        // Console.WriteLine((await data.ReadAsFormDataAsync()));

        HttpResponseMessage response = await client.PostAsync($"https://api.telegram.org/bot{token}/sendVideo?chat_id=-626820668", data); // Mamy takie samo data, trzeba sprawdzić czy PostAsync sprawdza coś po za streamem
    
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);


        // HttpResponseMessage response = await client.GetAsync($"https://api.telegram.org/bot{token}/getMe");
        // response.EnsureSuccessStatusCode();
        // string responseBody = await response.Content.ReadAsStringAsync();
        // Console.WriteLine(responseBody);

        // Downloaders.ForEach(downloader => downloader.Download());
    }
}