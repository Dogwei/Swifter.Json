using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Swifter 为 AspNet 提供的扩展方法。
/// </summary>
public static partial class SwifterExtensions
{
    internal const string JSONContentType = "application/json";

    internal static string GetNonEmptyString(string st1, string st2, string st3) => string.IsNullOrEmpty(st1) ? string.IsNullOrEmpty(st2) ? st3 : st2 : st1;
}