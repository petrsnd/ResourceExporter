using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResourceExporter;

namespace ResourceExporterTest
{
    [TestClass]
    public class EmbeddedResourceExtractorTest
    {
        private static string _workingDirectory = "";

        private const string BuildConfiguration = 
#if DEBUG
            "Debug";
#else
            "Release";
#endif

        private const string TestProjectRelativePath = @"..\..\..\..\ClassLibraryWithEmbeddedResources";

        private string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private string TestAssemblyRelativePath
        {
            get
            {
                return string.Format(
                    @"{0}\bin\{1}\ClassLibraryWithEmbeddedResources.dll",
                    TestProjectRelativePath, BuildConfiguration);
            }
        }

        private string TestProjectFullPath
        {
            get { return Path.GetFullPath(AssemblyDirectory + TestProjectRelativePath); }
        }

        private string TestAssemblyFullPath
        {
            get { return Path.GetFullPath(AssemblyDirectory + TestAssemblyRelativePath); }
        }

        [ClassInitialize]
        public static void Setup(TestContext textContext)
        {
            _workingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_workingDirectory);
        }

        [ClassCleanup]
        public static void TearDown()
        {
            Directory.Delete(_workingDirectory, true);
        }

        [TestMethod]
        public void TestGetResourceNames()
        {
            var extractor = new EmbeddedResourceExtractor(TestAssemblyFullPath);
            var actual = extractor.GetEmbeddedResourceNames();
            Array.Sort(actual);
            CollectionAssert.AreEqual(new[]
                {
                    "bible-kjv.txt",
                    "fotr.pdf",
                    "huckleberryfinn.epub",
                    "hypertrm.dll",
                    "hypertrm.exe"
                }, actual);
        }

        [TestMethod]
        public void TestGetResourceInfos()
        {
            var extractor = new EmbeddedResourceExtractor(TestAssemblyFullPath);
            var actual = extractor.GetEmbeddedResourceInfos().ToArray();
            Array.Sort(actual,
                (i1, i2) => string.Compare(i1.FileName, i2.FileName, StringComparison.Ordinal));
            var expected = new[]
            {
                new EmbeddedResourceInfo
                {
                    FileName = "bible-kjv.txt",
                    FileType = "Text Document",
                    Size = 4452069
                },
                new EmbeddedResourceInfo
                {
                    FileName = "fotr.pdf",
                    FileType = "PDF File",
                    Size = 697344
                },
                new EmbeddedResourceInfo
                {
                    FileName = "huckleberryfinn.epub",
                    FileType = "EPUB File",
                    Size = 13523542
                },
                new EmbeddedResourceInfo
                {
                    FileName = "hypertrm.dll",
                    FileType = "Application Extension",
                    Size = 345088
                },
                new EmbeddedResourceInfo
                {
                    FileName = "hypertrm.exe",
                    FileType = "Application",
                    Size = 28160
                }
            };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGetEmbeddedResourceInfoByName()
        {
            var extractor = new EmbeddedResourceExtractor(TestAssemblyFullPath);
            Assert.IsNull(extractor.GetEmbeddedResourceInfoByName("doesn't exist"));
            var actual = extractor.GetEmbeddedResourceInfoByName("hypertrm.dll");
            Assert.AreEqual(new EmbeddedResourceInfo
                {
                    FileName = "hypertrm.dll",
                    FileType = "Application Extension",
                    Size = 345088
                }, actual);
        }

        [TestMethod]
        public void TestExtractEmbeddedResourceToDirectory()
        {
            const string resourceName = "huckleberryfinn.epub";
            var extractor = new EmbeddedResourceExtractor(TestAssemblyFullPath);
            extractor.ExtractEmbeddedResourceToDirectory(resourceName, _workingDirectory);
            Assert.IsTrue(File.Exists(Path.Combine(_workingDirectory, resourceName)));
            byte[] expectedHash, actualHash;
            using (var sha1 = SHA1.Create())
            using (var expectedFile = File.OpenRead(Path.Combine(TestProjectFullPath, resourceName)))
                expectedHash = sha1.ComputeHash(expectedFile);
            using (var sha1 = SHA1.Create())
            using (var actualFile = File.OpenRead(Path.Combine(_workingDirectory, resourceName)))
                actualHash = sha1.ComputeHash(actualFile);
            CollectionAssert.AreEqual(expectedHash, actualHash);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestExtractEmbeddedResourceToFileThrows()
        {
            var extractor = new EmbeddedResourceExtractor(TestAssemblyFullPath);
            extractor.ExtractEmbeddedResourceToFile("asdf.txt", _workingDirectory);
        }

        [TestMethod]
        public void TestExtractEmbeddedResourceToFile()
        {
            const string fileName = "abc.txt";
            const string resourceName = "bible-kjv.txt";
            var extractor = new EmbeddedResourceExtractor(TestAssemblyFullPath);
            extractor.ExtractEmbeddedResourceToFile(resourceName, Path.Combine(_workingDirectory, fileName));
            Assert.IsTrue(File.Exists(Path.Combine(_workingDirectory, fileName)));
            byte[] expectedHash, actualHash;
            using (var sha1 = SHA1.Create())
            using (var expectedFile = File.OpenRead(Path.Combine(TestProjectFullPath, resourceName)))
                expectedHash = sha1.ComputeHash(expectedFile);
            using (var sha1 = SHA1.Create())
            using (var actualFile = File.OpenRead(Path.Combine(_workingDirectory, fileName)))
                actualHash = sha1.ComputeHash(actualFile);
            CollectionAssert.AreEqual(expectedHash, actualHash);
        }
    }
}
