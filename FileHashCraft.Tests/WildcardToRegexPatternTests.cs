using System.Text.RegularExpressions;
using FileHashCraft.Models.FileScan;

namespace FileHashCraft.Tests
{
    public class WildcardToRegexPatternTests
    {
        [Theory]
        [InlineData("*.txt", "^.*\\.txt$", RegexOptions.IgnoreCase)]
        [InlineData("file.*", "^file(?:\\..+)?$", RegexOptions.IgnoreCase)]
        [InlineData("file?.log", "^file.\\.log$", RegexOptions.IgnoreCase)]
        [InlineData("*.log", "^.*\\.log$", RegexOptions.IgnoreCase)]
        [InlineData("file", "^file$", RegexOptions.IgnoreCase)]
        public void WildcardToRegexPattern_ShouldReturnCorrectRegex(string wildcardPattern, string expectedPattern, RegexOptions options)
        {
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);

            // Assert
            Assert.Equal(expectedPattern, result.ToString());
            Assert.Equal(options, result.Options);
        }
    }
}
