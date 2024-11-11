using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FileHashCraft.Models.FileScan;

namespace FileHashCraft.Tests
{
    public class WildcardToRegexPatternTests
    {
        [Fact]
        public void FileNameAsteriskTest()
        {
            const string wildcardPattern = "craft*.txt";
            const string expectedPattern = "^craft.*\\.txt$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileExtentionAsteriskTest()
        {
            const string wildcardPattern = "file.*";
            const string expectedPattern = "^file(?:\\..+)?$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameQuestionTest()
        {
            const string wildcardPattern = "file?.log";
            const string expectedPattern = "^file.\\.log$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameAllAsteriskTest()
        {
            const string wildcardPattern = "*.log";
            const string expectedPattern = "^.*\\.log$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameNoWildcardTest()
        {
            const string wildcardPattern = "file";
            const string expectedPattern = "^file$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameBracketsEscapeTest()
        {
            const string wildcardPattern = "file[1].txt";
            const string expectedPattern = "^file\\[1]\\.txt$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameParenthesesEscapeTest()
        {
            const string wildcardPattern = "file(1).txt";
            const string expectedPattern = "^file\\(1\\)\\.txt$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameCurlyBracesEscapeTest()
        {
            const string wildcardPattern = "file{1}.txt";
            const string expectedPattern = "^file\\{1}\\.txt$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNamePlusEscapeTest()
        {
            const string wildcardPattern = "file+.log";
            const string expectedPattern = "^file\\+\\.log$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameStartEndEscapeTest()
        {
            const string wildcardPattern = "file^$.log";
            const string expectedPattern = "^file\\^\\$\\.log$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }
    }
}
