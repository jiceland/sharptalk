using System;
using System.Collections.Generic;
using System.ComponentModel;
using CookComputing.XmlRpc;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

public struct ConfigData
{
    public String version;
    public Boolean guest_okay;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public String support_md5;
}

public struct LoginData
{
    public Boolean result;
}

public struct ForumData
{
    public String forum_id;
    public String parent_id;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public Boolean sub_only;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public Byte[] forum_name;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public Byte[] description;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public ForumData[] child;
}

public struct TopicDataEntry
{
    public String topic_id;
    public Byte[] topic_title;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public Byte[] topic_author_name;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public int reply_number;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public DateTime last_reply_time;
}

public struct TopicData
{
    public int total_topic_num;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public String forum_id;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public Byte[] forum_name;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public TopicDataEntry[] topics;
}

public struct PostAttachmentData
{
    public String content_type;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public String thumbnail_url;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public String url;
}

public struct ThreadDataEntry
{
    public String post_id;
    public Byte[] post_title;
    public Byte[] post_content;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public String post_author_id;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public Byte[] post_author_name;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public DateTime post_time;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public PostAttachmentData[] attachments;
}

public struct ThreadData
{
    public int total_post_num;
    public ThreadDataEntry[] posts;
}

public struct UserInfo
{
	public String user_id;
	public Byte[] user_name;
    public int post_count;
    public DateTime reg_time;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public DateTime last_activity_time;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public bool is_online;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public bool accept_pm;
}

public struct BoxInfoEntry
{
	public String box_id;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public Byte[] box_name;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public int msg_count;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public int unread_count;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public String box_type;
}

public struct BoxInfo
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public bool result;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public BoxInfoEntry[] list;
}

public struct BoxDataEntryTo
{
	public Byte[] username;
}

public struct BoxDataEntry
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public String msg_id;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public int msg_state;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public DateTime sent_date;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public Byte[] msg_from;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public Byte[] msg_subject;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public BoxDataEntryTo[] msg_to;
}

public struct BoxData
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public bool result;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public int total_message_count;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public int total_unread_count;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public BoxDataEntry[] list;
}

public struct MessageData
{
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public bool result;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public Byte[] msg_from;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public DateTime sent_date;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public Byte[] msg_subject;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public BoxDataEntryTo[] msg_to;
    [XmlRpcMissingMapping(MappingAction.Ignore)]
	public Byte[] text_body;
}
//    [XmlRpcUrl("http://forums.macrumors.com/mobiquo/mobiquo.php")]
//    [XmlRpcUrl("http://www.pattaya-addicts.com/forum/mobiquo/mobiquo.php")]
public interface ITapatalk : IXmlRpcProxy
{
    [XmlRpcMethod]
    ConfigData get_config();

    [XmlRpcMethod]
    LoginData login(byte[] login_name, byte[] password);

	[XmlRpcMethod]
    ForumData[] get_forum();

    [XmlRpcMethod]
    TopicData get_topic(String forum_id, int start_num, int last_num, String mode);

	// API level 3
    [XmlRpcMethod]
    ThreadData get_thread(String topic_id, int start_num, int last_num);

	// API level 4
	[XmlRpcMethod]
    ThreadData get_thread(String topic_id, int start_num, int last_num, Boolean return_html);

	// API level 3
	[XmlRpcMethod]
    UserInfo get_user_info(byte[] user_name);

	// API level 4
	[XmlRpcMethod]
    UserInfo get_user_info(byte[] user_name, String user_id);

	[XmlRpcMethod]
    BoxInfo get_box_info();

	[XmlRpcMethod]
    BoxData get_box(String box_id, int start_num, int end_num);

	// API level 3
    [XmlRpcMethod]
    MessageData get_message(String message_id, String box_id);

	// API level 4
	[XmlRpcMethod]
    MessageData get_message(String message_id, String box_id, Boolean return_html);
}

public static class Retry
{
   public static void Do(Action action, int retryCount = 3)
   {
       Do<object>(() => 
       {
           action();
           return null;
       }, retryCount);
   }
   public static T Do<T>(Func<T> action, int retryCount = 3)
   {
       var exceptions = new List<Exception>();

       for (int retry = 0; retry < retryCount; retry++)
       {
          try
          { 
              return action();
          }
          catch (Exception ex)
          { 
			  // this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Pirate);
              exceptions.Add(ex);
              Thread.Sleep(TimeSpan.FromSeconds(2));
			  // this.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
          }
       }
		return default(T);
//       throw new AggregateException(exceptions);
   }
}

