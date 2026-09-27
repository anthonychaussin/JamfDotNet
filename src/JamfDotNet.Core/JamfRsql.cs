using System.Globalization;
using System.Text;

namespace JamfDotNet.Core;

/// <summary>
/// Lightweight RSQL expression builder for Jamf Pro <c>filter</c> query parameters.
/// </summary>
/// <remarks>
/// Produces strings such as <c>name=="Apps*"</c> or <c>name=="Admin" and siteId==-1</c>.
/// This is intentionally not a full RSQL DSL — use <see cref="Raw"/> for advanced expressions.
/// </remarks>
public static class JamfRsql
{
    /// <summary>Builds <c>field==value</c> (quotes string values).</summary>
    /// <param name="field">Field name.</param>
    /// <param name="value">Comparison value.</param>
    /// <returns>RSQL equality clause.</returns>
    public static string Eq(string field, object? value) => Binary(field, "==", value);

    /// <summary>Builds <c>field!=value</c>.</summary>
    /// <param name="field">Field name.</param>
    /// <param name="value">Comparison value.</param>
    /// <returns>RSQL inequality clause.</returns>
    public static string Ne(string field, object? value) => Binary(field, "!=", value);

    /// <summary>Builds <c>field=like=pattern</c> (Jamf often accepts <c>==</c> with <c>*</c> wildcards instead).</summary>
    /// <param name="field">Field name.</param>
    /// <param name="pattern">Like pattern.</param>
    /// <returns>RSQL like clause.</returns>
    public static string Like(string field, string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentNullException.ThrowIfNull(pattern);
        return $"{field}=like={Quote(pattern)}";
    }

    /// <summary>Builds <c>field=in=(a,b,...)</c>.</summary>
    /// <param name="field">Field name.</param>
    /// <param name="values">Values to match.</param>
    /// <returns>RSQL in clause.</returns>
    public static string In(string field, params object?[] values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
        {
            throw new ArgumentException("At least one value is required.", nameof(values));
        }

        var joined = string.Join(",", values.Select(FormatValue));
        return $"{field}=in=({joined})";
    }

    /// <summary>Combines clauses with <c> and </c>.</summary>
    /// <param name="clauses">Non-empty RSQL clauses.</param>
    /// <returns>Combined expression.</returns>
    public static string And(params string[] clauses) => Combine("and", clauses);

    /// <summary>Combines clauses with <c> or </c>.</summary>
    /// <param name="clauses">Non-empty RSQL clauses.</param>
    /// <returns>Combined expression.</returns>
    public static string Or(params string[] clauses) => Combine("or", clauses);

    /// <summary>Passes through a pre-built RSQL expression after trimming.</summary>
    /// <param name="expression">Raw RSQL.</param>
    /// <returns>Trimmed expression.</returns>
    public static string Raw(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        return expression.Trim();
    }

    private static string Binary(string field, string op, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        return $"{field}{op}{FormatValue(value)}";
    }

    private static string Combine(string op, string[] clauses)
    {
        ArgumentNullException.ThrowIfNull(clauses);
        if (clauses.Length == 0)
        {
            throw new ArgumentException("At least one clause is required.", nameof(clauses));
        }

        var parts = clauses
            .Where(static c => !string.IsNullOrWhiteSpace(c))
            .Select(static c => c.Trim())
            .ToArray();
        if (parts.Length == 0)
        {
            throw new ArgumentException("At least one non-empty clause is required.", nameof(clauses));
        }

        return string.Join($" {op} ", parts);
    }

    private static string FormatValue(object? value) => value switch
    {
        null => "null",
        bool b => b ? "true" : "false",
        byte or sbyte or short or ushort or int or uint or long or ulong => Convert.ToString(value, CultureInfo.InvariantCulture)!,
        float or double or decimal => Convert.ToString(value, CultureInfo.InvariantCulture)!,
        string s => Quote(s),
        _ => Quote(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty),
    };

    private static string Quote(string value)
    {
        var escaped = value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
        return $"\"{escaped}\"";
    }
}
