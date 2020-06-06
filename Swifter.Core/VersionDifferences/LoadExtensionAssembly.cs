using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter
{
    partial class VersionDifferences
    {
        static void LoadExtensionAssembly()
        {
            try
            {
                var fileInfo = new FileInfo(typeof(VersionDifferences).Assembly.Location);

                foreach (var file in fileInfo.Directory.GetFiles().Where(file=> file.Name.StartsWith("Swifter") && file.Name.EndsWith($"Extensions{fileInfo.Extension}")))
                {
                    try
                    {
                        var assembly = Assembly.LoadFile(file.FullName);

                        if (assembly.GetType("Swifter.ExtensionLoader") is Type type)
                        {
                            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }
    }
}