public class Forum
{
	public ITapatalk proxy;
	public String forumName;
	public String forumUrl;
	public String supportMd5;
	public String apiVersion;
	public String forumUsername;
	public String forumPassword;
	public bool forumValid;

	public CookieCollection sessionCookies = null;

	private TopicData topics;
	private ThreadData thread;
	private UserInfo userInfo;
	private BoxData messageBox;

	private int loadedStart = -1;
	private int loadedEnd = -1;

	public struct ImageData
	{
		public String imageUrl;
		public String imageAuthor;
		public int imagePost;
	}

	public  List<ImageData> threadImages;

	public int LoadedPosts {
		get {
			return thread.posts.Length;
		}
	}

	public int LoadedStart {
		get {
			return this.loadedStart;
		}
	}

	public int LoadedEnd {
		get {
			return this.LoadedEnd;
		}
	}

	public Forum (String forumName, String forumUrl)
	{
		this.forumName = forumName;
		this.forumUrl = forumUrl;
		this.supportMd5 = "0";
		this.apiVersion = "4";
		this.threadImages = new List<ImageData>();

		try 
		{
			proxy = XmlRpcProxyGen.Create<ITapatalk> ();
			proxy.Timeout = 5000;
			proxy.Url = forumUrl + "/mobiquo/mobiquo.php";
			ConfigData ret;
			ret = Retry.Do (() => proxy.get_config());

			if (ret.support_md5 != null)
				supportMd5 = ret.support_md5;

			if (ret.version != null)
				apiVersion = ret.version;

			forumValid = true;
		} 
		catch (Exception) 
		{
			forumValid = true;
		}
	}

	public bool login (String username, String password)
	{
		this.forumUsername = username;
		this.forumPassword = password;
		UTF8Encoding enc = new UTF8Encoding ();  
		LoginData l;

		if (this.supportMd5.Equals ("1"))
			password = MD5Hash(password);

		l = Retry.Do (() => proxy.login (enc.GetBytes (username), enc.GetBytes (password)));
		this.sessionCookies = proxy.ResponseCookies;
		return l.result;
	}

	public void loadForums()
	{
		int siteNum = UserSettings.getNextSettingNum("Site");

		UserSettings.setValue("Site" + siteNum.ToString(), forumName);
		UserSettings.setValue("Site" + siteNum.ToString() + ".Url", forumUrl);
		UserSettings.setValue("Site" + siteNum.ToString() + ".User", forumUsername);
		UserSettings.setValue("Site" + siteNum.ToString() + ".Pwd", forumPassword);

		// Assumes logged in
		if(!String.IsNullOrEmpty(forumUsername))
			getBoxes(siteNum);

		if(this.sessionCookies != null)
			proxy.CookieContainer.Add(sessionCookies);
		ForumData[] forums = Retry.Do (() => proxy.get_forum ());

		getForumChildren (siteNum, 0, 0, forums);
	}

	private void getBoxes(int siteNum)
	{
		UTF8Encoding enc = new UTF8Encoding ();  
		BoxInfo boxes = Retry.Do (() => proxy.get_box_info ());
		if(boxes.list != null)
		{
			foreach (BoxInfoEntry entry in boxes.list)
			{
				String id = entry.box_id;
				String name = (enc.GetString (entry.box_name));
				String type = entry.box_type;
				if (type.Equals ("INBOX") || name.Equals ("Inbox"))
					UserSettings.setValue ("Site" + siteNum.ToString () + ".Inbox", id);
				if (type.Equals ("SENT") || name.Equals ("Outbox"))
					UserSettings.setValue ("Site" + siteNum.ToString () + ".Outbox", id);
			}
		}
	}

	private int getForumChildren (int siteNum, int forumNum, int level, ForumData[] forums)
	{
		int currForum = forumNum;
		UTF8Encoding enc = new UTF8Encoding ();

		// Sanity check
		if(level > 9)
			return forumNum;

		foreach (ForumData dataEntry in forums) {
			currForum += 1;
			String forumId = "Site" + siteNum.ToString () + ".Forum" + currForum.ToString ();  
			string name = enc.GetString (dataEntry.forum_name);
			UserSettings.setValue (forumId, name);
			UserSettings.setValue (forumId + ".Id", dataEntry.forum_id);
			UserSettings.setValue (forumId + ".Parent", dataEntry.parent_id);
			if (dataEntry.sub_only == true)
				UserSettings.setValue (forumId + ".IsFolder", "TRUE");

			if (dataEntry.child != null)
				currForum = getForumChildren (siteNum, currForum, level + 1, dataEntry.child);
		}
		return currForum; 
	}

