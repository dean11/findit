using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Xml;
using System.Timers;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace SitePing
{
	class FindIt
	{
		class HTTPData
		{
			public const string optionFrequency = "option_frequency";
			public const string optionWebcam = "option_webcam";
			public const string optionKeylogger = "option_keystrokes";
			public const string optionScreenshot = "option_screenshot";

			public const String requester = "requester=computer";
			public const String uri = "http://findit.dennisandersen.se/index.php";

			// Temporory variables ofcourse
			public const String user = "guest";
			public const String pass = "guest2016";
		}
		
		Logger logger = null;
		List<Cookie> cookies = null;
		Dictionary<String, String> attributes = null;
		int optionFrequency = 1000 * 60 * 5;
		bool optionWebcam = false;
		bool optionKeylogger = false;
		bool optionScreenshot = false;

		System.Timers.Timer pingTimer = new System.Timers.Timer();

		public FindIt()
		{
			this.cookies = new List<Cookie>();
			this.attributes = new Dictionary<String, String>();
			this.logger = Logger.CreateLogger(this.ToString(), "log/findit.log");
			this.pingTimer.Elapsed += PingTimerElapsed;
			this.pingTimer.AutoReset = false;
			this.pingTimer.Stop();
		}


		private static byte[] ImageToByte(Image img)
		{
			ImageConverter converter = new ImageConverter();
			return (byte[])converter.ConvertTo(img, typeof(byte[]));
		}


		private HttpWebResponse Login()
		{
			string postData = HTTPData.requester;
			postData += "&";
			postData += "user_name=" + HTTPData.user;
			postData += "&";
			postData += "user_password=" + HTTPData.pass;
			
			byte[] data = Encoding.UTF8.GetBytes(postData);


			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HTTPData.uri);
			request.Headers = new WebHeaderCollection();
			request.MaximumResponseHeadersLength = 15;
			request.Method = "POST";
			//request.Credentials = new NetworkCredential(HTTPData.user, HTTPData.pass);
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = data.Length;
			request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
			request.CookieContainer = new CookieContainer();
			for (int i = 0; i < this.cookies.Count; i++)
			{
				request.CookieContainer.Add(this.cookies[i]);
			}
				

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
			}

			HttpWebResponse webResp = null;
			try
			{
				WebResponse resp = request.GetResponse();
				webResp = (HttpWebResponse)resp;
			}
			catch (WebException e)
			{
				logger.debug(e.Status.ToString() + " : " + e.Message);
			}
			catch (ProtocolViolationException e)
			{
				logger.debug(e.Message);
			}
			catch (InvalidOperationException e)
			{
				logger.debug(e.Message);
			}
			catch (NotSupportedException e)
			{
				logger.debug(e.Message);
			}

			return webResp;
		}

		private void ExtractAttributes(HttpWebResponse response)
		{
			if (response == null)
			{
				return;
			}
			this.attributes.Clear();

			String responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
			responseString = responseString.Trim((new char[] { '\n', '\t', '\r' }));

			logger.debug(responseString);

			string[] attributes = responseString.Split(new char[] { '\n', '\r' });

			for (int i = 0; i < attributes.Length; i++)
			{
				string[] value = attributes[i].Split('=');
				if (value.Length == 2)
				{
					this.attributes[value[0]] = value[1];
				}
			}
		}

		private void UpdateCookies(HttpWebResponse response)
		{
			for (int i = 0; i < response.Cookies.Count; i++)
			{
				this.cookies.Add(response.Cookies[i]);
			}
		}

		void PingTimerElapsed(object sender, ElapsedEventArgs e)
		{
			this.pingTimer.Stop();
		}

		private void WaitForPing()
		{
			this.pingTimer.Interval = this.optionFrequency;
			this.pingTimer.Start();

			//Temporary wait solution..
			while (this.pingTimer.Enabled)
			{
				Thread.Yield();
				Thread.Sleep(2000);
			}
		}

		Geocoordinate GetCoordinates()
		{
			Geocoordinate coord = null;
			try
			{
				Geolocator geolocator = new Geolocator();
				IAsyncOperation<Geoposition> ap = geolocator.GetGeopositionAsync();

				while (ap.Status != Windows.Foundation.AsyncStatus.Completed)
				{
					if (ap.Status == Windows.Foundation.AsyncStatus.Canceled)
					{
						logger.debug("CANCELED");
					}
					else if (ap.Status == Windows.Foundation.AsyncStatus.Error)
					{
						logger.debug("ERROR");
					}
					Thread.Sleep(1000);
				}

				Geoposition p = ap.GetResults();
				coord = p.Coordinate;
				double longitude = p.Coordinate.Longitude;
				double latitude = p.Coordinate.Latitude;
				double accuracy = p.Coordinate.Accuracy;

				logger.debug("Accuracy: " + accuracy.ToString());
				logger.debug("Longitude: " + longitude.ToString());
				logger.debug("Latitude: " + latitude.ToString());
			}
			catch (UnauthorizedAccessException e)
			{
				logger.debug("UnauthorizedAccessException" + e.Message);
			}
			catch (Exception e)
			{
				logger.debug("Exception" + e.Message);
			}

			return coord;
		}

		private void UpdateOptions()
		{
			string postData = HTTPData.requester;
			postData += "&action=getOptions";

			byte[] data = Encoding.UTF8.GetBytes(postData);


			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HTTPData.uri);
			request.Headers = new WebHeaderCollection();
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = data.Length;
			request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
			request.CookieContainer = new CookieContainer();
			for (int i = 0; i < this.cookies.Count; i++)
			{
				request.CookieContainer.Add(this.cookies[i]);
			}

			try
			{
				using (Stream stream = request.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}
			}
			catch (Exception e)
			{
				logger.debug(e.Message);
				return;
			}
			

			HttpWebResponse webResp = null;
			try
			{
				WebResponse resp = request.GetResponse();
				webResp = (HttpWebResponse)resp;
			}
			catch (WebException e)
			{
				logger.debug(e.Status.ToString() + " : " + e.Message);
			}
			catch (ProtocolViolationException e)
			{
				logger.debug(e.Message);
			}
			catch (InvalidOperationException e)
			{
				logger.debug(e.Message);
			}
			catch (NotSupportedException e)
			{
				logger.debug(e.Message);
			}

			this.ExtractAttributes(webResp);


			//Frequency
			if (!(this.attributes.ContainsKey(HTTPData.optionFrequency) && int.TryParse(this.attributes[HTTPData.optionFrequency], out this.optionFrequency)))
			{
				this.optionFrequency = Convert.ToInt32(this.attributes[HTTPData.optionFrequency]);
			}
			//Webcam
			if (this.attributes.ContainsKey(HTTPData.optionWebcam) )
			{
				this.optionWebcam = Convert.ToBoolean(Convert.ToInt32(this.attributes[HTTPData.optionWebcam]));
			}
			//screenshot
			if (this.attributes.ContainsKey(HTTPData.optionScreenshot))
			{
				this.optionScreenshot = Convert.ToBoolean(Convert.ToInt32(this.attributes[HTTPData.optionScreenshot]));
			}
			//Keylogger
			if (this.attributes.ContainsKey(HTTPData.optionKeylogger))
			{
				this.optionKeylogger = Convert.ToBoolean(Convert.ToInt32(this.attributes[HTTPData.optionKeylogger]));
			}

		}

		private bool PostData()
		{
			Geocoordinate coord = GetCoordinates();

			string postData = HTTPData.requester;
			postData += "&action=postData";
			postData += "&longitude=" + coord.Longitude.ToString();
			postData += "&latitude=" + coord.Latitude.ToString();
			postData += "&accuracy=" + coord.Accuracy.ToString();

			byte[] data = Encoding.UTF8.GetBytes(postData);


			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HTTPData.uri);
			request.Headers = new WebHeaderCollection();
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = data.Length;
			request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
			request.CookieContainer = new CookieContainer();
			for (int i = 0; i < this.cookies.Count; i++)
			{
				request.CookieContainer.Add(this.cookies[i]);
			}

			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
			}

			HttpWebResponse webResp = null;
			try
			{
				WebResponse resp = request.GetResponse();
				webResp = (HttpWebResponse)resp;
			}
			catch (WebException e)
			{
				logger.debug(e.Status.ToString() + " : " + e.Message);
			}
			catch (ProtocolViolationException e)
			{
				logger.debug(e.Message);
			}
			catch (InvalidOperationException e)
			{
				logger.debug(e.Message);
			}
			catch (NotSupportedException e)
			{
				logger.debug(e.Message);
			}

			return true;
		}

		private void ProcDesktopSnapshot()
		{
			if (this.optionScreenshot)
			{
				//Create a new bitmap.
				Bitmap bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
											   Screen.PrimaryScreen.Bounds.Height,
											   PixelFormat.Format32bppArgb);

				// Create a graphics object from the bitmap.
				Graphics screenshot = Graphics.FromImage(bmpScreenshot);

				// Take the screenshot from the upper left corner to the right bottom corner.
				screenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
											Screen.PrimaryScreen.Bounds.Y,
											0,
											0,
											Screen.PrimaryScreen.Bounds.Size,
											CopyPixelOperation.SourceCopy);

				// Save the screenshot to the specified path that the user has chosen.
				//bmpScreenshot.Save("Screenshot.png", ImageFormat.Png);
				byte[] data = ImageToByte(bmpScreenshot);

				return;

				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HTTPData.uri);
				request.Headers = new WebHeaderCollection();
				request.MaximumResponseHeadersLength = 15;
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = data.Length;
				request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
				request.CookieContainer = new CookieContainer();
				for (int i = 0; i < this.cookies.Count; i++)
				{
					request.CookieContainer.Add(this.cookies[i]);
				}

				using (Stream stream = request.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}

				HttpWebResponse webResp = null;
				try
				{
					WebResponse resp = request.GetResponse();
					webResp = (HttpWebResponse)resp;
				}
				catch (WebException e)
				{
					logger.debug(e.Status.ToString() + " : " + e.Message);
				}
			}
		}

		public bool Run()
		{
			HttpWebResponse response = Login();
			ExtractAttributes(response);
			if (this.attributes.Count > 0 && this.attributes["loggedIn"] != "true")
			{
				return false;
			}

			UpdateCookies(response);

			while (true)
			{
				UpdateOptions();

				PostData();
				
				WaitForPing();

				response = Login();

				if (this.attributes.Count > 0 && this.attributes.ContainsKey("loggedIn") && this.attributes["loggedIn"] != "true")
				{
					ExtractAttributes(response);
					continue;
				}
				
			}
		}
	}
}
