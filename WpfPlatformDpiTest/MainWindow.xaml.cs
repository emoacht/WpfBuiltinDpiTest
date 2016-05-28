using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfPlatformDpiTest
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			this.DpiChanged += OnWindowDpiChanged;
			this.TestImage.DpiChanged += OnImageDpiChanged;
		}

		#region Property

		private static readonly DpiScale defaultDpi = new DpiScale(1D, 1D); // 96DPI

		public DpiScale CurrentDpi
		{
			get { return (DpiScale)GetValue(CurrentDpiProperty); }
			set { SetValue(CurrentDpiProperty, value); }
		}
		public static readonly DependencyProperty CurrentDpiProperty =
			DependencyProperty.Register(
				nameof(CurrentDpi),
				typeof(DpiScale),
				typeof(MainWindow),
				new PropertyMetadata(
					defaultDpi,
					(d, e) =>
					{
						var newDpi = (DpiScale)e.NewValue;
						Debug.WriteLine($"CurrentDpi -> {newDpi.PixelsPerInchX}-{newDpi.PixelsPerInchY}");
					}));

		public bool SetHandledTrue
		{
			get { return (bool)GetValue(SetHandledTrueProperty); }
			set { SetValue(SetHandledTrueProperty, value); }
		}
		public static readonly DependencyProperty SetHandledTrueProperty =
			DependencyProperty.Register(
				nameof(SetHandledTrue),
				typeof(bool),
				typeof(MainWindow),
				new PropertyMetadata(false));

		public bool SetRootDpi
		{
			get { return (bool)GetValue(SetRootDpiProperty); }
			set { SetValue(SetRootDpiProperty, value); }
		}
		public static readonly DependencyProperty SetRootDpiProperty =
			DependencyProperty.Register(
				nameof(SetRootDpi),
				typeof(bool),
				typeof(MainWindow),
				new PropertyMetadata(false));

		public bool CallBaseOnDpiChanged
		{
			get { return (bool)GetValue(CallBaseOnDpiChangedProperty); }
			set { SetValue(CallBaseOnDpiChangedProperty, value); }
		}
		public static readonly DependencyProperty CallBaseOnDpiChangedProperty =
			DependencyProperty.Register(
				nameof(CallBaseOnDpiChanged),
				typeof(bool),
				typeof(MainWindow),
				new PropertyMetadata(true));

		public ObservableCollection<string> Status { get; } = new ObservableCollection<string>();

		#endregion

		private HwndSource _source;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			CurrentDpi = VisualTreeHelper.GetDpi(this);
			Status.Add($"Initial -> {CurrentDpi.PixelsPerInchX}-{CurrentDpi.PixelsPerInchY}");

			_source = HwndSource.FromHwnd((new WindowInteropHelper(this)).Handle);
			_source?.AddHook(WndProc);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			_source?.RemoveHook(WndProc);
		}

		private const int WM_DPICHANGED = 0x02E0;

		private static ushort GetLoWord(uint dword) => (ushort)(dword & 0xffff);
		private static ushort GetHiWord(uint dword) => (ushort)(dword >> 16);

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_DPICHANGED)
			{
				var x = GetLoWord((uint)wParam);
				var y = GetHiWord((uint)wParam);

				Status.Add($"WM_DPICHANGED -> {x}-{y}");

				CurrentDpi = new DpiScale(x / 96D, y / 96D);

				if (SetHandledTrue)
					handled = true;

				if (SetRootDpi)
					VisualTreeHelper.SetRootDpi(this, CurrentDpi);
			}

			return IntPtr.Zero;
		}

		protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
		{
			Status.Add($"Window OnDpiChanged method BEFORE -> {newDpi.PixelsPerInchX}-{newDpi.PixelsPerInchY}");

			if (CallBaseOnDpiChanged)
				base.OnDpiChanged(oldDpi, newDpi);

			Status.Add($"Window OnDpiChanged method AFTER -> {newDpi.PixelsPerInchX}-{newDpi.PixelsPerInchY}");
		}

		private void OnWindowDpiChanged(object sender, DpiChangedEventArgs e)
		{
			Status.Add($"Window DpiChanged event -> {e.NewDpi.PixelsPerInchX}-{e.NewDpi.PixelsPerInchY}");
		}

		private void OnImageDpiChanged(object sender, DpiChangedEventArgs e)
		{
			Status.Add($"Image DpiChanged event -> {e.NewDpi.PixelsPerInchX}-{e.NewDpi.PixelsPerInchY}");
		}

		private void ButtonClear_Click(object sender, RoutedEventArgs e)
		{
			Status.Clear();
		}
	}
}