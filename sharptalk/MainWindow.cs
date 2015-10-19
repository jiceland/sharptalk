using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Text;
using Gtk;
using sharptalk;
using WebKit;

public partial class MainWindow: Gtk.Window
{	
	TreeStore treestoreForums;
	TreeIter favouritesRoot;
	TreeIter mailRoot;

	int searchSiteNum;
	String searchForumId;
	TreeIter searchFoundIter;

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		treeview1.HeadersVisible = false;
		// treeview1.EnableTreeLines = true;
		treeview1.LevelIndentation = 4;
		treeview1.BorderWidth = 0;
		treeview1.AppendColumn ("Icon", new Gtk.CellRendererPixbuf (), "pixbuf", 0);
		treeview1.AppendColumn ("Name", new CellRendererText(), "text", 1);

		CellRendererText cellSiteNum = new CellRendererText();
		cellSiteNum.Visible = false;
		treeview1.AppendColumn ("Site", cellSiteNum, "text", 2);

		CellRendererText cellForumNum = new CellRendererText();
		cellForumNum.Visible = false;
		treeview1.AppendColumn("Forum", cellForumNum, "text", 3);

		notebook2.BorderWidth = 0;
		notebook2.Scrollable = true;

		ShowAll();

		// TODO get / set in UserSettings
		Resize(1000, 600);
        Move(140, 80);
		populateForums();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		// Kills any threads created in ForumViews
		notebook2.Destroy();
		Application.Quit ();
		a.RetVal = true;
	}

	[GLib.ConnectBefore]
	protected void treeview1_ButtonPress (object o, ButtonPressEventArgs args)
	{
	    if((int)args.Event.Button == 3)
    	{       
			Gtk.Menu popup = new Gtk.Menu();
        	Gtk.MenuItem menuitemAddFavourite = new MenuItem("Add To Favourites");
			menuitemAddFavourite.Activated += menuitemAddFavourite_Activated;
        	Gtk.MenuItem menuitemRemoveFavourite = new MenuItem("Remove From Favourites");
			menuitemRemoveFavourite.Activated += menuitemRemoveFavourite_Activated;
        	Gtk.MenuItem menuitemRemoveSite = new MenuItem("Remove Site");
			menuitemRemoveSite.Activated += menuitemRemoveSite_Activated;
        	Gtk.MenuItem menuitemAddSite = new MenuItem("Add Site...");
			menuitemAddSite.Activated += menuitemAddSite_Activated;

			TreePath path;
			treeview1.GetPathAtPos((int)args.Event.X, (int)args.Event.Y, out path);

//			if(selectedIter != null)
//			if(treeview1.Selection.CountSelectedRows() > 0)
			if(path != null)
			{
				bool isFavourite = false;
				TreeIter selectedIter;
				TreeIter parentIter;
				this.treestoreForums.GetIter (out selectedIter, path);
				//treeview1.Model.GetIter(out selectedIter, treeview1.Selection.GetSelectedRows()[0]);
				bool hasParent = treeview1.Model.IterParent(out parentIter, selectedIter);
				bool inMessage = selectedIter.Equals(mailRoot);
				if(hasParent)
					inMessage = parentIter.Equals(mailRoot);

				String siteNum = (String)treestoreForums.GetValue (selectedIter, 2);
				String forumNum = (String)treestoreForums.GetValue (selectedIter, 3);

				if(hasParent && !inMessage)
				{
					if(parentIter.Equals(favouritesRoot))
						isFavourite = true;
				}

				if(!selectedIter.Equals(favouritesRoot) && !isFavourite && !forumNum.Equals(String.Empty) && !inMessage)
					popup.Add(menuitemAddFavourite);

				if(isFavourite)
					popup.Add(menuitemRemoveFavourite);

				if(forumNum.Equals(String.Empty) && !selectedIter.Equals(favouritesRoot) && !inMessage)
			        popup.Add(menuitemRemoveSite);

				if(!selectedIter.Equals(favouritesRoot))
					popup.Add(new SeparatorMenuItem());
			}
	        popup.Add(menuitemAddSite);
	        popup.ShowAll();
    	    popup.Popup();
	    }
	}

	protected void menuitemAddSite_Activated (object sender, EventArgs args)
	{
		ForumAdd dlg = new ForumAdd ();
		ResponseType response;
		bool forumOk = false;
		do
		{
			response = (ResponseType)dlg.Run ();
			if (response == ResponseType.Ok)
			{
				this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
				while (Gtk.Application.EventsPending ())
    		    	Gtk.Application.RunIteration ();

				if(!dlg.ForumUrl.StartsWith("http://"))
					forumOk = forumAdd(dlg.ForumName, "http://" + dlg.ForumUrl, dlg.ForumUsername, dlg.ForumPassword);
				else
					forumOk = forumAdd(dlg.ForumName, dlg.ForumUrl, dlg.ForumUsername, dlg.ForumPassword);

				this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Arrow);
			}
		} while(response == ResponseType.Ok && !forumOk);
		dlg.Destroy();
    }

	protected void menuitemRemoveSite_Activated (object sender, EventArgs args)
	{
		TreeIter selectedIter;
		treeview1.Model.GetIter(out selectedIter, treeview1.Selection.GetSelectedRows()[0]);
		String forumName = (String)treestoreForums.GetValue (selectedIter, 1);
		String selSite = (String)treestoreForums.GetValue (selectedIter, 2);

		MessageDialog md = new MessageDialog(this,  DialogFlags.DestroyWithParent, MessageType.Question, 
         ButtonsType.OkCancel, "Remove site \"" + forumName + "\" and all Favourites linked to this site?");
		ResponseType result = (ResponseType)md.Run ();

		if (result == ResponseType.Ok)
		{
			UserSettings.clearValue("Site", int.Parse(selSite));
			// Remove references from Favourites
			int favNum = 1;
			while (UserSettings.getValue("Favourite" + favNum.ToString()) != String.Empty)
			{
				String favSite =  UserSettings.getValue ("Favourite" + favNum.ToString () + ".Site");
				if(favSite.Equals(selSite))
					UserSettings.clearValue("Favourite", favNum);
				else
					favNum++;
			}
		}
		md.Destroy();
		populateForums();
	}

	protected void menuitemAddFavourite_Activated (object sender, EventArgs args)
	{
		TreeIter selIter;
		treeview1.Model.GetIter(out selIter, treeview1.Selection.GetSelectedRows()[0]);
		String siteNum = (String)treestoreForums.GetValue (selIter, 2);
		String forumNum = (String)treestoreForums.GetValue (selIter, 3);
		String forumName = (String)treestoreForums.GetValue (selIter, 1);
		String siteName = UserSettings.getValue("Site" + siteNum);

		int favouriteNum = UserSettings.getNextSettingNum("Favourite");
		UserSettings.setValue("Favourite" + favouriteNum.ToString(), forumName + " in " +siteName);
		UserSettings.setValue("Favourite" + favouriteNum.ToString() + ".Site", siteNum);
		UserSettings.setValue("Favourite" + favouriteNum.ToString() + ".Forum", forumNum);

		populateForums();
	}

	protected void menuitemRemoveFavourite_Activated (object sender, EventArgs args)
	{
		TreeIter selectedIter;
		treeview1.Model.GetIter (out selectedIter, treeview1.Selection.GetSelectedRows () [0]);
		String forumName = (String)treestoreForums.GetValue (selectedIter, 1);
		String selSite = (String)treestoreForums.GetValue (selectedIter, 2);
		String selForum = (String)treestoreForums.GetValue (selectedIter, 3);

		MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Question, 
         ButtonsType.OkCancel, "Remove \"" + forumName + "\" from favouries?");

		ResponseType result = (ResponseType)md.Run ();

		if (result == ResponseType.Ok)
		{
			int favNum = 1;
			while (UserSettings.getValue("Favourite" + favNum.ToString()) != String.Empty)
			{
				String site =  UserSettings.getValue ("Favourite" + favNum.ToString () + ".Site");
				String forum =  UserSettings.getValue ("Favourite" + favNum.ToString () + ".Forum");

				if(site.Equals(selSite) && forum.Equals(selForum))
				{
					UserSettings.clearValue("Favourite", favNum);
					break;
				}
				favNum++;
			}
		}
		md.Destroy();
		populateForums();
	}

	private bool forumAdd (String name, String url, String username, String password)
	{
		Forum f = new Forum (name, url);

		if (!f.forumValid) {
			MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Info, 
    	     ButtonsType.Ok, "No valid forum found at specified URL.");
			md.Run ();
			md.Destroy ();
			return false;
		}

		if (username != string.Empty)
		{
			if(!f.login (username, password))
			{
				MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Info, 
    		     ButtonsType.Ok, "Forum login failed - invalid username or password.");
				md.Run ();
				md.Destroy ();
				return false;
			}
		}
		f.loadForums();
		populateForums();
		return true;
	}

	private void populateForums ()
	{
		treestoreForums = new TreeStore (typeof(Gdk.Pixbuf), typeof(string), typeof(string), typeof(string));
		mailRoot = treestoreForums.AppendValues (new Gdk.Pixbuf (".images/icon_mail.png"), "Messages", "", "");
		TreeIter inboxRoot = treestoreForums.AppendValues (mailRoot, new Gdk.Pixbuf (".images/mailbox.png"), "Inbox", "MAILBOX", "Inbox");
		TreeIter outboxRoot = treestoreForums.AppendValues (mailRoot, new Gdk.Pixbuf (".images/mailbox.png"), "Sent", "MAILBOX", "Outbox");
		favouritesRoot = treestoreForums.AppendValues (new Gdk.Pixbuf (".images/icon_favourites16.png"), "Favourites", "", "");
		TreeIter iter;

		//TreeIter forumsRoot = treestoreForums.AppendValues (new Gdk.Pixbuf(".images/icon_contents16.png"), "All Forums", "", "");

		String siteName;
		int siteNum = 1;

		while ((siteName = UserSettings.getValue("Site" + siteNum.ToString())) != String.Empty)
		{
			TreeIter siteRoot = treestoreForums.AppendValues (new Gdk.Pixbuf (".images/icon_book16.png"), siteName, siteNum.ToString (), "");

			String forumName;
			int forumNum = 1;

			while ((forumName = UserSettings.getValue("Site" + siteNum.ToString() + ".Forum" + forumNum.ToString())) != String.Empty)
			{
				String id = UserSettings.getValue ("Site" + siteNum.ToString () + ".Forum" + forumNum.ToString () + ".Id");
				String parent = UserSettings.getValue ("Site" + siteNum.ToString () + ".Forum" + forumNum.ToString () + ".Parent");
				String hasFavourites = UserSettings.getValue ("Site" + siteNum.ToString () + ".Forum" + id + ".Favourites");
				String iconFile;

				if(hasFavourites.Length > 0)
					iconFile = ".images/icon_page_star.png";
				else
					iconFile = ".images/icon_page.png";

				// TODO store id in tree, only 1 level now!
				if (parent.Equals ("-1") || parent.Equals ("0"))
					iter = treestoreForums.AppendValues (siteRoot, new Gdk.Pixbuf (iconFile), forumName, siteNum.ToString (), forumNum.ToString ());
				else
				{
					searchSiteNum = siteNum;
					searchForumId = parent;
					treestoreForums.Foreach(new TreeModelForeachFunc(findParent));
					treestoreForums.AppendValues (searchFoundIter, new Gdk.Pixbuf (iconFile), forumName, siteNum.ToString (), forumNum.ToString ());
				}

				forumNum++;
			}
			siteNum++;
		}

		siteNum = 1;
		while ((siteName = UserSettings.getValue("Favourite" + siteNum.ToString())) != String.Empty)
		{
			String site =  UserSettings.getValue ("Favourite" + siteNum.ToString () + ".Site");
			String forum =  UserSettings.getValue ("Favourite" + siteNum.ToString () + ".Forum");
			String id = UserSettings.getValue ("Site" + site + ".Forum" + forum + ".Id");

			String hasFavourites = UserSettings.getValue ("Site" + site + ".Forum" + id + ".Favourites");
			String iconFile;

			if(hasFavourites.Length > 0)
				iconFile = ".images/icon_page_star.png";
			else
				iconFile = ".images/icon_page.png";

			iter = treestoreForums.AppendValues (favouritesRoot, new Gdk.Pixbuf (iconFile), siteName, site, forum);
			siteNum++;
		}

		treeview1.Model = treestoreForums;
		treeview1.ExpandRow(treestoreForums.GetPath(mailRoot), true);
		//treeview1.ExpandRow(treestoreForums.GetPath(favouritesRoot), false);
		treeview1.Selection.SelectIter(inboxRoot);
		treeview1.ActivateRow(treestoreForums.GetPath(inboxRoot), treeview1.Columns[0]);
	}

	protected void treeview1_RowActivated (object o, RowActivatedArgs args)
	{
		TreeIter iter;
		treestoreForums.GetIter (out iter, args.Path);
		String siteNum = (String)treestoreForums.GetValue (iter, 2);
		String forumNum = (String)treestoreForums.GetValue (iter, 3);


		if (String.IsNullOrEmpty (siteNum) || String.IsNullOrEmpty (forumNum))
			return;

		if (siteNum.Equals ("MAILBOX"))
		{
			launchMessageView(forumNum);
		} 
		else
		{
			launchForumView (siteNum, forumNum);
		}
	}

	private void launchForumView (String siteNum, String forumNum)
	{
		String id = UserSettings.getValue ("Site" + siteNum + ".Forum" + forumNum + ".Id");
		String hasFavourites = UserSettings.getValue ("Site" + siteNum + ".Forum" + id + ".Favourites");
		String iconFile;

		if(hasFavourites.Length > 0)
			iconFile = ".images/icon_page_star.png";
		else
			iconFile = ".images/icon_page.png";

		Image i = new Image(new Gdk.Pixbuf (iconFile));
		String siteUrl = UserSettings.getValue ("Site" + siteNum + ".Url");
		String activeForum = UserSettings.getValue ("Site" + siteNum + ".Forum" + forumNum + ".Id");

		if (UserSettings.getValue ("Site" + siteNum + ".Forum" + forumNum + ".IsFolder").Equals ("TRUE"))
			return;

		this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
		while (Gtk.Application.EventsPending ())
        	Gtk.Application.RunIteration ();

		Forum activeSite = new Forum (siteNum, siteUrl);

		String userName = UserSettings.getValue("Site" + siteNum + ".User");
		String password = UserSettings.getValue("Site" + siteNum + ".Pwd");

		if(userName != String.Empty)
			activeSite.login(userName, password);

		ForumView view = new ForumView(activeSite, activeForum);
		String tabTitle = UserSettings.getValue("Site" + siteNum) + ": " + UserSettings.getValue("Site" + siteNum + ".Forum" + forumNum);
		int newPage = notebook2.AppendPage(view, new ClosableTab(i, tabTitle, view));
		notebook2.CurrentPage = newPage;
		int splitPos = (this.Allocation.Height / 2) - 30;
		if(splitPos > 0)
			view.Position = splitPos;

		this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Arrow);
	}

	private void launchMessageView (String mailBox)
	{
		this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
		while (Gtk.Application.EventsPending ())
        	Gtk.Application.RunIteration ();

		Image i = new Image(new Gdk.Pixbuf (".images/mailbox.png"));
		MessageView view = new MessageView (mailBox);
		String tabTitle = mailBox;
		int newPage = notebook2.AppendPage (view, new ClosableTab (i, tabTitle, view));
		notebook2.CurrentPage = newPage;
		int splitPos = (this.Allocation.Height / 2) - 30;
		if (splitPos > 0)
			view.Position = splitPos;

		this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Arrow);
	}

	private bool findParent(Gtk.TreeModel model, Gtk.TreePath path, Gtk.TreeIter iter)
	{
		String site = (String)model.GetValue (iter, 2);
		String forum = (String)model.GetValue (iter, 3);
		
		if (site.Equals (searchSiteNum.ToString())) {
			String iterParentId = UserSettings.getValue ("Site" + site + ".Forum" + forum + ".Id");
			if (iterParentId.Equals (searchForumId))
			{
				searchFoundIter = iter;
				return true;
			}
		}
		return false;
	}
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                      