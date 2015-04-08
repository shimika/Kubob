using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KuBob {
	/// <summary>
	/// NotiWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class NotiWindow : Window {
		public NotiWindow(MainWindow mainWindow) {
			InitializeComponent();
			mWindow = mainWindow;

			RearrangeWindow();
			DispatcherTimer dtm = new DispatcherTimer() {
				Interval = TimeSpan.FromMilliseconds(500),
				IsEnabled = true,
			};
			dtm.Tick += (o, e) => {
				if (stackNotiPanel.Children.Count == 0) {
					//this.Visibility = Visibility.Collapsed;
				}
				RearrangeWindow();
			};

			this.Loaded += NotiWindow_Loaded;
			this.Closing += (o, e) => {
				if (!IsClose) { e.Cancel = true; }
			};
		}

		MainWindow mWindow = null;
		public bool IsClose = false;

		private void NotiWindow_Loaded(object sender, RoutedEventArgs e) {
			new AltTab().HideAltTab(this);
		}

		private void RearrangeWindow() {
			this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top;
			this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - 350;
			this.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
		}

		public void Alert(string Title, string Script, SolidColorBrush sb, string date, int Timeout = 70) {
			this.Topmost = false; this.Topmost = true;

			Grid gridBackground = new Grid() {
				Margin = new Thickness(160, 10, 10, 10), MinHeight = 80,
				Background = sb,
				ClipToBounds = true,
				Opacity = 0,
			};

			gridBackground.PreviewMouseDown += (o, e) => {
				// Cancel

				if (mWindow.DictNoti.ContainsKey(date)) {
					mWindow.DictNoti[date] = true;
				}
				
				DeleteAlert(o as Grid);
			};

			StackPanel stackContent = new StackPanel() { Margin = new Thickness(11,1,1,1), Background = Brushes.White };
			TextBlock txtTitle = new TextBlock() { Text = Title, Margin = new Thickness(15, 10, 15, 5), FontSize = 20, Foreground = sb };
			TextBlock txtScript = new TextBlock() { Text = Script, Margin = new Thickness(15, 5, 15, 10), FontSize = 13.33, TextWrapping = TextWrapping.Wrap };

			stackContent.Children.Add(txtTitle);
			stackContent.Children.Add(txtScript);

			gridBackground.Children.Add(stackContent);

			stackNotiPanel.Children.Add(gridBackground);
			gridBackground.BeginAnimation(Grid.OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(250)) {
				EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 5 }
			});
			gridBackground.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(160, 10, 10, 10), new Thickness(10), TimeSpan.FromMilliseconds(250)) {
				EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 5 }
			});

			DispatcherTimer dtm = new DispatcherTimer() {
				Interval = TimeSpan.FromSeconds(Timeout),
				IsEnabled = true, Tag = gridBackground,
			};
			dtm.Tick += (o, e) => {
				(o as DispatcherTimer).Stop();
				DeleteAlert((o as DispatcherTimer).Tag as Grid);
				// Timeout
			};
		}

		public int UnreadCount = 0;
		private void DeleteAlert(Grid grid) {
			Storyboard sb = new Storyboard();
			DoubleAnimation da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) {
				EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 5 }
			};
			Storyboard.SetTarget(da, grid);
			Storyboard.SetTargetProperty(da, new PropertyPath(Grid.OpacityProperty));

			sb.Children.Add(da);
			sb.Completed += (o, e) => {
				try {
					stackNotiPanel.Children.Remove(grid);
				} catch { }
			};
			sb.Begin(this);
		}
	}
}