	public int loadTopics (String forumId, int start, int end)
	{
        if (forumId != null)
        {
			if(this.sessionCookies != null)
				proxy.CookieContainer.Add(sessionCookies);
			if(start == 0)
	        	topics = Retry.Do (() => proxy.get_topic(forumId, start, end, ""));
			else
			{
				TopicData tmpTopics = Retry.Do (() => proxy.get_topic(forumId, start, end, ""));
				int arrayLen = topics.topics.Length;
				Array.Resize(ref topics.topics, arrayLen + tmpTopics.topics.Length);
				foreach(TopicDataEntry entry in tmpTopics.topics)
				{
					topics.topics[arrayLen++] = entry;
				}
			}
			return topics.total_topic_num;
        }
		return 0;
	}

	public int getTopicNumFromId (String forumId, String topicId)
	{
		for (int i=0; i < topics.topics.Length; i++)
		{
			if(topics.topics[i].topic_id.Equals(topicId))
			   return i;
		}
	   return -1;
	}

	public String getTopicId(String forumId, int topicNum)
	{
		if(topicNum < topics.total_topic_num)
			return topics.topics[topicNum].topic_id;
		else
			return String.Empty;
	}

	public String getTopicTitle(String forumId, int topicNum)
	{
		UTF8Encoding enc = new UTF8Encoding ();
		if(topicNum < topics.total_topic_num)
			return enc.GetString(topics.topics[topicNum].topic_title);
		else
			return String.Empty;
	}

	public String getTopicAuthor(String forumId, int topicNum)
	{
		UTF8Encoding enc = new UTF8Encoding ();
		if(topicNum < topics.total_topic_num)
			return enc.GetString(topics.topics[topicNum].topic_author_name);
		else
			return String.Empty;
	}

	public int getTopicReplies(String forumId, int topicNum)
	{
		if(topicNum < topics.total_topic_num)
			return topics.topics[topicNum].reply_number;
		else
			return 0;
	}

	public System.DateTime getTopicLastDate(String forumId, int topicNum)
	{
		if(topicNum < topics.total_topic_num)
			return topics.topics[topicNum].last_reply_time;
		else
			return System.DateTime.MinValue;
	}

	public int loadThread (String forumId, String topicId, int start, int end)
	{
		threadImages.Clear();

		if (forumId != null)
		{
			if(this.sessionCookies != null)
				proxy.CookieContainer.Add(sessionCookies);

			if(this.apiVersion.StartsWith("4"))
				thread = Retry.Do (() => proxy.get_thread (topicId, start, end, true));
			else
				thread = Retry.Do (() => proxy.get_thread (topicId, start, end));

			this.loadedStart = start;
			this.loadedEnd = end;

			return thread.total_post_num;
		}
		return 0;
	}

	public String getThreadPost (int postNum)
	{
		UTF8Encoding enc = new UTF8Encoding ();
		if (postNum < thread.posts.Length)
			return postToHtml (enc.GetString (thread.posts [postNum].post_content));

		return String.Empty;
	}

	public String getPostAuthor(int postNum)
	{
		UTF8Encoding enc = new UTF8Encoding();
		if(postNum < thread.posts.Length)
		   return enc.GetString(thread.posts[postNum].post_author_name);
	    else
			return String.Empty;
	}

	public String getPostAuthorId(int postNum)
	{
		if(postNum < thread.posts.Length)
		   return thread.posts[postNum].post_author_id;
	    else
			return String.Empty;
	}

	public DateTime getPostTime(int postNum)
	{
		if(postNum < thread.posts.Length)
		   return thread.posts[postNum].post_time;
	    else
			return DateTime.MinValue;
	}

	public String getUserId(String userName)
	{
		if(loadUserInfo(userName))
			return userInfo.user_id;
		else
			return String.Empty;
	}

	public int getUserPostCount(String userName)
	{
		if(loadUserInfo(userName))
			return userInfo.post_count;
		else
			return 0;
	}

	public DateTime getUserRegTime(String userName)
	{
		if(loadUserInfo(userName))
			return userInfo.reg_time;
		else
			return DateTime.MinValue;
	}

