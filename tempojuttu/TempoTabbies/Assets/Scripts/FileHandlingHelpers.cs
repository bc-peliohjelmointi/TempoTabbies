using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Centralized helper methods for file-system operations used by gameplay and profile/config persistence code.
/// </summary>
/// <remarks>
/// Design goals:
/// <list type="bullet">
/// <item><description>Provide explicit, reusable guardrails for common file I/O operations.</description></item>
/// <item><description>Normalize and validate user-provided file name input (for example, profile names).</description></item>
/// <item><description>Guarantee consistent path-building through <see cref="Path.Combine(string[])"/>.</description></item>
/// <item><description>Offer non-throwing read/write helpers that return a success flag + error text.</description></item>
/// </list>
/// The methods in this class are intentionally explicit and verbose to make failure modes obvious during maintenance.
/// </remarks>
public static class FileHandlingHelpers
{
    // Allow letters, digits, underscore, hyphen, and space. Keep this strict to avoid path ambiguity.
    private static readonly Regex SafeNamePattern = new Regex(@"^[A-Za-z0-9_\- ]+$", RegexOptions.Compiled);

    /// <summary>
    /// Attempts to normalize and validate a user-provided profile/config name so it is safe to use as a file name.
    /// </summary>
    /// <param name="rawName">
    /// The untrusted input value, such as a profile name entered by a user.
    /// </param>
    /// <param name="safeName">
    /// On success, receives the normalized safe name to use in file paths.
    /// </param>
    /// <param name="error">
    /// On failure, receives a human-readable reason describing why validation failed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the value is safe for file-name use; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Validation rules:
    /// <list type="number">
    /// <item><description>Name must not be null, empty, or whitespace.</description></item>
    /// <item><description>Zero-width spaces are removed before validation.</description></item>
    /// <item><description>Path separators and traversal forms are rejected.</description></item>
    /// <item><description>Only a strict allowlist of characters is accepted.</description></item>
    /// <item><description>Name must survive <see cref="Path.GetFileName(string)"/> unchanged.</description></item>
    /// </list>
    /// </remarks>
    public static bool TryNormalizeSafeName(string rawName, out string safeName, out string error)
    {
        safeName = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(rawName))
        {
            error = "Name is null, empty, or whitespace.";
            return false;
        }

        // Remove zero-width spaces that can hide malicious or confusing file names in UI.
        string candidate = rawName.Replace("\u200B", string.Empty).Trim();

        if (candidate.Length == 0)
        {
            error = "Name is empty after normalization.";
            return false;
        }

        // Fast block for explicit traversal indicators.
        if (candidate.Contains("..", StringComparison.Ordinal))
        {
            error = "Name contains forbidden traversal sequence '..'.";
            return false;
        }

        if (candidate.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            error = "Name contains invalid file-name characters.";
            return false;
        }

        if (candidate.Contains('/') || candidate.Contains('\\'))
        {
            error = "Name contains path separators.";
            return false;
        }

        if (!SafeNamePattern.IsMatch(candidate))
        {
            error = "Name contains unsupported characters. Allowed: A-Z, a-z, 0-9, space, '_', '-'.";
            return false;
        }

        string fileNameOnly = Path.GetFileName(candidate);
        if (!string.Equals(candidate, fileNameOnly, StringComparison.Ordinal))
        {
            error = "Name was transformed by Path.GetFileName and is therefore unsafe.";
            return false;
        }

        safeName = candidate;
        return true;
    }

    /// <summary>
    /// Builds a profile JSON path under a specific root folder using a previously validated safe name.
    /// </summary>
    /// <param name="rootDirectory">Base directory (for example, "JSON").</param>
    /// <param name="safeName">A name that has passed <see cref="TryNormalizeSafeName"/>.</param>
    /// <returns>A combined path in the form <c>{rootDirectory}/{safeName}.json</c>.</returns>
    public static string BuildProfileJsonPath(string rootDirectory, string safeName)
    {
        return Path.Combine(rootDirectory, safeName + ".json");
    }

    /// <summary>
    /// Builds the game-manager settings JSON file path in a platform-safe way.
    /// </summary>
    /// <param name="rootDirectory">Base directory (for example, "JSON").</param>
    /// <returns>A combined path in the form <c>{rootDirectory}/GameManager/_GameManager.json</c>.</returns>
    public static string BuildGameManagerSettingsPath(string rootDirectory)
    {
        return Path.Combine(rootDirectory, "GameManager", "_GameManager.json");
    }

    /// <summary>
    /// Reads text from disk without throwing to callers. Returns false and sets an error string on failure.
    /// </summary>
    /// <param name="path">Absolute or relative file path.</param>
    /// <param name="content">On success, receives full file content; otherwise empty string.</param>
    /// <param name="error">On failure, receives a diagnostic reason.</param>
    /// <returns><see langword="true"/> if read succeeded; otherwise <see langword="false"/>.</returns>
    public static bool TryReadAllText(string path, out string content, out string error)
    {
        content = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Path is null, empty, or whitespace.";
            return false;
        }

        try
        {
            if (!File.Exists(path))
            {
                error = $"File does not exist: {path}";
                return false;
            }

            content = File.ReadAllText(path);
            return true;
        }
        catch (Exception ex)
        {
            error = $"Read failed for '{path}': {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Writes text to disk without throwing to callers. Parent directory is created if missing.
    /// </summary>
    /// <param name="path">Absolute or relative destination path.</param>
    /// <param name="content">Text to write (null becomes empty string).</param>
    /// <param name="error">On failure, receives a diagnostic reason.</param>
    /// <returns><see langword="true"/> if write succeeded; otherwise <see langword="false"/>.</returns>
    public static bool TryWriteAllText(string path, string content, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Path is null, empty, or whitespace.";
            return false;
        }

        try
        {
            EnsureParentDirectoryExists(path);
            File.WriteAllText(path, content ?? string.Empty);
            return true;
        }
        catch (Exception ex)
        {
            error = $"Write failed for '{path}': {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Ensures the parent directory of <paramref name="path"/> exists.
    /// </summary>
    /// <param name="path">Path whose parent directory should exist.</param>
    /// <exception cref="ArgumentException">Thrown when path is null/empty/whitespace.</exception>
    public static void EnsureParentDirectoryExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path must not be null, empty, or whitespace.", nameof(path));
        }

        string directory = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(directory))
        {
            // No directory component means current directory; nothing to create.
            return;
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Helper for consistent warning logs related to file operations.
    /// </summary>
    /// <param name="context">Short operation context, for example "LoadPlayer".</param>
    /// <param name="message">Detailed message.</param>
    public static void LogFileWarning(string context, string message)
    {
        Debug.LogWarning($"[FileHandling:{context}] {message}");
    }

    /// <summary>
    /// Helper for consistent error logs related to file operations.
    /// </summary>
    /// <param name="context">Short operation context, for example "SaveGameManager".</param>
    /// <param name="message">Detailed message.</param>
    public static void LogFileError(string context, string message)
    {
        Debug.LogError($"[FileHandling:{context}] {message}");
    }
}
