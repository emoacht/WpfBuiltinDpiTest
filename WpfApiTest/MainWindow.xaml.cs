using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using static WpfApiTest.DpiHelper;

namespace WpfApiTest
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			FindHwndSource();
		}

		private HwndSource _source;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			//Debug.WriteLine(SetThreadAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE));

			Debug.WriteLine(GetThreadAwarenessContext());
			Debug.WriteLine(GetWindowAwarenessContext(this));
			Debug.WriteLine(GetProcessAwareness());

			Debug.WriteLine(EnableScaling(this));

			_source = PresentationSource.FromVisual(this) as HwndSource;
			_source?.AddHook(WndProc);

			FindHwndSource();
		}

		private const int WM_NCCREATE = 0x0081;

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_NCCREATE)
			{
				Debug.WriteLine("WM_NCCREATE");
			}

			return IntPtr.Zero;
		}

		private void FindHwndSource()
		{
			var sourceWindowHelperType = typeof(Window).Assembly.GetType("System.Windows.Window+SourceWindowHelper");
			var sourceWindowHelperField = typeof(Window).GetField("_swh", BindingFlags.NonPublic | BindingFlags.Instance);
			var sourceWindowHelperValue = sourceWindowHelperField.GetValue(this);

			Debug.WriteLine($"SourceWindowHelper: {sourceWindowHelperValue != null}");

			if (sourceWindowHelperValue != null)
			{
				var hwndSourceField = sourceWindowHelperType.GetField("_sourceWindow", BindingFlags.NonPublic | BindingFlags.Instance);
				var hwndSourceValue = hwndSourceField.GetValue(sourceWindowHelperValue) as HwndSource;

				Debug.WriteLine($"HwndSource: {hwndSourceValue != null}");
			}
		}
	}
}