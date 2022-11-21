using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

internal class Program
{
  // Config variables
  static string ChannelID = "UCSJ4gkVC6NrvII8umztf0Ow"; // Copied from https://www.youtube.com/account_advanced
  static string APIkey = "AIzaSyArBG6NAoS_OXAwKi9gBC5KOfYA8-BoVwc"; // API key generated at https://console.developers.google.com/ (STEP 3)
  static string ClientID = "665677050582-qqbv9g5tfetk5riimbh4a85jl14fv9tv.apps.googleusercontent.com"; // From OAuth 2.0 (STEP 4)
  static string ClientSecret = "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"; // From OAuth 2.0 (STEP 4)

  // Other global variables
  static HttpClient Client = new HttpClient();
  static HttpListener LocalServer = new HttpListener();
  static string AccessToken; // Auto generated

  private static void Main(string[] args)
  {
    // STEP 1: Generate new project at https://console.cloud.google.com/projectselector2 - youtube bot project
    // STEP 2: Enable "YouTube Data API v3" in your new project at https://console.developers.google.com/apis/api/youtube.googleapis.com
    // STEP 3: Generate API key at https://console.developers.google.com/ and assign APIkey variable to it
    // STEP 4: Generate OAuth 2.0 identificator at https://console.developers.google.com/ and assign ID and Secret to variables
    // STEP 5: Change APP state from testing to in production via "publish app" at https://console.cloud.google.com/projectselector2

    // Steps 6+ are automated
    // STEP 6: Starting local server
    LocalServer.Prefixes.Add("http://localhost:23456/");
    LocalServer.Start();

    string response; // Used to store response messages
    string uri; // Used for url generation
    int startIndex, endIndex; // Index variables for substring

    if (false)
    {
      // STEP 7: Requesting OAuth 2.0 token
      // Uri for OAuth 2.0 token generation
      uri = "https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={ClientID}" +
            "&redirect_uri=http://localhost:23456" +
            "&scope=https://www.googleapis.com/auth/youtube.readonly" +
            "&state=try_sample_request" +
            "&include_granted_scopes=true" +
            "&response_type=code";

      // Open the link for the user to complete authorization
      Process.Start(new ProcessStartInfo() { FileName = uri, UseShellExecute = true });

      HttpListenerContext context = LocalServer.GetContext(); // Await response

      // For now lets just redirect to youtube to hide received code in browser url
      using (HttpListenerResponse resp = context.Response)
      {
        resp.Headers.Set("Content-Type", "text/plain");
        resp.Redirect(@"https://www.youtube.com/" + ChannelID);
      }

      response = context.Request.Url != null ? context.Request.Url.Query : string.Empty;
    }
    else
    {
      // TODO: Delete after testing
      response = "?state=try_sample_request&code=4/0AfgeXvumC76o9oikYDBLi8MoyLK5kCVMsvaQC5q9IgHF2nrSfeRvdeDo2EIqehj-tIZ3Vw&scope=https://www.googleapis.com/auth/youtube.readonly";
    }

    // Parse received request url - get code from it
    startIndex = response.IndexOf("code=");
    if (startIndex >= 0)
    {
      // Next step - request user token with received authorization code
      startIndex += "code=".Length;
      endIndex = response.IndexOf('&', startIndex);
      if (endIndex < 0) endIndex = response.Length;
      string code = response.Substring(startIndex, endIndex - startIndex);
      using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("POST"), "https://oauth2.googleapis.com/token"))
      {
        if (false)
        {
          request.Content = new StringContent($"client_id={ClientID}" +
                                            $"&client_secret={ClientSecret}" +
                                            $"&code={code}" +
                                            "&grant_type=authorization_code" +
                                            "&redirect_uri=http://localhost:23456");
          request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

          response = Client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
        }
        else
        {
          // TODO: Delete after testing
          response = "{\n  \"access_token\": \"ya29.a0AeTM1ieSPBsUGiR2s9umZMYFqGXTC78A1APXh8qZkGoCuRMs2jR9xAT-_DUiTa9U-Uau5dvcpMlG0DKvooMeJ9VcI-_gC31zyGz5xv1OZEkkUWYObOzTGZQkKPyGBN9xTy7IzZ-TP7YA7S3Pw1V_BJ-bJzoCaCgYKAcsSARMSFQHWtWOmnw9HIkCdokPj1LgDQJnUUg0163\",\n  \"expires_in\": 3599,\n  \"refresh_token\": \"1//0c9kfSnN8_CsOCgYIARAAGAwSNwF-L9IrSabIX4jBCfoDOCqJ4DnMnNKTqLwmcadwtp3OdxUW--FV0wOah8kHYdDmSvdvc0J1AFo\",\n  \"scope\": \"https://www.googleapis.com/auth/youtube.readonly\",\n  \"token_type\": \"Bearer\"\n}";
        }

        // Check if repsonse contains "access_token" keyword
        startIndex = response.IndexOf("access_token");
        if (startIndex >= 0)
        {
          // Received access token
          startIndex += "access_token\": \"".Length;
          endIndex = response.IndexOf("\"", startIndex);
          AccessToken = response.Substring(startIndex, endIndex - startIndex);
        }
        else
        {
          // Something went wrong
          throw new NotImplementedException("Implement something here :)");
        }
      }
    }
    else
    {
      // Something went wrong
      throw new NotImplementedException("Implement something here :)");
    }

    // STEP 8: Get liveChatId
    // uri = "https://youtube.googleapis.com/youtube/v3/liveBroadcasts?" +
    //       "id=" + ChannelID +
    //       "&key=" + APIkey;
    uri = $"https://www.googleapis.com/youtube/v3/search?part=snippet&channelId={ChannelID}&eventType=live&type=video&key={APIkey}";
    using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), uri))
    {
      request.Headers.Add("Authorization", $"Bearer {AccessToken}");
      request.Headers.Add("Accept", "application/json");

      response = Client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;

      Console.WriteLine(response);
    }
    // https://www.youtube.com/watch?v=3KfclTlg51c
    // STEP x: Get chat messages

    Client.Dispose();
    LocalServer.Close();
  }
}
