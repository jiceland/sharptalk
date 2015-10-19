using Gtk;
using Cairo;
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
	public class ForumView : Gtk.VPaned
	{
		TreeStore treestoreTopics;
		ScrolledWindow topicWindow;
		TreeView treeviewTopics;
		VBox contentBox;
		ScrolledWindow threadwindow;
		WebView threadbrowser;
		ListStore iconStore;
		ScrolledWindow iconwindow;
		EventBox imagewindow;
		IconView iconview;
		ToolButton firstbtn;
		ToolButton prevbtn;
		ToolButton nextbtn;
		ToolButton lastbtn;
		ToggleToolButton imageviewbtn;
		ToggleToolButton textviewbtn;
		ToolButton closebtn;
		ToolButton rotatebtn;
		ToolButton savebtn;
		ToolButton upbtn;
		ToolButton downbtn;
		SeparatorToolItem sepFullsize;
		Image fullSizeImage;
		Layout fullsizeLayout;
		TreeIter fullSizeIter;
		Label pageLabel;
		ToggleToolButton imageSortAscending;
		ToggleToolButton imageSortDescending;
		ProgressBar imageLoadingProgress;

		Forum site;
		String forum;
		String[] favThreads;
		String currTopicId;
		Thread iconThread = null;
		int topicsLoaded = 0;
		bool showFavourites = false;
		bool threadViewLoaded;
		bool iconViewLoaded;
		bool fullSizeMode = false;
		int vpanepos;

		const int postsPerPage = 20;
		int numThreadPages;
		int currThreadPage;
		String iconPageLabel;
		String threadPageLabel;
		int foundImages;
		TreeIter selectedIconIter;
		String readImageAuthor = "";
		int gotoAnchorNum = -1;
		string saveFolder = string.Empty;

		uint oldWidth;
		uint oldHeight;

		public ForumView (Forum site, String forum)
		{
			this.site = site;
			this.forum = forum;
			this.Destroyed += delegate(object sender, EventArgs e) {
				if (iconThread != null)
				if (iconThread.IsAlive)
					iconThread.Abort ();			
			};
			topicWindow = new ScrolledWindow ();
			topicWindow.ShadowType = ShadowType.EtchedIn;
			topicWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);

			treeviewTopics = new TreeView ();
			treeviewTopics.BorderWidth = 0;

			treeviewTopics.AppendColumn ("", new CellRendererPixbuf (), "pixbuf", 5);
			CellRendererText cellTopic = new CellRendererText ();
			TreeViewColumn topicCol = treeviewTopics.AppendColumn ("Topic", cellTopic, "text", 1);
			topicCol.SetCellDataFunc (cellTopic, new Gtk.TreeCellDataFunc (renderTopic));
			topicCol.Resizable = true;
			treeviewTopics.AppendColumn ("Author", new CellRendererText (), "text", 2).Resizable = true;
			;
			treeviewTopics.AppendColumn ("Replies", new CellRendererText (), "text", 3).Resizable = true;
			;
			treeviewTopics.AppendColumn ("Last Reply", new CellRendererText (), "text", 4).Resizable = true;
			;
			// Use treeView.Selection.Changed?
			treeviewTopics.RowActivated += treeviewTopics_RowActivated;
			treeviewTopics.ButtonPressEvent += treeviewTopics_ButtonPress;

			topicWindow.Add (treeviewTopics);
			this.Add1 (topicWindow);

			contentBox = new VBox (false, 0);
			Toolbar toolbar = new Toolbar ();
			toolbar.HeightRequest = 38;
			toolbar.ToolbarStyle = ToolbarStyle.Icons;
			toolbar.BorderWidth = 0;
			firstbtn = new ToolButton (Stock.GotoFirst);
			firstbtn.TooltipText = "First page";
			firstbtn.Sensitive = false;
			firstbtn.Clicked += firstbtn_Clicked;
			prevbtn = new ToolButton (Stock.GoBack);
			prevbtn.TooltipText = "Previous page";
			prevbtn.Sensitive = false;
			prevbtn.Clicked += prevbtn_Clicked;
			nextbtn = new ToolButton (Stock.GoForward);
			nextbtn.TooltipText = "Next page";
			nextbtn.Sensitive = false;
			nextbtn.Clicked += nextbtn_Clicked;
			lastbtn = new ToolButton (Stock.GotoLast);
			lastbtn.TooltipText = "Last page";
			lastbtn.Sensitive = false;
			lastbtn.Clicked += lastbtn_Clicked;

			Image tmpimage = new Image ();
			tmpimage.Pixbuf = new Gdk.Pixbuf (".images/icon_up.png");
			upbtn = new ToolButton (tmpimage, "");
			upbtn.TooltipText = "Expand view to top";
			upbtn.Clicked += upbtn_Clicked;
			Image tmpimage2 = new Image ();
			tmpimage2.Pixbuf = new Gdk.Pixbuf (".images/icon_down.png");
			downbtn = new ToolButton (tmpimage2, "");
			downbtn.TooltipText = "Split view";
			downbtn.Clicked += downbtn_Clicked;
			closebtn = new ToolButton (Stock.Close);
			closebtn.TooltipText = "Close image view";
			closebtn.Clicked += closebtn_Clicked;
			Image tmpimage3 = new Image ();
			tmpimage3.Pixbuf = new Gdk.Pixbuf (".images/rotate_16.png");
			rotatebtn = new ToolButton (tmpimage3, "");
			rotatebtn.TooltipText = "Rotate image clockwise";
			rotatebtn.Clicked += rotatebtn_Clicked;
			Image tmpimage4 = new Image ();
			tmpimage4.Pixbuf = new Gdk.Pixbuf (".images/icon_save.png");
			savebtn = new ToolButton (tmpimage4, "");
			savebtn.TooltipText = "Download image";
			savebtn.Clicked += savebtn_Clicked;
			pageLabel = new Label ("");
			ToolItem textItem = new ToolItem ();
			textItem.Expand = false;
			textItem.Add (pageLabel);
			imageviewbtn = new ToggleToolButton (Stock.ZoomFit);
			imageviewbtn.TooltipText = "Image view";
			imageviewbtn.Toggled += imageviewbtn_Toggled;
			textviewbtn = new ToggleToolButton (Stock.Properties);
			textviewbtn.TooltipText = "Thread view";
			textviewbtn.Active = true;
			textviewbtn.Toggled += textviewbtn_Toggled;
			SeparatorToolItem sepSpacer = new SeparatorToolItem ();
			sepSpacer.Expand = true;
			sepSpacer.Draw = false;
			sepFullsize = new SeparatorToolItem ();
			imageSortAscending = new ToggleToolButton (Stock.SortAscending);
			imageSortAscending.TooltipText = "Show earliest images first";
			imageSortAscending.Active = true;
			imageSortAscending.Toggled += imageSortAscending_Toggled;
			imageSortDescending = new ToggleToolButton (Stock.SortDescending);
			imageSortDescending.TooltipText = "Show latest images first";
			imageSortDescending.Toggled += imageSortDescending_Toggled;
			imageLoadingProgress = new ProgressBar ();
			ToolItem progressItem = new ToolItem ();
			progressItem.Expand = false;
			progressItem.Add (imageLoadingProgress);
			imageLoadingProgress.Fraction = 0;
			toolbar.Add (imageSortAscending);
			toolbar.Add (imageSortDescending);
			toolbar.Add (progressItem);
			toolbar.Add (firstbtn);
			toolbar.Add (prevbtn);
			toolbar.Add (textItem);
			toolbar.Add (nextbtn);
			toolbar.Add (lastbtn);
			toolbar.Add (sepFullsize);
			toolbar.Add (savebtn);
			toolbar.Add (rotatebtn);
			toolbar.Add (sepSpacer);
			toolbar.Add (imageviewbtn);
			toolbar.Add (textviewbtn);
			toolbar.Add (closebtn);
			toolbar.Add (upbtn);
			toolbar.Add (downbtn);
			threadwindow = new ScrolledWindow ();
			threadbrowser = new WebView ();
			threadbrowser.Editable = false;
			threadbrowser.NavigationRequested += threadbrowser_NavigationRequested;
			threadwindow.Add (threadbrowser);

			iconStore = new ListStore (typeof(string), typeof(Gdk.Pixbuf), typeof(string), typeof(string), typeof(int), typeof(int));
			iconview = new IconView (iconStore);
			iconview.Margin = 1;
			iconview.Spacing = 1;
			iconview.BorderWidth = 0;
			iconview.ColumnSpacing = 1;
			iconview.RowSpacing = 1;
			iconview.PixbufColumn = 1;
			iconview.TooltipColumn = 2;
			iconview.SelectionMode = SelectionMode.Multiple;
			iconview.ItemActivated += iconview_ItemActivated;
			iconview.ButtonPressEvent += iconView_ButtonPress;
			iconview.Model = iconStore;
			iconview.ModifyBase (StateType.Normal, new Gdk.Color (0x66, 0x66, 0x66));
			iconwindow = new ScrolledWindow ();
			iconwindow.ShadowType = ShadowType.EtchedIn;
			iconwindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			iconwindow.Add (iconview);
		
			imagewindow = new EventBox ();
			fullSizeImage = new Image ();
			// JICE TEST
			fullsizeLayout = new Layout (null, null);
			fullsizeLayout.Add (fullSizeImage);
			fullsizeLayout.SizeRequested += fullsizeLayout_SizeRequested;
			imagewindow.ModifyBase (StateType.Normal, new Gdk.Color (0x66, 0x66, 0x66));
			imagewindow.Add (fullsizeLayout);
			imagewindow.CanFocus = true;
			imagewindow.KeyPressEvent += imagewindow_keyPressEvent;
			imagewindow.SizeRequested += imagewindow_sizeAllocated;

//			imagewindow.Add(fullSizeImage);

			contentBox.PackStart (toolbar, false, false, 0);
			contentBox.PackStart (iconwindow);
			contentBox.PackStart (imagewindow);
			contentBox.PackStart (threadwindow);
			this.Add2 (contentBox);
			this.ShowAll ();
			imageSortAscending.Hide ();
			imageSortDescending.Hide ();
			iconwindow.Hide ();
			imagewindow.Hide ();
			closebtn.Hide ();
			rotatebtn.Hide ();
			savebtn.Hide ();
			sepFullsize.Hide ();
			downbtn.Hide ();
			imageLoadingProgress.Hide ();
		
			String favouriteThreads = UserSettings.getValue ("Site" + site.forumName + ".Forum" + forum + ".Favourites");
			favThreads = favouriteThreads.Split (';');

			treestoreTopics = new TreeStore (typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(Gdk.Pixbuf));

			topicsLoaded = 0;
			loadTopics ();
			treeviewTopics.Model = treestoreTopics;
		}

		[GLib.ConnectBefore]
		protected void imagewindow_keyPressEvent (object o, KeyPressEventArgs args)
		{
			switch((Gdk.Key)args.Event.KeyValue)
			{
				case Gdk.Key.Left:
					fullsizePrev ();
					args.RetVal = true;
					break;
				case Gdk.Key.Right:
					fullsizeNext();
					args.RetVal = true;
					break;

				case Gdk.Key.Escape:
					fullsizeClose();
					args.RetVal = true;
					break;

				default:
					break;
			}
//			fullSizeImage.GrabFocus();
		}

		protected void imagewindow_sizeAllocated (object o, EventArgs args)
		{
			int i = imagewindow.WidthRequest;

			if (i > 0)
			{
				String fileName = (String)iconStore.GetValue (fullSizeIter, 0);
				Image tmpimage = new Image ();
				tmpimage.Pixbuf = new Gdk.Pixbuf (fileName);

				// Set new thumbnail to icon
				Image thumbnailImage = new Image ();
				createThumbnail (tmpimage, ref thumbnailImage);
				iconStore.SetValue (fullSizeIter, 1, thumbnailImage.Pixbuf);

				// TODO refactor to own resize method
				Gdk.Rectangle r = contentBox.Allocation;
				double orgWidth = tmpimage.Pixbuf.Width;
				double orgHeight = tmpimage.Pixbuf.Height;
				if (orgWidth > (r.Width - 4) || orgHeight > (r.Height - 44))
				{
					double ratio = Math.Min ((r.Width - 4) / orgWidth, (r.Height - 44) / orgHeight);
					int width = (int)(ratio * (double)tmpimage.Pixbuf.Width);
					int height = (int)(ratio * (double)tmpimage.Pixbuf.Height);

					fullSizeImage.Pixbuf = tmpimage.Pixbuf.ScaleSimple (width, height, Gdk.InterpType.Nearest);
					fullsizeLayout.Move (fullSizeImage, (int)((r.Width - width) / 2), (int)((r.Height - height) / 2));
				} 
				else
				{
					fullSizeImage.Pixbuf = tmpimage.Pixbuf;
					fullsizeLayout.Move (fullSizeImage, (int)((r.Width - orgWidth) / 2), (int)((r.Height - orgHeight) / 2));
				}
			}
		}

		protected void treeviewTopics_RowActivated (object o, RowActivatedArgs args)
		{
			TreeIter iter;
			treestoreTopics.GetIter (out iter, args.Path);
			String selTopicId = (String)treestoreTopics.GetValue (iter, 0);
			if (selTopicId.Equals ("000"))
			{
				treestoreTopics.Remove(ref iter);
				this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
			
				loadTopics();
				this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.LeftPtr);
			} 
			else 
			{
				currTopicId = (String)treestoreTopics.GetValue (iter, 0);
				if(currTopicId != null)
				{
					currThreadPage = 1;
					loadThreadPage ();
				}
			}
		}
		[GLib.ConnectBefore]
		protected void treeviewTopics_ButtonPress (object o, ButtonPressEventArgs args)
		{
		    if((int)args.Event.Button == 3)
    		{       
				Gtk.Menu popup = new Gtk.Menu();
	        	Gtk.MenuItem menuitemOpen = new MenuItem("Open");
				menuitemOpen.Activated += menuitemOpen_Activated;
	        	Gtk.MenuItem menuitemOpenLast = new MenuItem("Open Last Page");
				menuitemOpenLast.Activated += menuitemOpenLast_Activated;
				Gtk.MenuItem menuitemAddFavourite = new MenuItem("Set Star");
				menuitemAddFavourite.Activated += menuitemAddFavourite_Activated;
	        	Gtk.MenuItem menuitemRemoveFavourite = new MenuItem("Remove Star");
				menuitemRemoveFavourite.Activated += menuitemRemoveFavourite_Activated;
				Gtk.SeparatorMenuItem topicsSeparator = new SeparatorMenuItem();
	        	Gtk.MenuItem menuitemShowFavourites = new MenuItem("Show Only Starred Topics");
				menuitemShowFavourites.Activated += menuitemShowFavourites_Activated;
	        	Gtk.MenuItem menuitemShowAll = new MenuItem("Show All Topics");
				menuitemShowAll.Activated += menuitemShowAll_Activated;
	        	Gtk.MenuItem menuitemReload = new MenuItem("Reload Topics");
				menuitemReload.Activated += menuitemReload_Activated;

				TreePath path;
				treeviewTopics.GetPathAtPos((int)args.Event.X, (int)args.Event.Y, out path);

				if(path != null)
				{
					bool isFavourite = false;
					TreeIter selectedIter;
					this.treestoreTopics.GetIter (out selectedIter, path);

					String selTopicId = (String)treestoreTopics.GetValue (selectedIter, 0);
					for (int j=0; j < favThreads.Length; j++)
					{
						if(favThreads[j].Equals(selTopicId))
						{
							isFavourite = true;
							break;
						}
					}

					popup.Add(menuitemOpen);
					popup.Add(menuitemOpenLast);

					if(isFavourite)
						popup.Add(menuitemRemoveFavourite);
					else
						popup.Add (menuitemAddFavourite);

					popup.Add(topicsSeparator);
				}

				if(favThreads.Length > 1)
				{
					popup.Add(new SeparatorMenuItem());
					if(showFavourites)
						popup.Add (menuitemShowAll);
					else
						popup.Add (menuitemShowFavourites);
				}
				popup.Add(menuitemReload);
	        	popup.ShowAll();
	    	    popup.Popup();
		    }
		}

		protected void menuitemOpen_Activated (object sender, EventArgs args)
		{
			TreeIter selIter;
			treeviewTopics.Model.GetIter(out selIter, treeviewTopics.Selection.GetSelectedRows()[0]);
			currTopicId = (String)treestoreTopics.GetValue (selIter, 0);

			if(currTopicId != null)
			{
				currThreadPage = 1;
				loadThreadPage ();
			}
		}

		protected void menuitemOpenLast_Activated (object sender, EventArgs args)
		{
			TreeIter selIter;
			treeviewTopics.Model.GetIter(out selIter, treeviewTopics.Selection.GetSelectedRows()[0]);
			currTopicId = (String)treestoreTopics.GetValue (selIter, 0);

			if(currTopicId != null)
			{
				// TODO optimize!!!
				currThreadPage = 1;
				loadThreadPage ();
				currThreadPage = numThreadPages;
				loadThreadPage ();
			}
		}

		protected void menuitemReload_Activated (object sender, EventArgs args)
		{
			topicsLoaded = 0;
			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
			
			loadTopics();
			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.LeftPtr);
		}

		protected void menuitemAddFavourite_Activated (object sender, EventArgs args)
		{
			TreeIter selIter;
			treeviewTopics.Model.GetIter(out selIter, treeviewTopics.Selection.GetSelectedRows()[0]);
			String selTopicId = (String)treestoreTopics.GetValue (selIter, 0);

			String favouriteThreads = selTopicId;
			for(int i=0; i < favThreads.Length; i++)
				favouriteThreads += ";" + favThreads[i];

			UserSettings.setValue("Site" + site.forumName + ".Forum" + forum + ".Favourites", favouriteThreads);
			favThreads = favouriteThreads.Split (';');

			treeviewTopics.Model.SetValue (selIter, 5, new Gdk.Pixbuf (".images/icon_favourites16.png"));
		}

		protected void menuitemRemoveFavourite_Activated (object sender, EventArgs args)
		{
			TreeIter selIter;
			treeviewTopics.Model.GetIter (out selIter, treeviewTopics.Selection.GetSelectedRows () [0]);
			String selTopicId = (String)treestoreTopics.GetValue (selIter, 0);

			String favouriteThreads = String.Empty;
			for (int i=0; i < favThreads.Length; i++)
			{
				if(!favThreads[i].Equals(selTopicId))
				{
					if(favouriteThreads != String.Empty)
						favouriteThreads += ";" + favThreads [i];
					else
						favouriteThreads = favThreads [i];
				}
			}

			UserSettings.setValue("Site" + site.forumName + ".Forum" + forum + ".Favourites", favouriteThreads);
			favThreads = favouriteThreads.Split (';');

			// TODO set to empty
			treeviewTopics.Model.SetValue(selIter, 5, new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 1, 0, 0));
		}

		protected void menuitemShowFavourites_Activated (object sender, EventArgs args)
		{
			topicsLoaded = 0;
			showFavourites = true;
			treestoreTopics.Clear ();
			int totalTopics;

			int count = 0;

			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();

			do
			{
				totalTopics = site.loadTopics (forum, topicsLoaded, topicsLoaded + 50);

				for (int i=topicsLoaded; i<totalTopics && i<topicsLoaded + 50; i++) {
					for (int j=0; j < favThreads.Length; j++) {
						if (favThreads [j].Equals (site.getTopicId (forum, i))) {
							treestoreTopics.AppendValues (site.getTopicId (forum, i), site.getTopicTitle (forum, i), site.getTopicAuthor (forum, i), site.getTopicReplies (forum, i).ToString (), Convert.ToString (site.getTopicLastDate (forum, i)), new Gdk.Pixbuf (".images/icon_favourites16.png"));
							count++;
						}
					}
				}
				topicsLoaded += 50;
			} while (topicsLoaded < totalTopics && count < favThreads.Length - 1);

			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.LeftPtr);
		}

		protected void menuitemShowAll_Activated (object sender, EventArgs args)
		{
			topicsLoaded = 0;
			showFavourites = false;
			treestoreTopics.Clear ();

			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();

			loadTopics();

			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.LeftPtr);
		}

		protected void iconview_ItemActivated (object o, ItemActivatedArgs args)
		{
			iconStore.GetIter(out fullSizeIter, args.Path);
			String fileName = (String)iconStore.GetValue(fullSizeIter, 0);
			int imageNum = (int)iconStore.GetValue(fullSizeIter, 5);
			iconwindow.Hide();
			threadwindow.Hide();
			imageviewbtn.Hide();
			textviewbtn.Hide();
			imageSortAscending.Hide();
			imageSortDescending.Hide();
			imageLoadingProgress.Hide ();
			downbtn.Hide();
			upbtn.Hide();
			prevbtn.Show ();
			nextbtn.Show ();
			imagewindow.Show();
			closebtn.Show();
			rotatebtn.Show();
			savebtn.Show();
			sepFullsize.Show();

			imagewindow.GrabFocus();
			showFullsizeImage(fileName, ref this.fullSizeImage, imageNum);
		}

		private int loadTopics ()
		{
			int totalTopics = site.loadTopics (forum, topicsLoaded, topicsLoaded + 50);

			for (int i=topicsLoaded; i<totalTopics && i<topicsLoaded + 50; i++) {
				bool isFavourite = false;
				for (int j=0; j < favThreads.Length; j++) {
					if (favThreads [j].Equals (site.getTopicId (forum, i))) {
						isFavourite = true;
						break;
					}
				}

				if (isFavourite)
					treestoreTopics.AppendValues (site.getTopicId (forum, i), site.getTopicTitle (forum, i), site.getTopicAuthor (forum, i), site.getTopicReplies (forum, i).ToString (), Convert.ToString (site.getTopicLastDate (forum, i)), new Gdk.Pixbuf (".images/icon_favourites16.png"));
				else
					treestoreTopics.AppendValues (site.getTopicId (forum, i), site.getTopicTitle (forum, i), site.getTopicAuthor (forum, i), site.getTopicReplies (forum, i).ToString (), Convert.ToString (site.getTopicLastDate (forum, i)), null);
			}

			if (totalTopics > topicsLoaded + 50)
			{
				topicsLoaded += 50;
				treestoreTopics.AppendValues ("000", "Load more topics...", "", "", "");
			}
			else
				topicsLoaded = totalTopics;

			return totalTopics;
		}

		private void renderTopic (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			String topicId = (String)model.GetValue (iter, 0);
			String topicText = (String)model.GetValue (iter, 1);
			if (topicId.Equals ("000")) {
				(cell as Gtk.CellRendererText).Foreground = "blue";
				(cell as Gtk.CellRendererText).Markup = "<u>Load More Topics...</u>";
			}
			else 
			{
				(cell as Gtk.CellRendererText).Foreground = "black";
			(cell as Gtk.CellRendererText).Text = topicText;
			}
		}

		private void loadThreadPage ()
		{
			if(currTopicId == null)
				return;

			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();

			int totalposts = site.loadThread (forum, currTopicId, (currThreadPage - 1) * postsPerPage, (currThreadPage * postsPerPage) - 1);

			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Arrow);

			numThreadPages = ((totalposts - 1) / postsPerPage) + 1;
			threadPageLabel = "Page " + currThreadPage.ToString () + "/" + numThreadPages.ToString ();

			if (numThreadPages > currThreadPage) {
				nextbtn.Sensitive = true;
				lastbtn.Sensitive = true;
			} else {
				nextbtn.Sensitive = false;
				lastbtn.Sensitive = false;
			}

			if (currThreadPage > 1) {
				prevbtn.Sensitive = true;
				firstbtn.Sensitive = true;
			} else {
				prevbtn.Sensitive = false;
				firstbtn.Sensitive = false;
			}

			if (iconThread != null) {
				if (iconThread.IsAlive)
					iconThread.Abort ();

				iconThread = null;
			}

			threadViewLoaded = false;
			iconViewLoaded = false;

			if (this.textviewbtn.Active)
			{
				loadThreadView ();
				pageLabel.Text = threadPageLabel;
			}
			else
			{
				loadIconView();
				pageLabel.Text = iconPageLabel;
			}
		}

		private void loadThreadView ()
		{
			if (currTopicId == null)
				return;

			String threadContent = "<HTML><HEAD><STYLE>" +
				".intlink A:link {text-decoration: none; color: black;}" +
				".intlink A:visited {text-decoration: none; color: black;}" +
				".intlink A:active {text-decoration: none; color: grey;}" +
				".intlink A:hover {text-decoration: underline; color: grey;}" +
				"img { display: block; margin-left: auto; margin-right: auto; max-height: 300px; max-width: 500px;}" +
				"div.quote { margin-top: 10px;  margin-left: 30px; padding-left: 15px; border-left: 3px solid #fa0;} " +
				"</STYLE>";

			threadContent = threadContent +
				"<script type=\"text/javascript\">function load() {" +
				"var urllocation = location.href;" +
				"window.location.hash=\"gotoanchor\";" +
				"}</script>";

			threadContent = threadContent + "</HEAD><BODY ONLOAD=\"load()\"><FONT SIZE=\"2\">";


			int postNum = 0;
			String postContent;
			while ((postContent = site.getThreadPost(postNum)) != String.Empty) {
				int replyNum = postNum + ((currThreadPage - 1) * postsPerPage);

				if (replyNum == gotoAnchorNum) {
					threadContent += "<a name=\"gotoanchor\"></a>";
				}
				threadContent += "<HR><TABLE WIDTH=\"100%\" BORDER=0><TR><TD WIDTH=\"30%\">";
				threadContent += "<SPAN class=\"intlink\">";
				threadContent += "<A HREF=\"user:" + site.getPostAuthor (postNum) + "\">" + site.getPostAuthor (postNum) + "</A></SPAN>";
				threadContent += "</TD><TD  WIDTH=\"40%\" ALIGN=\"center\"><FONT SIZE=\"2\">" + site.getPostTime (postNum).ToString ()
					+ "</FONT></TD><TD  WIDTH=\"30%\" ALIGN=\"right\">" + replyNum.ToString () + "</TD></TR></TABLE>";
				threadContent += "<BR>";
				threadContent += "<CENTER><TABLE WIDTH=\"80%\" BORDER=0><TR><TD><FONT SIZE=\"2\">";
				threadContent += postContent;
				threadContent += "</FONT></TD></TR></TABLE></CENTER>";
				//threadContent += "<img src=\"http://cdn1.iconfinder.com/data/icons/Keyamoon-IcoMoon--limited/16/reply.png\">";
				postNum++;
			}
			threadContent += "</FONT></BODY></HTML>";
			threadbrowser.LoadHtmlString (threadContent, ""); // TODO add base_uri
			threadViewLoaded = true;
			if (gotoAnchorNum >= 0) {
				threadbrowser.ExecuteScript ("window.location.hash=\"gotoanchor\";");
				gotoAnchorNum = -1;
			}
		}

		private void loadIconView ()
		{
			iconStore.Clear ();

			if(currTopicId == null)
				return;

			iconViewLoaded = true;

			if (iconThread != null)
			{
				if (iconThread.IsAlive)
					iconThread.Abort ();
			}

//			if (site.getFoundThreadImages() > 0)
//			{
				imageLoadingProgress.Fraction = 0;
				imageLoadingProgress.Show ();
				foundImages = 0;
				iconThread = new Thread (new ThreadStart (loadImages));
				iconThread.Start ();
//			}
//			else if (site.getTopicReplies (this.forum, Convert.ToInt32(currTopicId)) <= postsPerPage)
//			{
//				iconPageLabel = "No images found in thread.";
//			}
//			else
//			{
//				// Todo read more from later pages
//				iconPageLabel = "No images found in first page of thread.";
//			}
		}

		[GLib.ConnectBefore]
		protected void iconView_ButtonPress (object o, ButtonPressEventArgs args)
		{
			if ((int)args.Event.Button == 3) {       
				TreePath path = this.iconview.GetPathAtPos((int)args.Event.X, (int)args.Event.Y);
				Gtk.Menu popup = new Gtk.Menu();
				if(path != null)
				{
					this.iconStore.GetIter (out selectedIconIter, path);

	        		Gtk.MenuItem menuitemGoToPost = new MenuItem("Go to post");
	        		Gtk.MenuItem menuitemShowByAuthor = new MenuItem("Show only images by " + (String)iconStore.GetValue(selectedIconIter, 3));
					menuitemGoToPost.Activated += menuitemGoToPost_Activated;
					menuitemShowByAuthor.Activated += menuitemShowByAuthor_Activated;
					popup.Add(menuitemGoToPost);
					if(readImageAuthor == String.Empty)
						popup.Add(menuitemShowByAuthor);
				}
				if(readImageAuthor != String.Empty)
				{
	        		Gtk.MenuItem menuitemShowAll = new MenuItem("Show all images");
					menuitemShowAll.Activated += menuitemShowAllAuthors_Activated;
					popup.Add(menuitemShowAll);
				}
	        	popup.ShowAll();
	    	    popup.Popup();
			}
		}

		protected void menuitemGoToPost_Activated (object sender, EventArgs args)
		{
			int postNum = (int)iconStore.GetValue(selectedIconIter, 4);
			currThreadPage = ((postNum - 1) / postsPerPage) + 1;
			this.textviewbtn.Active = true;
			this.imageviewbtn.Active = false;
			setActivePane(true);
			gotoAnchorNum = postNum;
			loadThreadPage();
		}

		protected void menuitemShowByAuthor_Activated (object sender, EventArgs args)
		{
			this.readImageAuthor = (String)iconStore.GetValue(selectedIconIter, 3);
			loadIconView();
		}

		protected void menuitemShowAllAuthors_Activated (object sender, EventArgs args)
		{
			this.readImageAuthor = "";
			loadIconView();
		}

		protected void imageviewbtn_Toggled (object o, EventArgs args)
		{
			this.textviewbtn.Active = !this.imageviewbtn.Active;
			setActivePane(this.textviewbtn.Active);
		}

		protected void textviewbtn_Toggled(object o, EventArgs args)
		{
			this.imageviewbtn.Active = !this.textviewbtn.Active;
			setActivePane(this.textviewbtn.Active);
		}

		protected void firstbtn_Clicked(object o, EventArgs args)
		{
			currThreadPage = 1;
			loadThreadPage();
		}

		protected void prevbtn_Clicked (object o, EventArgs args)
		{
			if (fullSizeMode) {
				fullsizePrev();
			} 
			else
			{
				currThreadPage--;
				loadThreadPage ();
			}
		}

		protected void nextbtn_Clicked (object o, EventArgs args)
		{
			if (fullSizeMode)
			{
				fullsizeNext();
			}
			else
			{
				currThreadPage++;
				loadThreadPage();
			}
		}

		protected void lastbtn_Clicked(object o, EventArgs args)
		{
			currThreadPage = numThreadPages;
			loadThreadPage();
		}

		protected void imageSortAscending_Toggled (object o, EventArgs args)
		{
			this.imageSortDescending.Active = !this.imageSortAscending.Active;
			loadIconView();
		}

		protected void imageSortDescending_Toggled(object o, EventArgs args)
		{
			this.imageSortAscending.Active = !this.imageSortDescending.Active;
			loadIconView();
		}

		protected void closebtn_Clicked(object o, EventArgs args)
		{
			fullsizeClose();
		}

		protected void rotatebtn_Clicked (object o, EventArgs args)
		{
			String fileName = (String)iconStore.GetValue (fullSizeIter, 0);
			Image tmpimage = new Image ();
			tmpimage.Pixbuf = new Gdk.Pixbuf (fileName);

			if (tmpimage != null && tmpimage.Pixbuf != null) {
				tmpimage.Pixbuf = tmpimage.Pixbuf.RotateSimple (Gdk.PixbufRotation.Clockwise);
				if (fileName.Contains (".png"))
					tmpimage.Pixbuf.Save (fileName, "png");
				else
					tmpimage.Pixbuf.Save (fileName, "jpeg");
			}

			// Set new thumbnail to icon
			Image thumbnailImage = new Image();
			createThumbnail(tmpimage, ref thumbnailImage);
			iconStore.SetValue(fullSizeIter, 1, thumbnailImage.Pixbuf);

			// TODO refactor to own resize method
			// TODO Resize with parent resize event
			Gdk.Rectangle r = contentBox.Allocation;
			double orgWidth = tmpimage.Pixbuf.Width;
			double orgHeight = tmpimage.Pixbuf.Height;
			if(orgWidth > (r.Width - 4) || orgHeight > (r.Height - 44))
			{
				double ratio = Math.Min( (r.Width - 4) / orgWidth, (r.Height - 44) / orgHeight );
				int width = (int)(ratio * (double)tmpimage.Pixbuf.Width);
				int height = (int)(ratio * (double)tmpimage.Pixbuf.Height);

				fullSizeImage.Pixbuf = tmpimage.Pixbuf.ScaleSimple(width, height, Gdk.InterpType.Nearest);
				fullsizeLayout.Move (fullSizeImage, (int)((r.Width - width) / 2), (int)((r.Height - height) /2));
			}
			else
			{
				fullSizeImage.Pixbuf = tmpimage.Pixbuf;
				fullsizeLayout.Move (fullSizeImage, (int)((r.Width - orgWidth) / 2), (int)((r.Height - orgHeight) /2));
			}
		}

		protected void savebtn_Clicked (object o, EventArgs args)
		{
			String fileName = (String)iconStore.GetValue (fullSizeIter, 0);
			FileChooserDialog fc= new FileChooserDialog("Save image", (Window)this.Toplevel, FileChooserAction.Save);
			fc.AddButton(Stock.Cancel, ResponseType.Cancel);
			fc.AddButton(Stock.Save, ResponseType.Ok);
			fc.CurrentName = System.IO.Path.GetFileName(fileName);
			if(saveFolder != string.Empty)
				fc.SetCurrentFolder(saveFolder);
			if (fc.Run() == (int)ResponseType.Ok) 
			{
				File.Copy(fileName, fc.Filename);
			}
			saveFolder = fc.CurrentFolder;
			fc.Destroy();
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

		protected void fullsizeLayout_SizeRequested (object o, EventArgs args)
		{
			if(fullsizeLayout.Width == oldWidth && fullsizeLayout.Height == oldHeight)
				return;

			oldWidth = fullsizeLayout.Width;
			oldHeight = fullsizeLayout.Height;

			String fileName = (String)iconStore.GetValue (fullSizeIter, 0);
			Image tmpimage = new Image ();
			tmpimage.Pixbuf = new Gdk.Pixbuf (fileName);

			Gdk.Rectangle r = contentBox.Allocation;
			double orgWidth = tmpimage.Pixbuf.Width;
			double orgHeight = tmpimage.Pixbuf.Height;
			if(orgWidth > (r.Width - 4) || orgHeight > (r.Height - 44))
			{
				double ratio = Math.Min( (r.Width - 4) / orgWidth, (r.Height - 44) / orgHeight );
				int width = (int)(ratio * (double)tmpimage.Pixbuf.Width);
				int height = (int)(ratio * (double)tmpimage.Pixbuf.Height);

				fullSizeImage.Pixbuf = tmpimage.Pixbuf.ScaleSimple(width, height, Gdk.InterpType.Nearest);
				fullsizeLayout.Move (fullSizeImage, (int)((r.Width - width) / 2), (int)((r.Height - height) /2));
			}
			else
			{
				fullSizeImage.Pixbuf = tmpimage.Pixbuf;
				fullsizeLayout.Move (fullSizeImage, (int)((r.Width - orgWidth) / 2), (int)((r.Height - orgHeight) /2));
			}
		}

		protected virtual void threadbrowser_NavigationRequested (object sender, NavigationRequestedArgs args)
		{
			if (args.Request.Uri.StartsWith ("user:"))
			{
				UserDialog dlg = new UserDialog (this.site, args.Request.Uri.Substring(5));
				if(dlg.userInfoLoaded)
				{
					dlg.Run ();
					dlg.Destroy();
				}
				else
				{
					dlg.Destroy();
					MessageDialog md = new MessageDialog((Window)this.Toplevel, DialogFlags.DestroyWithParent, MessageType.Info, 
					                                     ButtonsType.Ok, "User information not available");
					md.Run();
					md.Destroy();
				}
			}
			else
			{
				System.Diagnostics.Process.Start ("xdg-open", args.Request.Uri);
			}
			args.RetVal = 1;
		}

		private void setActivePane (bool textView)
		{
			if (textView)
			{
				iconwindow.Hide ();
				imagewindow.Hide ();
				imageSortAscending.Hide();
				imageSortDescending.Hide();
				imageLoadingProgress.Hide ();
				threadwindow.Show();
				firstbtn.Show ();
				prevbtn.Show ();
				nextbtn.Show();
				lastbtn.Show ();
				if(!threadViewLoaded)
				{
					currThreadPage = 1;
					loadThreadPage ();
					loadThreadView();
				}

				pageLabel.Text = threadPageLabel;
			}
			else
			{
				imagewindow.Hide ();
				threadwindow.Hide();
				firstbtn.Hide ();
				prevbtn.Hide ();
				nextbtn.Hide();
				lastbtn.Hide ();
				imageSortAscending.Show();
				imageSortDescending.Show();
				iconwindow.Show ();
				pageLabel.Text = iconPageLabel;

				if(iconThread != null)
					if (iconThread.IsAlive)
						imageLoadingProgress.Show ();

				if(!iconViewLoaded)
					loadIconView();
			}
		}

		private void loadImages ()
		{
			// Separate thread, so no need, just added for clarity...
			String topicId = currTopicId;

			if (topicId == null)
				return;

			if (!Directory.Exists(FileCache.CacheDir)) {
				Directory.CreateDirectory(FileCache.CacheDir);
			}

			int totalReplies = site.getTopicReplies (this.forum, site.getTopicNumFromId (this.forum, topicId));
			int counter; // TODO counter -> pageNum
			List<Forum.ImageData> imageData = new List<Forum.ImageData> ();

			if (this.imageSortAscending.Active)
			{
				counter = 0;
				if (site.LoadedStart != 0)
					site.loadThread (this.forum, this.currTopicId, 0, postsPerPage - 1);
				for (int i = 0; i< site.LoadedPosts; i++)
				{
					if(readImageAuthor.Length > 0)
					{
						if(!site.getPostAuthor(i).Equals(readImageAuthor))
							continue;
					}
					site.getPostImages (i, ref imageData);
				}
			}
			else
			{
				counter = numThreadPages- 1;
				site.loadThread (this.forum, this.currTopicId, counter * postsPerPage, (counter * postsPerPage) + postsPerPage - 1);
				for (int i = site.LoadedPosts -1; i >= 0; i--)
				{
					List<Forum.ImageData> tmpImageData = new List<Forum.ImageData> ();
					if(readImageAuthor.Length > 0)
					{
						if(!site.getPostAuthor(i).Equals(readImageAuthor))
							continue;
					}
					site.getPostImages (i, ref tmpImageData);
					for(int j = tmpImageData.Count -1; j >= 0; j--)
					{
						imageData.Add(tmpImageData.ToArray()[j]);
					}
				}
			}

			bool loadingDone = false;

			do {
				int imageNum = 0;
				foreach (Forum.ImageData image in imageData)
				{
					try 
					{
						String fileName = image.imageUrl.Replace ('/', '.');
						String filePathName = System.IO.Path.Combine(FileCache.CacheDir, fileName);
						if (!File.Exists (filePathName))
						{
							HttpWebRequest request = HttpWebRequest.Create(image.imageUrl) as HttpWebRequest;
							request.Timeout = 5000;
							String userName = site.forumUsername;
							String password = site.forumPassword;
							if (site.sessionCookies != null) 
							{
								request.CookieContainer = new CookieContainer ();
								request.CookieContainer.Add (site.sessionCookies);
							}
							request.KeepAlive = false;
							if (userName != String.Empty)
							{
								request.Credentials = new NetworkCredential (userName, password);
							}
							HttpWebResponse response = request.GetResponse () as HttpWebResponse;
							if (response.StatusCode == HttpStatusCode.OK) {
								Stream readstream = response.GetResponseStream ();
								FileStream filestream = new FileStream (filePathName, FileMode.Create, FileAccess.ReadWrite);
								int Length = 256;
								byte [] buffer = new byte[Length];
								int bytesRead = readstream.Read (buffer, 0, Length);
								while (bytesRead > 0) {
									filestream.Write (buffer, 0, bytesRead);
									bytesRead = readstream.Read (buffer, 0, Length);
								}
								filestream.Close ();
								readstream.Close ();
								response.Close ();
								FileCache.Add(fileName);
							}
						}
						// Send filename to UI thread
						double progress = 0;
						imageNum++;
						if (this.imageSortAscending.Active)
						{
							// TODO imageData.Count check only images in current page
							if(totalReplies > postsPerPage - 1)
								progress = (double)((counter * postsPerPage) + (double)((double)imageNum / (double)imageData.Count)) / (double)(totalReplies);
							else
								progress = (double)((double)imageNum / (double)imageData.Count);
						}
						else
						{
							if(totalReplies > postsPerPage -1)
								progress = (double)(totalReplies -(counter * postsPerPage) + (double)((double)(imageData.Count - imageNum) / (double)imageData.Count)) / (double)(totalReplies);
							else
								progress = (double)((double)imageNum / (double)imageData.Count);
						}

						Gtk.Application.Invoke (delegate { updateImageIcons(filePathName, image, topicId, progress); });
					} 
					catch (Exception ex)
					{
						// for WebException
						/*
						if (ex.Status == WebExceptionStatus.ProtocolError)
						{
							if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
							{
								// TODO handle the 404 here - return "error" image?
							}
						}
						else if (ex.Status == WebExceptionStatus.NameResolutionFailure) 
						{
							// TODO handle name resolution failure - return "error" image?
						}
						*/
					}
				}
				imageData = new List<Forum.ImageData> ();
				if (this.imageSortAscending.Active)
				{
					counter ++;
					if ((postsPerPage * counter) >= totalReplies)
						loadingDone = true;
					else
					{
						site.loadThread(this.forum, this.currTopicId, postsPerPage * counter, (postsPerPage * counter) + postsPerPage - 1);
						for (int i = 0; i< site.LoadedPosts; i++)
						{
							if(readImageAuthor.Length > 0)
							{
								if(!site.getPostAuthor(i).Equals(readImageAuthor))
									continue;
							}
							site.getPostImages (i, ref imageData);
						}
					}
				}
				else
				{
					counter--;
					if (counter < 0)
						loadingDone = true;
					else
					{
						site.loadThread(this.forum, this.currTopicId, postsPerPage * counter, (postsPerPage * counter) + postsPerPage - 1);
						for (int i = site.LoadedPosts - 1; i >= 0; i--)
						{
							if(readImageAuthor.Length > 0)
							{
								if(!site.getPostAuthor(i).Equals(readImageAuthor))
									continue;
							}
							site.getPostImages (i, ref imageData);
						}
					}
				}
			} while(!loadingDone);
			Gtk.Application.Invoke (delegate { this.imageLoadingProgress.Visible = false; });
		}

		private void fullsizeNext()
		{
			TreeIter iter;
			if(getNextIcon (fullSizeIter, out iter) == true)
			{
				fullSizeIter = iter;
				String fileName = (String)iconStore.GetValue(fullSizeIter, 0);
				int imageNum = (int)iconStore.GetValue(fullSizeIter, 5);
				showFullsizeImage(fileName, ref this.fullSizeImage, imageNum);
			}
		}

		private void fullsizePrev()
		{
			TreeIter iter;
			if (getPreviousIcon (fullSizeIter, out iter) == true)
			{
				fullSizeIter = iter;
				String fileName = (String)iconStore.GetValue (fullSizeIter, 0);
				int imageNum = (int)iconStore.GetValue (fullSizeIter, 5);
				showFullsizeImage (fileName, ref this.fullSizeImage, imageNum);
			}
		}

		private void fullsizeClose()
		{
			closebtn.Hide();
			rotatebtn.Hide ();
			savebtn.Hide ();
			sepFullsize.Hide ();
			imageviewbtn.Show();
			textviewbtn.Show();
			if(this.Position == 0)
				this.downbtn.Show();
			else
				this.upbtn.Show();
			this.fullSizeMode = false;
			setActivePane(this.textviewbtn.Active);
			iconview.Model = iconStore;
		}

		private void updateImageIcons (String fileName, Forum.ImageData imageData, String topicId, double progress)
		{
			foundImages++;
			// iconPageLabel = "Fetched image " + (imageNum + 1).ToString () + "/" + site.getFoundThreadImages ().ToString ();
			// pageLabel.Text = iconPageLabel;
/*
			// TODO change handling of progress bar!
			if (foundImages == site.getFoundThreadImages ())
			{
				imageLoadingProgress.Hide ();
			} 
			else
			{
				imageLoadingProgress.Fraction = (double)foundImages / (double)(site.getFoundThreadImages ());
				imageLoadingProgress.Show ();
			}
*/
			imageLoadingProgress.Fraction = progress;

			// Make sure other topic has not been selected
			if (fileName != null && topicId.Equals(currTopicId))
			{
				Image tmpimage = new Image ();
				try
				{
					tmpimage.Pixbuf = new Gdk.Pixbuf (fileName);
				}
				catch(Exception)
				{
					// TODO display "error" image?
				}
				if (tmpimage != null && tmpimage.Pixbuf != null)
				{
					String tooltipText;
					Image thumbnailImage = new Image();
					if(imageData.imagePost == 0)
						tooltipText = "From original post by " + imageData.imageAuthor;
					else
						tooltipText = "From reply " + (imageData.imagePost).ToString () + 
						 " by " + imageData.imageAuthor;
					createThumbnail (tmpimage, ref thumbnailImage); 
					iconStore.AppendValues (fileName, thumbnailImage.Pixbuf, tooltipText, imageData.imageAuthor, imageData.imagePost, foundImages - 1);
				}
			}
			if(!fullSizeMode)
				iconview.Model = iconStore;
		}

		private void createThumbnail (Image fullsizeImage, ref Image thumbnailImage)
		{
			// Scale image
			double orgWidth = fullsizeImage.Pixbuf.Width;
			double orgHeight = fullsizeImage.Pixbuf.Height;
			if (orgWidth > 150 || orgHeight > 150)
			{
				double ratio = Math.Min (150 / orgWidth, 150 / orgHeight);
				int width = (int)(ratio * (double)fullsizeImage.Pixbuf.Width);
				int height = (int)(ratio * (double)fullsizeImage.Pixbuf.Height);

				thumbnailImage.Pixbuf = fullsizeImage.Pixbuf.ScaleSimple (width, height, Gdk.InterpType.Nearest);
			}
			else
			{
				thumbnailImage.Pixbuf = fullsizeImage.Pixbuf;
			}
		}

		private void showFullsizeImage(String fileName, ref Image image, int imageNum)
		{
			pageLabel.Text = (imageNum + 1).ToString() + "/" + this.foundImages.ToString();
			if(imageNum > 0)
				prevbtn.Sensitive = true;
			else
				prevbtn.Sensitive = false;

			if(imageNum < (this.foundImages - 1))
				nextbtn.Sensitive = true;
			else
				nextbtn.Sensitive = false;

			if(File.Exists(fileName))
			{
				Image tmpimage = new Image();
				tmpimage.Pixbuf = new Gdk.Pixbuf(fileName);

				if(tmpimage != null && tmpimage.Pixbuf != null)
				{
					// TODO Resize with parent resize event
					Gdk.Rectangle r = contentBox.Allocation;
					double orgWidth = tmpimage.Pixbuf.Width;
					double orgHeight = tmpimage.Pixbuf.Height;
					if(orgWidth > (r.Width - 4) || orgHeight > (r.Height - 44))
					{
						double ratio = Math.Min( (r.Width - 4) / orgWidth, (r.Height - 44) / orgHeight );
						int width = (int)(ratio * (double)tmpimage.Pixbuf.Width);
						int height = (int)(ratio * (double)tmpimage.Pixbuf.Height);

						image.Pixbuf = tmpimage.Pixbuf.ScaleSimple(width, height, Gdk.InterpType.Nearest);
						fullsizeLayout.Move (image, (int)((r.Width - width) / 2), (int)((r.Height - height) /2));
					}
					else
					{
						image.Pixbuf = tmpimage.Pixbuf;
						fullsizeLayout.Move (image, (int)((r.Width - orgWidth) / 2), (int)((r.Height - orgHeight) /2));
					}
				}
			}
			//transparentTextBox();
			this.fullSizeMode = true;
	    }

		private bool getNextIcon (TreeIter iter, out TreeIter nextIter)
		{
			TreeIter foundIter;
			iconStore.GetIterFirst(out foundIter);
			do
			{
				if(foundIter.Equals(iter))
				{
					iconStore.IterNext(ref foundIter);
					nextIter = foundIter;
					return true;
				}
			} while(iconStore.IterNext(ref foundIter) == true);
			nextIter = new TreeIter ();
			return false;
		}

		private bool getPreviousIcon (TreeIter iter, out TreeIter prevIter)
		{
			TreeIter foundIter;
			TreeIter saveIter = new TreeIter();
			bool firstLoop = true;
			iconStore.GetIterFirst(out foundIter);
			do
			{
				if(foundIter.Equals(iter))
				{
					prevIter = saveIter;
					if(firstLoop)
						return false;
					else
					return true;
				}
				firstLoop = false;
				saveIter = foundIter;
			} while(iconStore.IterNext(ref foundIter) == true);
			prevIter = new TreeIter ();
			return false;
		}

		private void transparentTextBox()
		{
			Gdk.Rectangle r = contentBox.Allocation;
			Image i = new Image();
			i.Pixbuf = new Gdk.Pixbuf(".images/transpBlack50.png");
			i.Pixbuf = i.Pixbuf.ScaleSimple(r.Width, 50, Gdk.InterpType.Nearest);
    		fullsizeLayout.Add(i);
		    fullsizeLayout.Move (i, 0, 0);


			/*
			ImageSurface surface = new ImageSurface(Format.RGB24, 120, 120);
			Context cr = new Context(surface);

			cr.SetSourceRGBA((0, 0, 1, i*0.1);
			cr.Rectangle(50*i, 20, 40, 40);
			cr.Fill  ();
			*/
			Label l= new Label("Teststring in label");
			l.ModifyFg(StateType.Normal, new Gdk.Color (0xff, 0xff, 0xff));
			fullsizeLayout.Add (l);
			fullsizeLayout.Move(l, 20, 10);

			//transBox.ShowAll();
			fullsizeLayout.ShowAll();
		}
	}
}

