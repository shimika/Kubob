using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace KuBob {
	public partial class MainWindow : Window {

		static string ffSet = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\KuBob.ini";
		private void InitSetting() {
			if (File.Exists(ffSet)) {
				string[] format, line;
				using (StreamReader sr = new StreamReader(ffSet)) {
					format = sr.ReadToEnd().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
				}

				foreach (string f in format) {
					line = f.Split('=');
					switch (line[0]) {
						case "Notify":
							Setting.Notify = Convert.ToBoolean(line[1]);
							break;
						case "TimeVisible":
							Setting.TimeVisible = Convert.ToBoolean(line[1]);
							break;
					}
				}
			} else {
				SaveSetting();
			}

			RefreshBySetting();
		}

		bool isInit = true;
		private void RefreshBySetting() {
			isInit = false;

			checkNoti.IsChecked = Setting.Notify;
			checkTime.IsChecked = Setting.TimeVisible;

			textTime.Visibility = Setting.TimeVisible ? Visibility.Visible : Visibility.Collapsed;
		}

		private void SettingsChanged(object sender, RoutedEventArgs e) {
			if (isInit) { return; }

			Setting.Notify = checkNoti.IsChecked == true ? true : false;
			Setting.TimeVisible = checkTime.IsChecked == true ? true : false;

			textTime.Visibility = Setting.TimeVisible ? Visibility.Visible : Visibility.Collapsed;
			SaveSetting();
		}

		private void SaveSetting() {
			using (StreamWriter sw = new StreamWriter(ffSet)) {
				sw.WriteLine(string.Format("Notify={0}", Setting.Notify));
				sw.WriteLine(string.Format("TimeVisible={0}", Setting.TimeVisible));
			}
		}

		private void buttonMain_Click(object sender, RoutedEventArgs e) { SettingMode(false); }
		private void buttonSetting_Click(object sender, RoutedEventArgs e) { SettingMode(true); }

		private void SettingMode(bool Showing) {
			buttonMain.Visibility = Showing ? Visibility.Visible : Visibility.Collapsed;
			buttonSetting.Visibility = !Showing ? Visibility.Visible : Visibility.Collapsed;

			gridMenu.IsHitTestVisible = !Showing;
			stackSetting.IsHitTestVisible = Showing;

			Storyboard sb = new Storyboard();
			sb.Children.Add(GetDoubleAnimation(gridMenu, Showing ? 0 : 1, 200, Showing ? 0 : 100));
			sb.Children.Add(GetDoubleAnimation(stackContent, Showing ? 0 : 1, 200, Showing ? 0 : 100));
			sb.Children.Add(GetThicknessAnimation(gridMenu, new Thickness(Showing ? -100 : 0, 0, 0, 0), 300, Showing ? 50 : 150));
			sb.Children.Add(GetThicknessAnimation(stackContent, new Thickness(Showing ? -100 : 0, 0, 0, 0), 300, Showing ? 50 : 150));

			sb.Children.Add(GetDoubleAnimation(gridSettingMenu, Showing ? 1 : 0, 400, Showing ? 100 : 0));
			sb.Children.Add(GetDoubleAnimation(stackSetting, Showing ? 1 : 0, 400, Showing ? 100 : 0));
			sb.Children.Add(GetThicknessAnimation(gridSettingMenu, new Thickness(Showing ? 0 : 100, 0, 0, 0), 300, Showing ? 150 : 50));
			sb.Children.Add(GetThicknessAnimation(stackSetting, new Thickness(Showing ? 0 : 100, 0, 0, 0), 300, Showing ? 150 : 50));

			sb.Begin(this);
		}

		private DoubleAnimation GetDoubleAnimation(UIElement uie, double opacity, double time, double delay = 0) {
			DoubleAnimation da = new DoubleAnimation(opacity, TimeSpan.FromMilliseconds(time)) { BeginTime = TimeSpan.FromMilliseconds(delay) };
			Storyboard.SetTarget(da, uie);
			Storyboard.SetTargetProperty(da, new PropertyPath(UIElement.OpacityProperty));
			return da;
		}

		private ThicknessAnimation GetThicknessAnimation(UIElement uie, Thickness margin, double time, double delay = 50) {
			ThicknessAnimation ta = new ThicknessAnimation(margin, TimeSpan.FromMilliseconds(time)) {
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
				BeginTime = TimeSpan.FromMilliseconds(delay)
			};
			Storyboard.SetTarget(ta, uie);
			Storyboard.SetTargetProperty(ta, new PropertyPath(Grid.MarginProperty));

			return ta;
		}
	}

	public class Setting {
		public static bool Notify = true, TimeVisible = true;
	}
}
