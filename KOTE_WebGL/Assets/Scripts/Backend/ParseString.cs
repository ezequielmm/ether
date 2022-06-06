using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public static class ParseString
{
    public const string MatchEmailPattern =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
        + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
        + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
        + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

    public const string MatchPasswordPattern = @"^(?=.*[0-9])(?=.*[A-Z])(?=.*[a-z])(?=.*[!@#$%^&*()_+=\[{\]};:<>|./?,-]).{8,}$";

    public static bool IsEmail(string email)
    {
        return email != null && Regex.IsMatch(email, MatchEmailPattern);
    }

    public static bool IsPassword(string password)
    {
        return password != null && Regex.IsMatch(password, MatchPasswordPattern);
    }
}