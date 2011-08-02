﻿/*
Copyright 2011 Google Inc

Licensed under the Apache License, Version 2.0(the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;

/// <summary>
/// Extension method container class.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Trims the string to the specified length, and replaces the end with "..." if trimmed.
    /// </summary>
    public static string TrimLength(this string str, int maxLength)
    {
        if (maxLength < 3)
        {
            throw new ArgumentException("Please specify a maximum length of at least 3", "maxLength");
        }

        if (str.Length <= maxLength)
        {
            return str; // Nothing to do.
        }

        return str.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Trims the specified list of words from the beginning of the stream. Case is ignored.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="words">The list of words to remove from the beginning.</param>
    /// <returns>Trimmed string.</returns>
    public static string TrimStart(this string str, params string[] words)
    {
        foreach (string word in words)
        {
            while (str.StartsWith(word, StringComparison.InvariantCultureIgnoreCase))
            {
                str = str.Substring(word.Length);
            }
        }
        return str;
    }

    /// <summary>
    /// Formats an Exception as a HTML string.
    /// </summary>
    /// <param name="ex">The exception to format.</param>
    /// <returns>Formatted HTML string.</returns>
    public static string ToHtmlString(this Exception ex)
    {
        string str = ex.ToString();
        str = str.Replace(Environment.NewLine, Environment.NewLine + "<br/>");
        str = str.Replace("  ", " &nbsp;");
        return string.Format("<font color=\"red\">{0}</font>", str);
    }
}
