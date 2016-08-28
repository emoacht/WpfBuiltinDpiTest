using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace WpfManifestTest
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			var assembly = Assembly.GetExecutingAssembly();

			CheckManifest(assembly);
			CheckConfig(assembly);
		}

		private void CheckManifest(Assembly assembly)
		{
			// Build Action of app.manifest must be Embedded Resource.

			var manifest = $"{assembly.GetName().Name}.app.manifest";

			if (!assembly.GetManifestResourceNames().Contains(manifest))
				return;

			using (var stream = assembly.GetManifestResourceStream(manifest))
			using (var reader = new StreamReader(stream))
			{
				var content = XDocument.Load(reader);

				var dpiAware = content.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("dpiAware", StringComparison.Ordinal))?.Value;
				var dpiAwareness = content.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("dpiAwareness", StringComparison.Ordinal))?.Value;

				this.DpiAwareValue.Text = dpiAware;
				this.DpiAwarenessValue.Text = dpiAwareness;
			}
		}

		private void CheckConfig(Assembly assembly)
		{
			var exeUri = new UriBuilder(assembly.CodeBase);
			var exePath = Uri.UnescapeDataString(exeUri.Path);
			var config = ConfigurationManager.OpenExeConfiguration(exePath);

			var doNotScaleForDpiChanges = GetDoNotScaleForDpiChanges(config);

			this.DoNotScaleForDpiChangesValue.Text = doNotScaleForDpiChanges?.ToString();
		}

		private static bool? GetDoNotScaleForDpiChanges(Configuration config)
		{
			var xml = config.GetSection("runtime")?.SectionInformation?.GetRawXml();
			if (xml == null)
				return null;

			var section = XDocument.Parse(xml);
			var element = section?.Descendants()?.SingleOrDefault(x => x.Name.LocalName.Equals("AppContextSwitchOverrides", StringComparison.OrdinalIgnoreCase));
			var attribute = element?.Attributes()?.SingleOrDefault(x => x.Name.LocalName.Equals("value", StringComparison.OrdinalIgnoreCase));

			var fields = attribute.Value.Split('=').Select(x => x.Trim()).ToArray();
			if (fields.Length != 2)
				return null;

			if (!fields[0].Equals("Switch.System.Windows.DoNotScaleForDpiChanges", StringComparison.OrdinalIgnoreCase))
				return null;

			bool value;
			if (!bool.TryParse(fields[1], out value))
				return null;

			return value;
		}
	}
}