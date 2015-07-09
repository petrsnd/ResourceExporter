using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResourceExporter;

namespace ResourceExporterTest
{
    [TestClass]
    public class EmbeddedResourceExtractorTest
    {
        private const string BuildConfiguration = 
#if DEBUG
            "Debug";
#else
            "Release";
#endif

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
                    @"..\..\..\..\ClassLibraryWithEmbeddedResources\bin\{0}\ClassLibraryWithEmbeddedResources.dll",
                    BuildConfiguration);
            }
        }

        private string TestAssemblyFullPath
        {
            get { return Path.GetFullPath(AssemblyDirectory + TestAssemblyRelativePath); }
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
    }
}
