using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

// Example, optimise for custom data retrieval
public class TokenDataDocumentService : Custom.Hybrid.Code14
{
  public string GetTokenDataDocumentName(int tokenDataEntityId)
  {
    var tokenDataDocument = AsList(App.Data["TokenData"]).Where(tokenData => tokenData.EntityId == tokenDataEntityId).FirstOrDefault();

    if (tokenDataDocument == null) {
      return "Unknown";
    }

    return AsDynamic(tokenDataDocument as object).Name;
  }

  public string GetTokenValue(string text, string token) {
    var regex = new Regex(@"(?<=\[" + token + @"]=).*?(?=\[.*|$)");
    return regex.Match(text.Replace("\n", "")).Value; 
  }

  public string[] GetTokens(string text) {
    var regex = new Regex(@"(?<=\[).*?(?=\]=|$)");
    return regex.Matches(text)
      .Cast<Match>()
      .Select(m => m.Value)
      .ToArray();
  }

  public string GetTokenizedContent(string tokenData, string content) {
    var tokens = GetTokens(tokenData);

    var tokenizedContent = content;

    // replace tokens in body with placeholder tokens
    foreach(string token in tokens) {
      var tokenValue = GetTokenValue(tokenData, token);
      tokenValue = Kit.Scrub.All(tokenValue);
      tokenizedContent = tokenizedContent.Replace("[" + token + "]", tokenValue);
    }

    return tokenizedContent; 
  }
}

