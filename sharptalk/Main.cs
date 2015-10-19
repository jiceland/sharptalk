using System;
using System.IO;
using Gtk;

namespace sharptalk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//if (!Directory.Exists (Environment.SpecialFolder.ApplicationData + "/sharptalk"))
			//	Directory.CreateDirectory (Environment.SpecialFolder.ApplicationData + "/sharptalk");
			//

			if (args.Length > 1) 
			{
				if(args[0].Equals("-d"))
					UserSettings.SettingsDir = args[1];
			}

			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}
