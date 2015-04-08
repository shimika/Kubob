using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KuBob {
	public partial class MainWindow : Window {
		System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
		private void SetTrayIcon() {
			var iconHandle = KuBob.Properties.Resources.Trash_Empty.Handle;
			ni.Icon = System.Drawing.Icon.FromHandle(iconHandle);
			ni.Visible = true; ni.Text = "Kubob";

			ni.MouseDoubleClick += delegate(object senderX, System.Windows.Forms.MouseEventArgs eX) {
				if (eX.Button == System.Windows.Forms.MouseButtons.Left) {
					this.WindowState = WindowState.Normal;
					isActivated = true;
					new AltTab().ShowAltTab(this);
					this.Opacity = 1;
					this.Activate();
				}
			};

			System.Windows.Forms.ContextMenuStrip ctxt = new System.Windows.Forms.ContextMenuStrip();
			System.Windows.Forms.ToolStripMenuItem copen = new System.Windows.Forms.ToolStripMenuItem("열기");
			System.Windows.Forms.ToolStripMenuItem cshutdown = new System.Windows.Forms.ToolStripMenuItem("종료");

			copen.Click += delegate(object sender, EventArgs e) {
				this.Opacity = 0;
				this.Opacity = 1;

				isActivated = true;
				new AltTab().ShowAltTab(this);
				this.Activate(); 
			};
			cshutdown.Click += delegate(object sender, EventArgs e) { NWindow.IsClose = true; this.Close(); };

			ctxt.Items.Add(copen);
			ctxt.Items.Add(cshutdown);
			ni.ContextMenuStrip = ctxt;
		}
	}
}
