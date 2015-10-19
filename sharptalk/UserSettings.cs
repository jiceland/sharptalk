using System;
using System.Collections.Generic;
using System.IO;

public static class UserSettings
{
	public static String SettingsDir {
		get {
			return settingsDir;
		}
		set
		{
			settingsDir = value;
		}
	}

	static String fileName = "sharptalk-settings.txt";
	static String settingsDir = "";
	static List<String> settings = new List<String>();

	public static string getValue (String settingId)
	{
		if(settings.Count == 0)
			loadSettings();

		foreach (String setting in settings) 
		{
			if (setting.StartsWith (settingId + "="))
		    {
				return setting.Substring(settingId.Length + 1);
			}
		}
		return String.Empty;
	}

	public static int getNextSettingNum(String settingId)
	{
		if(settings.Count == 0)
			loadSettings();

		int counter = 1;

		bool settingFound;
		do {
			String searchSettingId = settingId + counter.ToString();
			settingFound = false;
			foreach (String setting in settings) 
			{
				if (setting.StartsWith (searchSettingId + "="))
		    	{
					settingFound = true;
					break;
				}
			}
			counter ++;
		} while(settingFound == true);

		return counter - 1;
	}

	public static void setValue (String settingId, String settingValue)
	{
		if (!File.Exists (System.IO.Path.Combine(settingsDir, fileName)))
			File.Create (System.IO.Path.Combine(settingsDir, fileName));

		bool settingFound = false;
		foreach (String setting in settings) 
		{
			if (setting.StartsWith (settingId + "=")) 
			{
				settingFound = true;
				settings.Remove(setting);
				break;
			}
		}

		settings.Add(settingId + '=' + settingValue);

		if (settingFound)
		{
			StreamWriter writer = new StreamWriter(System.IO.Path.Combine(settingsDir, fileName), false);
			foreach (String setting in settings) 
			{
				writer.WriteLine(setting);
			}
			writer.Close ();
		}
		else 
		{
			StreamWriter writer = new StreamWriter (System.IO.Path.Combine(settingsDir, fileName), true);
			writer.WriteLine (settingId + '=' + settingValue);
			writer.Close ();
		}
	}

	public static void clearValue(String settingId, int settingNum)
	{
		if(settings.Count == 0)
			loadSettings();

		StreamWriter writer = new StreamWriter(System.IO.Path.Combine(settingsDir, fileName), false);
		foreach (String setting in settings) 
		{
			String writeSetting = setting;
			if (setting.StartsWith (settingId))
			{
				// Do not rewrite setting
				if (setting.StartsWith (settingId + settingNum.ToString() + "=")
				 || setting.StartsWith (settingId + settingNum.ToString() + "."))
					continue;

				String[] settingParts = setting.Split('=');
				string foundSettingId;
				if(settingParts[0].Contains("."))
				{
					foundSettingId = (settingParts[0].Split('.'))[0];
				}
				else
					foundSettingId = settingParts[0];

				String remainder = setting.Substring(foundSettingId.Length);
				int foundSettingNum = int.Parse(foundSettingId.Substring(settingId.Length));
				if(foundSettingNum > settingNum)
					writeSetting = settingId + (foundSettingNum - 1).ToString() + remainder;
			}
			writer.WriteLine(writeSetting);
		}
		writer.Close();

		// Force reload
		loadSettings();
	}

	private static void loadSettings ()
	{
		settings.Clear ();

		if (File.Exists (System.IO.Path.Combine(settingsDir, fileName))) {
			StreamReader reader = new StreamReader(System.IO.Path.Combine(settingsDir, fileName));
			String setting;
			while ((setting = reader.ReadLine()) != null) {
				settings.Add (setting);
			}
			reader.Close ();
		}
	}
}

