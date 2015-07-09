using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace ResourceExporter
{
    public class EmbeddedResourceExtractor
    {
        private readonly Assembly _assembly;

        public EmbeddedResourceExtractor() :
            this(Assembly.GetExecutingAssembly())
        {
        }

        public EmbeddedResourceExtractor(string assemblyNameOrPath) :
            this(Assembly.LoadFrom(assemblyNameOrPath))
        {
        }

        public EmbeddedResourceExtractor(Assembly assembly)
        {
            _assembly = assembly;
        }

        public string GetAssemblyName()
        {
            return _assembly.GetName().Name;
        }

        public string[] GetEmbeddedResourceNames()
        {
            return GetEmbeddedResourceInfos().Select(i => i.FileName).ToArray();
        }

        public IEnumerable<EmbeddedResourceInfo> GetEmbeddedResourceInfos()
        {
            return new EmbeddedResourceInfoEnumerator(_assembly, _assembly.GetManifestResourceNames());
        }

        public EmbeddedResourceInfo GetEmbeddedResourceInfoByName(string resourceName)
        {
            return CreateEmbeddedResourceInfo(_assembly, GetAssemblyName() + "." + resourceName);
        }

        //public void ExtractEmbeddedResourceToDirectory(string resourceName, string targetDirectoryPath)
        //public void ExtractEmbeddedResourceToFile(string resourceName, string extractedFilePath)

        #region Private EmbeddedResourceInfoEnumerator class for IEnumerable<EmbeddedResourceInfo>
        private class EmbeddedResourceInfoEnumerator : IEnumerable<EmbeddedResourceInfo>
        {
            private readonly Assembly _assembly;
            private readonly string[] _resourceNames;

            public EmbeddedResourceInfoEnumerator(Assembly assembly, string[] resourceNames)
            {
                _assembly = assembly;
                _resourceNames = resourceNames;
            }

            public IEnumerator<EmbeddedResourceInfo> GetEnumerator()
            {
                return _resourceNames.Select(resourceName => CreateEmbeddedResourceInfo(_assembly, resourceName))
                    .Where(embeddedResourceInfo => embeddedResourceInfo != null).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        #endregion

        #region Static helper methods
        private static EmbeddedResourceInfo CreateEmbeddedResourceInfo(Assembly assembly, string resourceName)
        {
            var assemblyPrefix = assembly.GetName().Name + ".";
            var resourceInfo = assembly.GetManifestResourceInfo(resourceName);
            if (resourceInfo == null)
                return null;
            if ((resourceInfo.ResourceLocation & ResourceLocation.Embedded) != ResourceLocation.Embedded)
                return null;
            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                var extension = Path.GetExtension(resourceName);
                return new EmbeddedResourceInfo
                {
                    FileName = resourceName.Substring(assemblyPrefix.Length),
                    FileType = GetFileTypeFromExtension(extension),
                    Size = (resourceStream == null ? 0 : resourceStream.Length)
                };
            }
        }

        private static string GetFileTypeFromExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension) || extension == ".")
                return "Unknown Type";
            var identifier = (string)Registry.GetValue(string.Format(@"HKEY_CLASSES_ROOT\{0}", extension), null, null);
            if (!string.IsNullOrEmpty(identifier))
            {
                var fileType = (string)Registry.GetValue(string.Format(@"HKEY_CLASSES_ROOT\{0}", identifier), null, null);
                if (!string.IsNullOrEmpty(fileType))
                    return fileType;
            }
            return extension.TrimStart(new[] { '.' }).ToUpper() + " File";
        }
        #endregion

    }
}
