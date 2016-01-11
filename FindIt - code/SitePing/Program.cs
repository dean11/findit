using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Xml;

namespace SitePing
{
	class Program
	{
		static void Main(string[] args)
		{
			FindIt findit = new FindIt();

			findit.Run();
			
		}
	}
}
