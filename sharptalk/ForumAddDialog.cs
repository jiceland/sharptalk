using System;

namespace sharptalk
{
	public partial class ForumAdd : Gtk.Dialog
	{
		public String ForumName {
			get {
				return this.entryForumName.Text;
			}
		}

		public String ForumUrl {
			get {
				return this.entryForumUrl.Text;
			}
		}

		public String ForumUsername {
			get {
				return this.entryForumUsername.Text;
			}
		}

		public String ForumPassword {
			get {
				return this.entryForumPassword.Text;
			}
		}

		public ForumAdd ()
		{
			this.Build ();
			this.checkbutton1.Toggled += checkbutton1_Toggled;
			this.entryForumName.Changed += entryForumName_Changed;
			this.entryForumUrl.Changed += entryForumUrl_Changed;
			this.entryForumPassword.Visibility = false;
			this.entryForumUsername.Sensitive = false;
			this.entryForumPassword.Sensitive = false;
			this.buttonOk.Sensitive = false;
		}

		protected void entryForumName_Changed(object o, EventArgs args)
		{
			if(entryForumName.Text.Length > 0 && entryForumUrl.Text.Length > 0)
				this.buttonOk.Sensitive = true;
			else
				this.buttonOk.Sensitive = false;
		}

		protected void entryForumUrl_Changed(object o, EventArgs args)
		{
			if(entryForumName.Text.Length > 0 && entryForumUrl.Text.Length > 0)
				this.buttonOk.Sensitive = true;
			else
				this.buttonOk.Sensitive = false;
		}

		protected void checkbutton1_Toggled (object o, EventArgs args)
		{
			if (checkbutton1.Active) 
			{
				this.entryForumUsername.Sensitive = true;
				this.entryForumPassword.Sensitive = true;
			} 
			else
			{
				this.entryForumUsername.Text = "";
				this.entryForumPassword.Text = "";
				this.entryForumUsername.Sensitive = false;
				this.entryForumPassword.Sensitive = false;
			}
		}
	}
}

