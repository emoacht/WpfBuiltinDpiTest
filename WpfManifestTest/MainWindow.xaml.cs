using System;
using System.Collections.Generic;
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

			CheckManifest();
		}

		private void CheckManifest()
		{
			// Build Action of app.manifest must be Embedded Resource.

			var assembly = Assembly.GetExecutingAssembly();
			var manifest = $"{assembly.GetName().Name}.app.manifest";

			if (!assembly.GetManifestResourceNames().Contains(manifest))
				return;

			using (var stream = assembly.GetManifestResourceStream(manifest))
			using (var reader = new StreamReader(stream))
			{
				var content = XDocument.Load(reader);

				var dpiAware = content.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("dpiAware", StringComparison.Ordinal))?.Value;
				var dpiAwareness = content.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("dpiAwareness", StringComparison.Ordinal))?.Value;

				this.TextDpiAware.Text = dpiAware;
				this.TextDpiAwareness.Text = dpiAwareness;
			}
		}
	}
}