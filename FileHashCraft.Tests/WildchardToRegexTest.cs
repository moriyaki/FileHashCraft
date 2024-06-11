using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileHashCraft.ViewModels.PageSelectTarget;
using FileHashCraft.Models.FileScan;

namespace FileHashCraft.Tests
{
    public class WildchardToRegexTest
    {
        [Fact]
        public void WildcardFilename()
        {
            Regex regex = ScannedFilesManager.WildcardToRegexPattern("*.txt");

            Assert.Matches(regex, "test.txt");
            Assert.DoesNotMatch(regex, "test.csv");
            Assert.DoesNotMatch(regex, "test");
        }

        /*
        string pattern3 = "*.*";
        */
        [Fact]
        public void WildchardExtention()
        {
            Regex regex = ScannedFilesManager.WildcardToRegexPattern("file.*");

            Assert.Matches(regex, "file.txt");
            Assert.Matches(regex, "file.csv");
            Assert.Matches(regex, "file");
            Assert.DoesNotMatch(regex, "document.txt");
            Assert.DoesNotMatch(regex, "filename.txt");
        }

        [Fact]
        public void WildcardCharIncludeExtention()
        {
            Regex regex = ScannedFilesManager.WildcardToRegexPattern("*.m*");

            Assert.Matches(regex, "file.m.txt");
            Assert.Matches(regex, "file.m");
            Assert.Matches(regex, "file.mp4");
            Assert.Matches(regex, "file.tar.mp4");
            Assert.DoesNotMatch(regex, "file");
            Assert.DoesNotMatch(regex, "file.aac");
        }

        [Fact]
        public void WildcardAllAsterisk1()
        {
            Regex regex = ScannedFilesManager.WildcardToRegexPattern("*");

            Assert.Matches(regex, "file.m.txt");
            Assert.Matches(regex, "file.m");
            Assert.Matches(regex, "file.mp4");
            Assert.Matches(regex, "file.tar.mp4");
            Assert.Matches(regex, "file");
            Assert.Matches(regex, "file.aac");
            Assert.Matches(regex, "document.txt");
        }

        [Fact]
        public void WildcardAllAsterisk2()
        {
            Regex regex = ScannedFilesManager.WildcardToRegexPattern("*.*");

            Assert.Matches(regex, "file.m.txt");
            Assert.Matches(regex, "file.m");
            Assert.Matches(regex, "file.mp4");
            Assert.Matches(regex, "file.tar.mp4");
            Assert.Matches(regex, "file");
            Assert.Matches(regex, "file.aac");
            Assert.Matches(regex, "document.txt");
        }
    }
}
