using System;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

public static class JwtTokenUtility
{
    public static JwtTokenClaims ParseJwtToken(string token)
    {
        string[] tokenParts = token.Split('.');
        string claimsJson = Base64UrlDecode(tokenParts[1]);

        JwtTokenClaims tokenClaims = JsonConvert.DeserializeObject<JwtTokenClaims>(claimsJson);

        return tokenClaims;
    }

    private static string Base64UrlDecode(string input)
    {
        string padded = input + new string('=', (4 - input.Length % 4) % 4);
        string base64 = padded.Replace('-', '+').Replace('_', '/');
        byte[] base64Bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(base64Bytes);
    }
}

public class JwtTokenClaims
{
    public string Sub { get; set; }
    public string Type { get; set; }
    public long Iat { get; set; }
    public long Exp { get; set; }
}