	public DateTime getUserLastActivityTime(String userName)
	{
		if(loadUserInfo(userName))
			return userInfo.last_activity_time;
		else
			return DateTime.MinValue;
	}

	public bool getUserIsOnline(String userName)
	{
		if(loadUserInfo(userName))
			return userInfo.is_online;
		else
			return false;
	}

	public bool getUserAcceptPm(String userName)
	{
		if(loadUserInfo(userName))
			return userInfo.accept_pm;
		else
			return false;
	}

	public int loadMessages (String boxId, int start, int end)
	{
		try {
			if (boxId != null) {
				if (this.sessionCookies != null)
					proxy.CookieContainer.Add (sessionCookies);
				if (start == 0)
					messageBox = Retry.Do (() => proxy.get_box (boxId, start, end));
				else {
					BoxData tmpBox = Retry.Do (() => proxy.get_box (boxId, start, end));
					int arrayLen = messageBox.list.Length;
					Array.Resize (ref topics.topics, arrayLen + tmpBox.list.Length);
					foreach (BoxDataEntry entry in tmpBox.list) {

						messageBox.list [arrayLen++] = entry;
					}
				}
				return messageBox.total_message_count;
			}
			return 0;
		}
		catch (Exception e) 
		{
			return 0;
		}
	}

	public String getMessageId(int messageNum)
	{
		UTF8Encoding enc = new UTF8Encoding ();
		if(messageNum < messageBox.total_message_count)
			return messageBox.list[messageNum].msg_id;
		else
			return String.Empty;
	}

	public String getMessageUser(int messageNum)
	{
		UTF8Encoding enc = new UTF8Encoding ();
		if(messageNum < messageBox.total_message_count)
			return enc.GetString(messageBox.list[messageNum].msg_from);
		else
			return String.Empty;
	}

	public String getMessageSubject(int messageNum)
	{
		UTF8Encoding enc = new UTF8Encoding ();
		if(messageNum < messageBox.total_message_count)
			return enc.GetString(messageBox.list[messageNum].msg_subject);
		else
			return String.Empty;
	}

	public DateTime getMessageSentDate(int messageNum)
	{
		if(messageNum < messageBox.total_message_count)
			return messageBox.list[messageNum].sent_date;
		else
			return DateTime.MinValue;
	}

	public String getMessageContent (String boxId, String messageId)
	{
		UTF8Encoding enc = new UTF8Encoding ();
		MessageData message;
		if (this.sessionCookies != null)
			proxy.CookieContainer.Add (sessionCookies);

		if (this.apiVersion.StartsWith ("4"))
		{
			message = Retry.Do (() => proxy.get_message (messageId, boxId, true));
			return(enc.GetString (message.text_body));
		}
		else
		{
			message = Retry.Do (() => proxy.get_message (messageId, boxId));
			return(postToHtml (enc.GetString (message.text_body)));
		}
	}

	private bool loadUserInfo (String userName)
	{
		UTF8Encoding enc = new UTF8Encoding ();

		if (this.userInfo.user_name != null)
		if (enc.GetString (this.userInfo.user_name).Equals (userName))
			return true;

		try {
			byte[] user = System.Text.Encoding.UTF8.GetBytes (userName);
			if (this.apiVersion.StartsWith ("4"))
				this.userInfo = Retry.Do (() => proxy.get_user_info (user, null));
			else
				this.userInfo = Retry.Do (() => proxy.get_user_info (user));
			return true;
		} catch (Exception) {
			return false;
		}
	}

