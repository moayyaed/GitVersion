using GitVersion.Configuration;
using GitVersion.Core;
using GitVersion.Extensions;
using GitVersion.Helpers;
using GitVersion.OutputVariables;

namespace GitVersion.VersionCalculation;

internal sealed class VariableProvider(IEnvironment environment) : IVariableProvider
{
    private readonly IEnvironment environment = environment.NotNull();

    public GitVersionVariables GetVariablesFor(
        SemanticVersion semanticVersion, IGitVersionConfiguration configuration, int preReleaseWeight)
    {
        semanticVersion.NotNull();
        configuration.NotNull();

        var semverFormatValues = new SemanticVersionFormatValues(semanticVersion, configuration, preReleaseWeight);

        var informationalVersion = CheckAndFormatString(
            configuration.AssemblyInformationalFormat,
            semverFormatValues,
            semverFormatValues.InformationalVersion,
            "AssemblyInformationalVersion"
        );

        var assemblyFileSemVer = CheckAndFormatString(
            configuration.AssemblyFileVersioningFormat,
            semverFormatValues,
            semverFormatValues.AssemblyFileSemVer,
            "AssemblyFileVersioningFormat"
        );

        var assemblySemVer = CheckAndFormatString(
            configuration.AssemblyVersioningFormat,
            semverFormatValues,
            semverFormatValues.AssemblySemVer,
            "AssemblyVersioningFormat"
        );

        return new(
            semverFormatValues.Major,
            semverFormatValues.Minor,
            semverFormatValues.Patch,
            semverFormatValues.BuildMetaData,
            semverFormatValues.FullBuildMetaData,
            semverFormatValues.BranchName,
            semverFormatValues.EscapedBranchName,
            semverFormatValues.Sha,
            semverFormatValues.ShortSha,
            semverFormatValues.MajorMinorPatch,
            semverFormatValues.SemVer,
            semverFormatValues.FullSemVer,
            assemblySemVer,
            assemblyFileSemVer,
            semverFormatValues.PreReleaseTag,
            semverFormatValues.PreReleaseTagWithDash,
            semverFormatValues.PreReleaseLabel,
            semverFormatValues.PreReleaseLabelWithDash,
            semverFormatValues.PreReleaseNumber,
            semverFormatValues.WeightedPreReleaseNumber,
            informationalVersion,
            semverFormatValues.CommitDate,
            semverFormatValues.VersionSourceSha,
            semverFormatValues.CommitsSinceVersionSource,
            semverFormatValues.UncommittedChanges
        );
    }

    private string? CheckAndFormatString<T>(string? formatString, T source, string? defaultValue, string formatVarName)
    {
        string? formattedString;

        if (formatString.IsNullOrEmpty())
        {
            formattedString = defaultValue;
        }
        else
        {
            try
            {
                formattedString = formatString.FormatWith(source, this.environment)
                                              .RegexReplace(RegexPatterns.Output.SanitizeAssemblyInfoRegexPattern, "-");
            }
            catch (ArgumentException exception)
            {
                throw new WarningException($"Unable to format {formatVarName}.  Check your format string: {exception.Message}");
            }
        }

        return formattedString;
    }
}
