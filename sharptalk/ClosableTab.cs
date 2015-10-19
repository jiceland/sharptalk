using Gtk;
using System;

namespace sharptalk
{
	[System.ComponentModel.ToolboxItem(true)]
	public class ClosableTab : Gtk.HBox
	{
		public Widget content;
		public ClosableTab (Image icon, String label, Widget content)
		{
			this.content = content;
			RcStyle rcStyle = new RcStyle ();
			rcStyle.Xthickness = 0;
			rcStyle.Ythickness = 0;

			Image tmpimage = new Image(Stock.Close, IconSize.Button);
			Button tabClose = new Button(tmpimage);
			tabClose.TooltipText = "Close tab";
			tabClose.Relief = ReliefStyle.None;
			tabClose.FocusOnClick = false;
			tabClose.ModifyStyle(rcStyle);
			tabClose.Clicked += delegate
			{
				Notebook b = this.Parent as Notebook; 
				//b.CurrentPageWidget.Destroy();
				//b.RemovePage(b.CurrentPage);
				this.content.Destroy();
				Destroy();
			};
 
			Label l = new Label(label);
			this.PackStart(icon, false, false, 0);
			this.PackStart(l, true, true, 0);
			this.PackStart(tabClose, false, false, 0);
			this.ShowAll();
		}
	}
}

