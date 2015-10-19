using Gtk;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using System.Text;
using WebKit;

namespace sharptalk
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MessageView : Gtk.VPaned
	{
		TreeStore treestoreMessages;
		ScrolledWindow messagesWindow;
		TreeView treeviewMessages;
		VBox contentBox;
		ToolButton replybtn;
		ToolButton upbtn;
		ToolButton downbtn;
		ScrolledWindow messageWindow;
		WebView messagebrowser;

		// TODO Move to MainWindow for single logon / handling
		List<Forum> forums;

		String messagebox;
		int vpanepos;

		public MessageView (String messagebox)
		{
			this.messagebox = messagebox;

	        messagesWindow = new ScrolledWindow();
    	    messagesWindow.ShadowType = ShadowType.EtchedIn;
        	messagesWindow.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

			treeviewMessages = new TreeView();
			treeviewMessages.BorderWidth = 0;
			treeviewMessages.AppendColumn ("Site", new CellRendererText(), "text", 2).Resizable = true;;
			treeviewMessages.AppendColumn ("User", new CellRendererText(), "text", 3).Resizable = true;;
			treeviewMessages.AppendColumn ("Subject", new CellRendererText(), "text", 4).Resizable = true;;
			treeviewMessages.AppendColumn ("Date", new CellRendererText(), "text", 5).Resizable = true;;
        	treeviewMessages.RowActivated += treeviewMessages_RowActivated;
        	messagesWindow.Add(treeviewMessages);
			this.Add1 (messagesWindow);

			contentBox = new VBox(false, 0);
			Toolbar toolbar = new Toolbar();
			toolbar.HeightRequest = 38;
			toolbar.ToolbarStyle = ToolbarStyle.Icons;
			toolbar.BorderWidth = 0;
			Image tmpimage = new Image();
			tmpimage.Pixbuf = new Gdk.Pixbuf(".images/icon_up.png");
			upbtn = new ToolButton(tmpimage, "");
			upbtn.TooltipText = "Expand view to top";
			upbtn.Clicked += upbtn_Clicked;
			Image tmpimage2 = new Image();
			tmpimage2.Pixbuf = new Gdk.Pixbuf(".images/icon_down.png");
			downbtn = new ToolButton(tmpimage2, "");
			downbtn.TooltipText = "Split view";
			downbtn.Clicked += downbtn_Clicked;
			Image tmpimage3 = new Image();
			tmpimage3.Pixbuf = new Gdk.Pixbuf(".images/reply.png");
			replybtn = new ToolButton(tmpimage3, "");
			replybtn.TooltipText = "Reply to sender";
			SeparatorToolItem sepSpacer = new SeparatorToolItem();
			sepSpacer.Expand = true;
			sepSpacer.Draw = false;

			messageWindow = new ScrolledWindow();
			messagebrowser = new WebView();
			messagebrowser.Editable = false;
			messageWindow.Add(messagebrowser);
			contentBox.PackStart(toolbar, false, false, 0);
			contentBox.PackStart(messageWindow);
			toolbar.Add(replybtn);
			toolbar.Add(sepSpacer);
			toolbar.Add(upbtn);
			toolbar.Add(downbtn);
			this.Add2(contentBox);
			this.ShowAll();
			downbtn.Hide ();
			replybtn.Sensitive = false;

			treestoreMessages = new TreeStore (typeof(string),typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
			forums = new List<Forum> ();
			getBoxMessages(messagebox);
			treestoreMessages.AppendValues("000", "000", "SharpTalk", "Admin", "Welcome to SharpTalk", "");
			treestoreMessages.SetSortColumnId(5, SortType.Descending);
			treeviewMessages.Model = treestoreMessages;
		}

		protected void treeviewMessages_RowActivated (object o, RowActivatedArgs args)
		{
			TreeIter iter;
			treestoreMessages.GetIter (out iter, args.Path);
			String siteId = (String)treestoreMessages.GetValue (iter, 0);
			String messageId = (String)treestoreMessages.GetValue (iter, 1);
			String messageContent = "<HTML><HEAD><STYLE>" +
				".intlink A:link {text-decoration: none; color: black;}" +
				".intlink A:visited {text-decoration: none; color: black;}" +
				".intlink A:active {text-decoration: none; color: grey;}" +
				".intlink A:hover {text-decoration: underline; color: grey;}" +
				"img { display: block; margin-left: auto; margin-right: auto; max-height: 300px; max-width: 500px;}" +
				"div.quote { margin-top: 10px;  margin-left: 30px; padding-left: 15px; border-left: 3px solid #fa0;} " +
				"</STYLE></HEAD><BODY><FONT SIZE=\"2\">";
			// Internal SharpTalk message
			if (siteId.Equals ("000"))
			{
				messageContent += "<CENTER><H2>Welcome to SharpTalk.</H2>Copyright 2013, 2014 Jimmy Cederholm</CENTER><BR>Get Started by adding a site, using the menu item on the left pane.<BR><BR>" +
					"<IMG SRC=\"file://" + System.IO.Path.Combine (global::System.AppDomain.CurrentDomain.BaseDirectory, ".images/addsite.png") + "\">";
			}

			else
			{
				foreach(Forum f in forums)
				{
					if(f.forumUrl.Equals(UserSettings.getValue("Site" + siteId + ".Url")))
					{
						messageContent += f.getMessageContent(messagebox, messageId);
						break;
					}
				}
			}
			messageContent += "</FONT></BODY></HTML>";
			this.messagebrowser.LoadHtmlString(messageContent, "file://"); // TODO file:// only if internal message
		}

		protected void upbtn_Clicked(object o, EventArgs args)
		{
			this.downbtn.Show();
			this.upbtn.Hide();
			vpanepos = this.Position;
			this.Position = 0;
		}

		protected void downbtn_Clicked(object o, EventArgs args)
		{
			this.upbtn.Show();
			this.downbtn.Hide();
			this.Position = vpanepos;
		}

		private void getBoxMessages(String box)
		{
			String forumName;
			int siteNum = 1;

			forums.Clear();
			while ((forumName = UserSettings.getValue("Site" + siteNum.ToString())) != String.Empty)
			{
				String boxId = UserSettings.getValue("Site" + siteNum.ToString() + "." + box);
				String forumUrl = UserSettings.getValue("Site" + siteNum.ToString() + ".Url");

				if(boxId != String.Empty)
				{
					Forum f = new Forum(forumName, forumUrl);

					forums.Add(f);
					String username = UserSettings.getValue("Site" + siteNum.ToString() + ".User");
					String password = UserSettings.getValue("Site" + siteNum.ToString() + ".Pwd");
					f.login(username, password);

					int totalMessages = f.loadMessages(boxId, 0, 49);
					int messagesLoaded;
					if(totalMessages > 50)
						messagesLoaded = 50;
					else
						messagesLoaded = totalMessages;

					for (int i=0; i<messagesLoaded && i<50; i++)
					{
						treestoreMessages.AppendValues(siteNum.ToString (), f.getMessageId(i), f.forumName, f.getMessageUser(i), f.getMessageSubject(i), f.getMessageSentDate(i).ToString("u").Substring(0, 19));
					}
				}
				siteNum++;
			}
		}
	}
}

