using System;
using System.Net;
using System.IO;

namespace sharptalk
{
	public partial class UserDialog : Gtk.Dialog
	{
		public bool userInfoLoaded = false;

		private Forum site;
		private String userId;
		private String userName;

		public UserDialog (Forum site, String userName)
		{
			this.Build ();
			this.site = site;
			this.userName = userName;
			this.userId = site.getUserId (userName);
			if (!userId.Equals(string.Empty)) {
				this.image2.Pixbuf = new Gdk.Pixbuf (getAvatar ());
				this.labelNumPosts.Text = site.getUserPostCount (userName).ToString ();
				this.labelLastActivity.Text = site.getUserLastActivityTime (userName).ToString ();
				userInfoLoaded = true;
			}
		}

		private String getAvatar()
		{
			String fileName = System.IO.Path.Combine(FileCache.CacheDir, "Avatar-" + this.userName);
			try 
			{
				if (!File.Exists (fileName))
				{
					HttpWebRequest request = HttpWebRequest.Create(site.forumUrl + "/mobiquo/avatar.php?user_id=" + userId) as HttpWebRequest;

					String userName = site.forumUsername;
					String password = site.forumPassword;
					if (site.sessionCookies != null) 
					{
						request.CookieContainer = new CookieContainer ();
						request.CookieContainer.Add (site.sessionCookies);
					}
					if (userName != String.Empty)
					{
						request.Credentials = new NetworkCredential (userName, password);
					}
					request.KeepAlive = false;
					HttpWebResponse response = request.GetResponse () as HttpWebResponse;
					if (response.StatusCode == HttpStatusCode.OK)
					{
						Stream readstream = response.GetResponseStream ();
						FileStream filestream = new FileStream (fileName, FileMode.Create, FileAccess.ReadWrite);
						int Length = 256;
						byte [] buffer = new byte[Length];
						int bytesRead = readstream.Read (buffer, 0, Length);
						while (bytesRead > 0)
						{
							filestream.Write (buffer, 0, bytesRead);
							bytesRead = readstream.Read (buffer, 0, Length);
						}
						filestream.Close ();
						readstream.Close ();
						response.Close ();
						FileCache.Add("Avatar-" + this.userName);
					}
				}
			}
			catch(Exception) {}
			return fileName;
		}
	}
}

