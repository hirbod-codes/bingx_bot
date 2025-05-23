using bot.src.Configuration.Sources.DockerSecrets;

namespace bot.src.Configuration.Providers.DockerSecrets;

/// <summary>
/// Provides ability to read configuration parameters from docker secrets files.
/// </summary>
public static class DockerSecretsConfigurationExtension
{
    private const string DefaultSecretsDirectoryPath = "/run/secrets";
    private const string DefaultColonPlaceholder = "__";
    private const char PrefixesEnvironmentVariableDelimiter = ',';

    /// <summary>
    /// Read configuration from mounted docker secrets files
    /// </summary>
    /// <param name="configurationBuilder">Configuration builder</param>
    /// <param name="secretsDirectoryPath">Absolute path to the folder inside the container that holds the mounted Docker secret files.</param>
    /// <param name="colonPlaceholder">Provided placeholder value will be replaced with `:` within the secret filename.</param>
    /// <param name="allowedPrefixes">If not null and not empty reads only secret files that start with any of the provided prefixes.</param>
    /// <returns>Configuration builder</returns>
    public static IConfigurationBuilder AddDockerSecrets(
        this IConfigurationBuilder configurationBuilder,
        string secretsDirectoryPath = DefaultSecretsDirectoryPath,
        string colonPlaceholder = DefaultColonPlaceholder,
        ICollection<string>? allowedPrefixes = null
    ) => configurationBuilder.Add(new DockerSecretsConfigurationsSource(secretsDirectoryPath, colonPlaceholder, allowedPrefixes));

    /// <summary>
    /// Read configuration from mounted docker secrets files
    /// </summary>
    /// <param name="configurationBuilder">Configuration builder</param>
    /// <param name="allowedPrefixesCommaDelimited">comma separated allowed prefixes. If prefixes are defined then processed are only secrets which file names start with any of the provided
    /// prefixes.</param>
    /// <param name="secretsDirectoryPath">Path to the folder what holds the mounted Docker secret files.</param>
    /// <param name="colonPlaceholder">Provided placeholder value will be replaced with `:` within the secret filename.</param>
    /// <returns>Configuration builder</returns>
    public static IConfigurationBuilder AddDockerSecrets(
        this IConfigurationBuilder configurationBuilder,
        string? allowedPrefixesCommaDelimited = null,
        string secretsDirectoryPath = DefaultSecretsDirectoryPath,
        string colonPlaceholder = DefaultColonPlaceholder
    )
    {
        if (string.IsNullOrWhiteSpace(allowedPrefixesCommaDelimited)) return AddDockerSecrets(configurationBuilder, secretsDirectoryPath: secretsDirectoryPath, colonPlaceholder);

        ICollection<string>? allowedDockerPrefixes = string.IsNullOrWhiteSpace(allowedPrefixesCommaDelimited)
            ? null
            : allowedPrefixesCommaDelimited
                .Split(PrefixesEnvironmentVariableDelimiter)
                .Select(prefix => prefix.Trim())
                .Where(prefix => !string.IsNullOrWhiteSpace(prefix))
                .ToList();

        return configurationBuilder.AddDockerSecrets(secretsDirectoryPath, colonPlaceholder, allowedDockerPrefixes);
    }

    /// <summary>
    /// Read configuration from mounted docker secrets files
    /// </summary>
    /// <param name="configurationBuilder">Configuration builder</param>
    /// <param name="allowedPrefixes">allowed prefixes. If prefixes are defined then processed files are only secrets which file names start with any of the provided
    /// prefixes.</param>
    /// <param name="secretsDirectoryPath">Path to the folder what holds the mounted Docker secret files.</param>
    /// <param name="colonPlaceholder">Provided placeholder value will be replaced with `:` within the secret filename.</param>
    /// <returns>Configuration builder</returns>
    public static IConfigurationBuilder AddDockerSecrets(
        this IConfigurationBuilder configurationBuilder,
        IEnumerable<string>? allowedPrefixes = null,
        string secretsDirectoryPath = DefaultSecretsDirectoryPath,
        string colonPlaceholder = DefaultColonPlaceholder
    )
    {
        if (allowedPrefixes == null || !allowedPrefixes.Any()) return AddDockerSecrets(configurationBuilder, secretsDirectoryPath: secretsDirectoryPath, colonPlaceholder);

        return configurationBuilder.AddDockerSecrets(secretsDirectoryPath, colonPlaceholder, (ICollection<string>?)allowedPrefixes);
    }
}
