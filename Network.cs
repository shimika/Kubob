using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace KuBob {
	public partial class MainWindow : Window {
		public static Task<string> GetHTML(string url, string encoding) {
			return Task.Run(() => {
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new UriBuilder(url).Uri);
				httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
				httpWebRequest.Method = "POST";
				httpWebRequest.Referer = "http://www.google.com/";
				httpWebRequest.UserAgent =
					"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
					"Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
					".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
					"InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";
				httpWebRequest.ContentLength = 0;

				string rtHTML = "";


				try {
					httpWebRequest.Proxy = null;

					HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
					StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding(encoding));
					rtHTML = streamReader.ReadToEnd();
				} catch (Exception ex) {
					//MessageBox.Show(ex.Message);
				}

				return rtHTML;
			});
		}

		private async void RefreshMeal(string date) {
			string[] menu = new string[4];

			try {
				for (int i = 1; i <= 3; i++) {
					menu[i] = "";

					string url = string.Format(
						"http://emenu.ourhome.co.kr/meal/list.action?tempcd=8c2ea4cdbe74d231b29b4c69f4c90328&offerdt={0}&up_yn=&up_busiplcd=8c2ea4cdbe74d231b29b4c69f4c90328&busiord=&mealclass={1}"
						, date, i);

					Task<string> httpTask = GetHTML(url, "UTF-8");
					string strHTML = await httpTask;

					int lastIndex = 0, endIndex = 0, openIndex = 0, closeIndex = 0;

					for (int j = 0; j < 2; j++) {
						lastIndex = strHTML.IndexOf("메뉴정보박스시작", lastIndex);
						if (lastIndex < 0) { break; }
						endIndex = strHTML.IndexOf("메뉴정보박스끝", lastIndex);

						if (j == 1) { menu[i] += "\n"; }

						openIndex = lastIndex;
						for (; ; ) {
							openIndex = strHTML.IndexOf("<span class=\"MAR5\">", openIndex);
							if (openIndex < 0 || openIndex > endIndex) { break; }
							closeIndex = strHTML.IndexOf("</td>", openIndex);

							menu[i] += strHTML.Substring(openIndex, closeIndex + 5 - openIndex)
								.Replace("<span class=\"MAR5\">·</span>", "")
								.Replace("</td>", "") + " ";

							openIndex++;
						}

						lastIndex++;
					}

					try {
						if (DictData.ContainsKey(string.Format("{0}{1}", date, i))) {
							DictData[string.Format("{0}{1}", date, i)] = menu[i];
						} else {
							DictData.Add(string.Format("{0}{1}", date, i), menu[i]);
						}
					} catch { }
				}

				await this.Dispatcher.BeginInvoke(new Action(() => {
					for (int i = 1; i <= 3; i++) {
						if (menu[i] == "") {
							(FindName(string.Format("stackMeal{0}", i)) as StackPanel).Visibility = Visibility.Collapsed;
						} else {
							(FindName(string.Format("stackMeal{0}", i)) as StackPanel).Visibility = Visibility.Visible;
							(FindName(string.Format("textMeal{0}", i)) as TextBlock).Text = menu[i];
							(FindName(string.Format("textMeal{0}", i)) as TextBlock).ToolTip = menu[i];
						}
					}

					stackContent.BeginAnimation(StackPanel.OpacityProperty,
						new DoubleAnimation(1, TimeSpan.FromMilliseconds(300)));
				}));
			} catch (Exception ex) {
				//MessageBox.Show(ex.Message);
			}
		}
	}
}
