using System.Text.RegularExpressions;
using FileHashCraft.Models.FileScan;

namespace FileHashCraft.Tests
{
    public class WildcardToRegexPatternTests
    {
        [Fact]
        public void FileNameAsteriskTest()
        {
            var wildcardPattern = "craft*.txt";
            var expectedPattern = "^craft.*\\.txt$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileExtentionAsteriskTest()
        {
            var wildcardPattern = "file.*";
            var expectedPattern = "^file(?:\\..+)?$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameQuestionTest()
        {
            var wildcardPattern = "file?.log";
            var expectedPattern = "^file.\\.log$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameAllAsteriskTest()
        {
            var wildcardPattern = "*.log";
            var expectedPattern = "^.*\\.log$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameNoWildcardTest()
        {
            var wildcardPattern = "file";
            var expectedPattern = "^file$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameBracketsEscapeTest()
        {
            var wildcardPattern = "file[1].txt";
            var expectedPattern = "^file\\[1]\\.txt$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameParenthesesEscapeTest()
        {
            var wildcardPattern = "file(1).txt";
            var expectedPattern = "^file\\(1\\)\\.txt$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameCurlyBracesEscapeTest()
        {
            var wildcardPattern = "file{1}.txt";
            var expectedPattern = "^file\\{1}\\.txt$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }


        [Fact]
        public void FileNamePlusEscapeTest()
        {
            var wildcardPattern = "file+.log";
            var expectedPattern = "^file\\+\\.log$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }

        [Fact]
        public void FileNameStartEndEscapeTest()
        {
            var wildcardPattern = "file^$.log";
            var expectedPattern = "^file\\^\\$\\.log$";
            // Act
            var result = ScannedFilesManager.WildcardToRegexPattern(wildcardPattern);
            // Result
            Assert.Equal(expectedPattern, result.ToString());
        }
    }
}
