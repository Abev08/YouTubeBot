using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

internal class Program
{
  // Config variables
  // static string ChannelID = "UCL7bBOIdfxxM9WyxTnTk7Qg";// Cakez, Copied from https://www.youtube.com/account_advanced
  static string ChannelID = "UCSJ4gkVC6NrvII8umztf0Ow";// LOFI // Copied from https://www.youtube.com/account_advanced
  static string APIkey = "AIzaSyArBG6NAoS_OXAwKi9gBC5KOfYA8-BoVwc"; // API key generated at https://console.developers.google.com/ (STEP 3)

  static HttpClient Client = new HttpClient(); // HTTP client to communicate with youtube API

  private static void Main(string[] args)
  {
    // STEP 1: Generate new project at https://console.cloud.google.com/projectselector2 - youtube bot project
    // STEP 2: Enable "YouTube Data API v3" in your new project at https://console.developers.google.com/apis/api/youtube.googleapis.com
    // STEP 3: Generate API key at https://console.developers.google.com/ and assign APIkey variable to it
    // STEP 4: Change APP state from testing to in production via "publish app" at https://console.cloud.google.com/projectselector2

    // Some local variables
    string response; // Used to store response messages
    string uri; // Used for url generation
    int startIndex, endIndex; // Index variables for substring

    // Next steps are automated
    // STEP 5: Get list of active streams on specified ChannelID
    uri = $"https://www.googleapis.com/youtube/v3/search?part=snippet&channelId={ChannelID}&eventType=live&type=video&key={APIkey}";
    using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), uri))
    {
      request.Headers.Add("Accept", "application/json");

      response = Client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
    }

    // STEP 6: From received list of active streams get videoID of first one
    string videoID;
    startIndex = response.IndexOf("videoId");
    if (startIndex >= 0)
    {
      startIndex += "videoId\": \"".Length;
      endIndex = response.IndexOf("\"", startIndex);
      videoID = response.Substring(startIndex, endIndex - startIndex);
    }
    else
    {
      Console.WriteLine(response);
      throw new Exception("Live broadcast not found");
    }

    // STEP 7: Using received videoID get stream info
    uri = $"https://youtube.googleapis.com/youtube/v3/videos?part=liveStreamingDetails&id={videoID}&key={APIkey}";
    using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), uri))
    {
      request.Headers.Add("Accept", "application/json");

      response = Client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
    }

    // STEP 8: From received stream info get activeLiveChatId
    string chatID;
    startIndex = response.IndexOf("activeLiveChatId");
    if (startIndex >= 0)
    {
      startIndex += "activeLiveChatId\": \"".Length;
      endIndex = response.IndexOf("\"", startIndex);
      chatID = response.Substring(startIndex, endIndex - startIndex);
    }
    else
    {
      Console.WriteLine(response);
      throw new Exception("Information about live broadcast not found");
    }

    // STEP 9: Get chat messages. Using new thread because youtube api requires waiting specified amount of time between asking for messages
    Thread messagesThread = new Thread(() =>
    {
      YoutubeMessagesResponse? youtubeResponse;
      uri = $"https://youtube.googleapis.com/youtube/v3/liveChat/messages?liveChatId={chatID}&part=snippet%2CauthorDetails&key={APIkey}";

      // Main loop used to receive new messages
      while (true)
      {
        using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), uri))
        {
          request.Headers.Add("Accept", "application/json");

          response = Client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
        }

        try
        {
          youtubeResponse = JsonSerializer.Deserialize<YoutubeMessagesResponse>(response);
          string chatter = string.Empty;
          if (youtubeResponse != null)
          {
            // Console.WriteLine($">>>>> RECEIVED {youtubeResponse.items.Length} MESSAGES");

            // Do something with received messages
            for (int i = 0; i < youtubeResponse.items?.Length; i++)
            {
              if (youtubeResponse.items[i].authorDetails?.isChatModerator == true) chatter = "MOD";
              else if (youtubeResponse.items[i].authorDetails?.isChatOwner == true) chatter = "OWNER";
              else if (youtubeResponse.items[i].authorDetails?.isChatSponsor == true) chatter = "SPONS";
              else if (youtubeResponse.items[i].authorDetails?.isVerified == true) chatter = "VER";
              else chatter = string.Empty;

              Console.WriteLine(String.Format("{0, -4}{1, 20}{2, 2}{3, -0}",
                                                      chatter,
                                                      youtubeResponse.items[i].authorDetails?.displayName,
                                                      ": ",
                                                      youtubeResponse.items[i].snippet?.displayMessage));

              // Based on received message (youtubeResponse.items[i].snippet?.displayMessage) the bot maybe could replay something?
              // Also "!tts xxxxxx" could be checked here
            }

            // Wait requsted amount before pulling messages again
            if (youtubeResponse.pollingIntervalMillis != null) Thread.Sleep(youtubeResponse.pollingIntervalMillis.Value);
            else Thread.Sleep(1000);

            // Update uri for next call
            uri = $"https://youtube.googleapis.com/youtube/v3/liveChat/messages?liveChatId={chatID}&part=snippet%2CauthorDetails&pageToken={youtubeResponse.nextPageToken}&key={APIkey}";
          }
          else
          {
            // Something went wrong parsing response message to object
            Console.WriteLine("Error parsing youtube response");
            Thread.Sleep(1000);
          }
        }
        catch (Exception ex)
        {
          // Something went badly
          Console.WriteLine(ex.Message);
          Thread.Sleep(1000);
        }
      }
    });
    messagesThread.Start();

    // While messages thread is running don't close app
    while (messagesThread.IsAlive) Thread.Sleep(10);

    // Dispose of used objects
    Client.Dispose();
  }
}
