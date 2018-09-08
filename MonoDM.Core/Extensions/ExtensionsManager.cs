using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
namespace MonoDM.Core.Extensions
{
	public static class ExtensionsManager
	{
		public static List<IExtension> LoadAllExtensions(string extFolder) {
			List<IExtension> list = new List<IExtension>();
			var assemblies = Directory.GetFiles(extFolder, "*.dll");
			var interfaceType = typeof(IExtension);
			foreach (var asmPath in assemblies)
			{
				try
				{
					var asm = Assembly.LoadFile(Path.GetFileName(asmPath));
					var types = asm.GetModules()[0].GetTypes().Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract).ToList();
					list.AddRange(types.Select(s => s.GetConstructor(new Type[0]).Invoke(new object[0])).Cast<IExtension>());
				}
				catch { }
			}
			return list;
		}
	}
}