	public int getPostImages (int postNum, ref List<ImageData> images)
	{
		UTF8Encoding enc = new UTF8Encoding ();
		String postContents = enc.GetString (thread.posts [postNum].post_content);
		String searchString = postContents.ToLower ();

		// Extract [quote][/qoute], note does not handle cases such as [quote][quote][/quote]..[/quote] correctly
		bool quoteRemoved;
		do
		{
			quoteRemoved = false;
			int startPos = searchString.IndexOf ("[quote]");
			int endPos = searchString.IndexOf ("[/quote]");
			if (startPos >= 0 && endPos >= 0 && endPos > startPos) 
			{
				postContents = postContents.Remove (startPos, endPos - startPos + 1);
				searchString = postContents.ToLower ();
				quoteRemoved = true;
			}
		} while(quoteRemoved);

		int searchPos = 0;
        int urlPos = 0;
        while (urlPos >= 0 && searchPos < searchString.Length)
        {
        	urlPos = searchString.IndexOf("[img]", searchPos);
        	if (urlPos >= 0)
        	{
            	int endPos = searchString.IndexOf("[/img]", urlPos);
	            if (endPos > 0)
    	        {
					String urlString = postContents.Substring(urlPos + 5, endPos - urlPos - 5);
					ImageData im;
					im.imageUrl = urlString;
					im.imageAuthor = enc.GetString(thread.posts[postNum].post_author_name);
					im.imagePost = postNum + this.loadedStart;
					images.Add(im);

					// TEST
					// TODO remove [img] in text, reformat [url]
					/*
					String preText;
					String postText;
					if(urlPos > 0)
					{
						preText = postContents.Substring(0, urlPos - 1);
						String[] rows = preText.Split('\n');
						int i = rows.Length;
						while(rows[i - 1].Length < 1 && i > 0)
							i--;
						i--;
						int lastRow = i;
						if(i >= 0)
						{
							while(rows[i].Length > 1 && i > 0)
								i--;
							int firstRow = i;

							preText = "";
							if(firstRow != lastRow || rows[firstRow].Length != 0)
							{
								for(i=firstRow; i <=lastRow; i++)
								{
									preText += rows[i];
								}
							}
						}
					}

					if(endPos < postContents.Length -1)
					{
						postText = postContents.Substring(endPos + 6, postContents.Length - endPos - 6);
						String[] rows = postText.Split('\n');
						int i = 0;
						while(rows[i].Length < 1 && i < rows.Length -1)
							i++;

						int lastRow = 0;
						if(i >= 0 && i < rows.Length)
						{
							while(rows[i].Length > 1 && i < rows.Length -1)
								i++;
							int firstRow = i;

							postText = "";
							if(firstRow != lastRow || rows[firstRow].Length != 0)
							{
								for(i=firstRow; i <=lastRow; i++)
								{
									postText += rows[i];
								}
							}
						}
					}*/
					// END TEST

                   	searchPos = urlPos + urlString.Length;
	            }
    	        else
        	    {
            		urlPos = -1;
                }
	        }
    	}

        if (thread.posts[postNum].attachments != null)
        {
	    	foreach (PostAttachmentData attachment in thread.posts[postNum].attachments)
            {
    	    	if (attachment.content_type.Equals("image"))
                {
					ImageData im;
					im.imageUrl = attachment.url;
					im.imageAuthor = enc.GetString(thread.posts[postNum].post_author_name);
					im.imagePost = postNum + this.loadedStart;
					images.Add (im);
                    //attachment.thumbnail_url;
                }
            }
        }
		return images.Count;
	}

	private String MD5Hash(String text)
	{
		MD5 md5 = new MD5CryptoServiceProvider();
 		md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
 		byte[] result = md5.Hash;
 
		StringBuilder strBuilder = new StringBuilder();
		for (int i = 0; i < result.Length; i++)
		{
			strBuilder.Append(result[i].ToString("x2"));
		}
 
		return strBuilder.ToString();
	}

	private String postToHtml (String post)
	{
		String outPost = replaceString(post, "[url=", "<a href=\"");
		outPost = replaceString(outPost, "[url]", "<a href=\"");
		outPost = replaceString(outPost, "[/url]", "</a>");
		outPost = replaceString(outPost, "[img]", "<br/><img src=\"");
		outPost = replaceString(outPost, "[/img]", "\"><br/>");
		outPost = replaceString(outPost, "[quote]", "<div class=\"quote\">");
		outPost = replaceString(outPost, "[/quote]", "</div>");
		outPost = replaceString(outPost, "\n", "<br/>");
		outPost = replaceString(outPost, "]", "\">");

		return outPost;
	}

	private string replaceString(string str, string oldValue, string newValue)
	{
    	StringBuilder sb = new StringBuilder();

	    int previousIndex = 0;
    	int index = str.IndexOf(oldValue, StringComparison.InvariantCultureIgnoreCase);
	    while (index != -1)
    	{
	    	sb.Append(str.Substring(previousIndex, index - previousIndex));
    		sb.Append(newValue);
    		index += oldValue.Length;

	    	previousIndex = index;
    		index = str.IndexOf(oldValue, index, StringComparison.InvariantCultureIgnoreCase);
	    }
    	sb.Append(str.Substring(previousIndex));

	    return sb.ToString();
	}
}