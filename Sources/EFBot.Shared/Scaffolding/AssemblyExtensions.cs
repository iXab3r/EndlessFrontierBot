﻿using System.IO;
using System.Reflection;
using System.Resources;
using JetBrains.Annotations;

namespace EFBot.Shared.Scaffolding
{
    public static class AssemblyExtensions
    {
        public static string ReadResourceAsString(this Assembly assembly, [NotNull] string path)
        {
            var assemblyName = assembly.GetName().Name;
            var resourcePath = $"{assemblyName}.{path}";
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    var assemblyResources = assembly.GetManifestResourceNames();
                    throw new MissingManifestResourceException($"Could not find resource '{resourcePath}'. Assembly '{assemblyName}' resources list:\n\t{assemblyResources.DumpToText()}");
                }
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
