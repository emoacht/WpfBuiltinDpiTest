using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace TargetFrameworkTest
{
	public static class TargetFrameworkChecker
	{
		public static Version GetTargetFrameworkVersion()
		{
			var assembly = Assembly.GetCallingAssembly();

			return GetTargetFrameworkVersion(assembly);
		}

		public static Version GetTargetFrameworkVersion(Window window)
		{
			var assembly = Assembly.GetAssembly(window.GetType());

			return GetTargetFrameworkVersion(assembly);
		}

		public static Version GetTargetFrameworkVersion(Assembly assembly)
		{
			var atr = assembly?.GetCustomAttributes(typeof(TargetFrameworkAttribute), false);

			var tfv = atr?.FirstOrDefault() as TargetFrameworkAttribute;
			if (tfv == null)
				return null;

			var pattern = new Regex(@"Version=v(?<version>\d\.\d\.\d)$");
			var match = pattern.Match(tfv.FrameworkName);
			if (!match.Success)
				return null;

			return new Version(match.Groups["version"].Value);
		}
	}
}