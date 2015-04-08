using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KuBob {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - 600;
			this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top + 50;

			DispatcherTimer MainTimer = new DispatcherTimer() {
				Interval = TimeSpan.FromMilliseconds(500), IsEnabled = true,
			};
			MainTimer.Tick += MainTimer_Tick;

			this.Activated += (o, e) => {
				grideffectShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.4, TimeSpan.FromMilliseconds(100)));
			};
			this.Deactivated += (o, e) => grideffectShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.1, TimeSpan.FromMilliseconds(100)));
		}

		DateTime NowDate = new DateTime(1970, 1, 1);
		Dictionary<string, string> DictData = new Dictionary<string, string>();
		NotiWindow NWindow;

		// System event (loaded, closing)

		bool isActivated = true;
		DateTime LastDate;

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			LastDate = DateTime.Now;

			NWindow = new NotiWindow(this);
			NWindow.Show();

			SetTrayIcon();

			InitSetting();

			ChangeDate(LastDate);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			ni.Dispose();

			NWindow.IsClose = true;
			NWindow.Close();
		}

		// Change Date

		private void ChangeDate(DateTime date) {
			NowDate = date;
			stackContent.BeginAnimation(StackPanel.OpacityProperty,
				new DoubleAnimation(0, TimeSpan.FromMilliseconds(100)));

			textYear.Text = date.Year.ToString();
			textMonth.Text = string.Format("{0}.", date.Month);
			textDay.Text = date.Day.ToString();
			textWeekDay.Text = date.ToString("ddd", new CultureInfo("en-US")).ToUpper();

			RefreshMeal(string.Format("{0}{1:D2}{2:D2}", date.Year, date.Month, date.Day));
		}

		// Date moving

		private void buttonNextDay_Click(object sender, RoutedEventArgs e) {
			if (NowDate.Year == 1970) { return; }
			ChangeDate(NowDate + TimeSpan.FromDays(1));
		}

		private void buttonYestDay_Click(object sender, RoutedEventArgs e) {
			if (NowDate.Year == 1970) { return; }
			ChangeDate(NowDate - TimeSpan.FromDays(1));
		}

		// System controls


		private void Window_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				isActivated = false;
				new AltTab().HideAltTab(this);
				this.Opacity = 0;
			}
		}

		private void buttonMinimize_Click(object sender, RoutedEventArgs e) {
			isActivated = false;
			new AltTab().HideAltTab(this);
			this.Opacity = 0;
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		// Drag move

		private void gridMain_MouseDown(object sender, MouseButtonEventArgs e) {
			DragMove();
		}

		// Move to today

		private void buttonToday_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (DateTime.Now.Year != NowDate.Year || DateTime.Now.Month != NowDate.Month || DateTime.Now.Day != NowDate.Day) {
				ChangeDate(DateTime.Now);
			}
		}

		string[,] WeekTime = new string[2, 7] {
			{ "7:30", "9:00", "12:00", "14:00", "17:30", "19:30", "7:30" },
			{ "7:30", "9:00", "12:00", "13:30", "17:30", "19:00", "7:30" }
		};
		string[] MealWork = new string[] { "", "아침", "점심", "저녁" };
		public Dictionary<string, bool> DictNoti = new Dictionary<string, bool>();

		private void MainTimer_Tick(object sender, EventArgs e) {
			DateTime now = DateTime.Now;
			int tag = 6, type = 1, isday = 0;
			if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday) {
				isday = 1;
			}

			for (int i = 0; i < 6; i++) {
				if (now < Convert.ToDateTime(string.Format("{0}:00", WeekTime[isday, i]))) {
					tag = i;
					break;
				}
			}

			SolidColorBrush brush = tag % 2 == 0 ? Brushes.ForestGreen : Brushes.Crimson;
			textTime.Foreground = brush;
			type = (tag) / 2 + 1;
			string date = string.Format("{0}{1:D2}{2:D2}{3}", now.Year, now.Month, now.Day, type);

			if ((Convert.ToDateTime(string.Format("{0}:00", WeekTime[isday, tag]))
				- now + TimeSpan.FromDays(tag == 6 ? 1 : 0)) < TimeSpan.FromMinutes(10) &&
				(Convert.ToDateTime(string.Format("{0}:00", WeekTime[isday, tag]))
				- now + TimeSpan.FromDays(tag == 6 ? 1 : 0)) > TimeSpan.FromMinutes(0)) {

				if (tag % 2 == 0 && !DictNoti.ContainsKey(date) && Setting.Notify) {
					DictNoti.Add(date, false);
					if (DictData.ContainsKey(date)) {
						NWindow.Alert(string.Format("{0}식사 시작 10분 전입니다", MealWork[type]), DictData[date], brush, date);
					} else {
						NWindow.Alert(string.Format("{0}식사 시작 10분 전입니다", MealWork[type]), "준비하세요.", brush, date);
					}
				}

				if (tag % 2 == 1) {
					if ((DictNoti.ContainsKey(date) && !DictNoti[date]) || !DictNoti.ContainsKey(date)) {
						if (Setting.Notify) {
							DictNoti[date] = true;
							NWindow.Alert(string.Format("{0}식사 종료 10분 전입니다", MealWork[type]), "10분남음!", brush, date);
						}
					}
				}
			}

			textTime.Text = (Convert.ToDateTime(string.Format("{0}:00", WeekTime[isday, tag]))
				- now + TimeSpan.FromDays(tag == 6 ? 1 : 0))
				.ToString(@"hh\:mm\:ss");


			if (now.Year != LastDate.Year || now.Month != LastDate.Month || now.Day != LastDate.Day) {
				LastDate = now;
				ChangeDate(LastDate);
			}
		}
	}
}
