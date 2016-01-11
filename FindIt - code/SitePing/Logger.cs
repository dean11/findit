using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitePing
{
	class Logger
	{
		enum LEVEL
		{
			LEVEL_OFF,
			LEVEL_INFO,
			LEVEL_WARNING,
			LEVEL_DEBUG,
		}
		private static Dictionary<String, Logger> loggers = new Dictionary<string,Logger>();
		private string path = "";
		LEVEL level = LEVEL.LEVEL_DEBUG;

		public static Logger CreateLogger(string clazz, string file)
		{
			if(!loggers.ContainsKey(clazz))
			{
				loggers.Add(clazz, new Logger(file));
			}

			return loggers[clazz];
		}

		private Logger(string path)
		{
			this.path = path;
		}
		private void write(string line)
		{
			string dir = Path.GetDirectoryName(path);
			if(!Directory.Exists(dir))
			{
				DirectoryInfo inf = Directory.CreateDirectory(dir);
			}
			if (!File.Exists(this.path))
			{
				try
				{
					File.Create(this.path).Close();
				}
				catch (Exception)
				{}
			}

			using (StreamWriter file = new StreamWriter(this.path, true))
			{
				file.WriteLine(line);
			}
		}
		
		public void info(String text)
		{
			if (level < LEVEL.LEVEL_INFO)
			{
				return;
			}
			String s = "";
			s += "INFO";
			s += " - ";
			s += DateTime.Now.ToLongTimeString();
			s += " :: ";
			s += text;

			this.write(s);
		}
		public void warning(String text)
		{
			if (level < LEVEL.LEVEL_WARNING)
			{
				return;
			}
			String s = "";
			s += "WARNING";
			s += " - ";
			s += DateTime.Now.ToLongTimeString();
			s += " :: ";
			s += text;

			this.write(s);
		}
		public void debug(String text)
		{
			if (level < LEVEL.LEVEL_DEBUG)
			{
				return;
			}
			String s = "";
			s += "DEBUG";
			s += " - ";
			s += DateTime.Now.ToLongTimeString();
			s += " :: ";
			s += text;

			this.write(s);
		}
	}
}
