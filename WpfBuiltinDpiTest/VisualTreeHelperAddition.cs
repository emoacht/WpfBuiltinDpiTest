using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace WpfBuiltinDpiTest
{
	public static class VisualTreeHelperAddition
	{
		#region Win32

		[DllImport("User32.dll", SetLastError = true)]
		private static extern IntPtr MonitorFromWindow(
			IntPtr hwnd,
			MONITOR_DEFAULTTO dwFlags);

		private enum MONITOR_DEFAULTTO : uint
		{
			MONITOR_DEFAULTTONULL = 0x00000000,
			MONITOR_DEFAULTTOPRIMARY = 0x00000001,
			MONITOR_DEFAULTTONEAREST = 0x00000002,
		}

		[DllImport("Gdi32.dll", SetLastError = true)]
		private static extern int GetDeviceCaps(
			IntPtr hdc,
			int nIndex);

		private const int LOGPIXELSX = 88;
		private const int LOGPIXELSY = 90;

		[DllImport("User32.dll", SetLastError = true)]
		private static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ReleaseDC(
			IntPtr hWnd,
			IntPtr hDC);

		[DllImport("Shcore.dll", SetLastError = true)]
		private static extern int GetDpiForMonitor(
			IntPtr hmonitor,
			MONITOR_DPI_TYPE dpiType,
			ref uint dpiX,
			ref uint dpiY);

		private enum MONITOR_DPI_TYPE
		{
			MDT_Effective_DPI = 0,
			MDT_Angular_DPI = 1,
			MDT_Raw_DPI = 2,
			MDT_Default = MDT_Effective_DPI
		}

		private const int S_OK = 0x00000000;

		#endregion

		#region OS

		private static Lazy<bool> _isOs81OrNewer = new Lazy<bool>(() =>
			new Version(6, 3) <= Environment.OSVersion.Version);

		private static Lazy<bool> _isOs10Redstone1OrNewer = new Lazy<bool>(() =>
			new Version(10, 0, 14393) <= Environment.OSVersion.Version);

		#endregion

		private const double DefaultPixelsPerInch = 96D; // Default pixels per Inch

		/// <summary>
		/// System DPI
		/// </summary>
		public static DpiScale SystemDpi => _systemDpi.Value;
		private static Lazy<DpiScale> _systemDpi = new Lazy<DpiScale>(() => GetSystemDpi());

		private static DpiScale GetSystemDpi()
		{
			var handle = IntPtr.Zero;
			try
			{
				handle = GetDC(IntPtr.Zero);
				if (handle == IntPtr.Zero)
					return new DpiScale(1D, 1D);

				return new DpiScale(
					GetDeviceCaps(handle, LOGPIXELSX) / DefaultPixelsPerInch,
					GetDeviceCaps(handle, LOGPIXELSY) / DefaultPixelsPerInch);
			}
			finally
			{
				if (handle != IntPtr.Zero)
					ReleaseDC(IntPtr.Zero, handle);
			}
		}

		/// <summary>
		/// Gets Per-Monitor DPI of the monitor to which a specified Visual belongs.
		/// </summary>
		/// <param name="visual">Visual</param>
		/// <returns>DPI information</returns>
		public static DpiScale GetDpi(Visual visual)
		{
			if (visual == null)
				throw new ArgumentNullException(nameof(visual));

			if (!_isOs81OrNewer.Value)
				return SystemDpi;

			if (_isOs10Redstone1OrNewer.Value)
				return VisualTreeHelper.GetDpi(visual);

			var source = PresentationSource.FromVisual(visual) as HwndSource;
			if (source == null)
				return SystemDpi;

			var handleMonitor = MonitorFromWindow(
				source.Handle,
				MONITOR_DEFAULTTO.MONITOR_DEFAULTTONEAREST);
			if (handleMonitor == IntPtr.Zero)
				return SystemDpi;

			uint dpiX = 1;
			uint dpiY = 1;

			var result = GetDpiForMonitor(
				handleMonitor,
				MONITOR_DPI_TYPE.MDT_Default,
				ref dpiX,
				ref dpiY);
			if (result != S_OK)
				return SystemDpi;

			return new DpiScale(dpiX / DefaultPixelsPerInch, dpiY / DefaultPixelsPerInch);
		}
	}
}