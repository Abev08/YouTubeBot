using System;

public class YoutubeMessagesResponse
{
  public string? kind { get; set; }
  public string? etag { get; set; }
  public int? pollingIntervalMillis { get; set; }
  public Pageinfo? pageInfo { get; set; }
  public string? nextPageToken { get; set; }
  public Item[]? items { get; set; }
}

public class Pageinfo
{
  public int? totalResults { get; set; }
  public int? resultsPerPage { get; set; }
}

public class Item
{
  public string? kind { get; set; }
  public string? etag { get; set; }
  public string? id { get; set; }
  public Snippet? snippet { get; set; }
  public Authordetails? authorDetails { get; set; }
}

public class Snippet
{
  public string? type { get; set; }
  public string? liveChatId { get; set; }
  public string? authorChannelId { get; set; }
  public DateTime? publishedAt { get; set; }
  public bool? hasDisplayContent { get; set; }
  public string? displayMessage { get; set; }
  public Textmessagedetails? textMessageDetails { get; set; }
}

public class Textmessagedetails
{
  public string? messageText { get; set; }
}

public class Authordetails
{
  public string? channelId { get; set; }
  public string? channelUrl { get; set; }
  public string? displayName { get; set; }
  public string? profileImageUrl { get; set; }
  public bool? isVerified { get; set; }
  public bool? isChatOwner { get; set; }
  public bool? isChatSponsor { get; set; }
  public bool? isChatModerator { get; set; }
}
