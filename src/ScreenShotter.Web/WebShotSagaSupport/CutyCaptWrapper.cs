using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace ScreenShotter.Web.WebShotSagaSupport
{
	// from: http://nolovelust.com/post/Contribution-to-C-Website-Screenshot-Generator-AKA-Get-Screenshot-of-Webpage-With-Aspnet-C.aspx
	// but, really, this code isn't very nice... Let's fix that later.

	public class CutyCaptWrapper
	{
		///1 - small
		///2 - medium
		///3 - large
		private int ThumbNailSize { get; set; }

		public bool ThumbKeepAspectRatio { get; set; }
		public int ThumbExpiryTimeInHours { get; set; }
		public string ScreenShotPath { get; set; }
		public string CutyCaptPath { get; set; }
		//public string CutyCaptWorkingDirectory { get; set; }
		public string CutyCaptDefaultArguments { get; set; }
		public int ThumbWidth { get; set; }
		public int ThumbMaxHeight { get; set; }

		public CutyCaptWrapper()
		{
			//default values
			ThumbNailSize = 1;
			ThumbKeepAspectRatio = false;
			ThumbExpiryTimeInHours = 168; //1 week
			ScreenShotPath = HttpContext.Current.Server.MapPath("~/ThumbCache/"); // must be within the web root
			CutyCaptPath = HttpContext.Current.Server.MapPath("~/App_Data/CutyCapt.exe"); // must be within the web root
			//CutyCaptWorkingDirectory = HttpContext.Current.Server.MapPath("~/App_Data/");
			CutyCaptDefaultArguments =
				" --max-wait=10000 --out-format=jpg --javascript=off --java=off --plugins=off --js-can-open-windows=off --js-can-access-clipboard=off --private-browsing=on";
		}

		///Checks if there is a cached screenshot of the website and returns url path to thumbnail of the website in order to use ase html image element source
		///Usage example:
		///&lt;img src="&lt;%=CutyCaptWrapper().GetScreenShot("http://google.com")%%gt;" alt=""&gtr;
		public string GetScreenShot(string url)
		{
			if (IsURLValid(url))
			{
				if (!Directory.Exists(ScreenShotPath))
				{
					Directory.CreateDirectory(ScreenShotPath);
				}
				//set thumbnail sizes
				//SetThumbnailSize();
				var ScreenShotFileName = ScreenShotPath + GetScreenShotFileName(url);
				var ScreenShotThumbnailFileName = ScreenShotPath +
				                                  GetScreenShotThumbnailFileName(ScreenShotFileName, ThumbWidth, ThumbMaxHeight);
				var RunArguments = " --url=" + url + " --out=" + ScreenShotFileName + CutyCaptDefaultArguments;

				var ScreenShotThumbnailFileNameInfo = new FileInfo(ScreenShotThumbnailFileName);

				if (!ScreenShotThumbnailFileNameInfo.Exists ||
				    ScreenShotThumbnailFileNameInfo.CreationTime < DateTime.Now.AddHours(-ThumbExpiryTimeInHours))
				{
					var info = new ProcessStartInfo(CutyCaptPath, RunArguments);
					info.UseShellExecute = false;
					info.RedirectStandardInput = true;
					info.RedirectStandardError = true;
					info.RedirectStandardOutput = true;
					info.CreateNoWindow = true;
					//info.WorkingDirectory = CutyCaptWorkingDirectory;
					using (var scr = Process.Start(info))
					{
						//string output = scr.StandardOutput.ReadToEnd();
						scr.WaitForExit();
						//return output;
					}
					//ThumbnailCreate(ScreenShotFileName, ScreenShotThumbnailFileName, ThumbWidth, ThumbMaxHeight, ThumbKeepAspectRatio);
					////delete original file
					//File.Delete(ScreenShotFileName);
				}
				return GetRelativeUri(ScreenShotThumbnailFileName);
			}
			else
			{
				return "Wrong URL";
			}
		}

		private void ThumbnailCreate(string sourceFilePath, string outFilePath, int NewWidth, int MaxHeight,
		                             bool keepAspectRatio)
		{
			using (Image FullsizeImage = Image.FromFile(sourceFilePath))
			{
				var NewHeight = MaxHeight;
				if (keepAspectRatio)
				{
					NewHeight = FullsizeImage.Height*NewWidth/FullsizeImage.Width;
					if (NewHeight > MaxHeight)
					{
						NewWidth = FullsizeImage.Width*MaxHeight/FullsizeImage.Height;
						NewHeight = MaxHeight;
					}
				}
				using (Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero))
				{
					NewImage.Save(outFilePath, ImageFormat.Png);
				}
			}
		}

		private string GetScreenShotFileName(string url)
		{
			var uri = new Uri(url);
			return uri.Host.Replace(".", "_") + uri.LocalPath.Replace("/", "_") + ".png";
		}

		private string GetScreenShotThumbnailFileName(string sourceFilename, int width, int height)
		{
			var sourceFile = new FileInfo(sourceFilename);
			var shortFilename = sourceFile.Name;
			var ext = Path.GetExtension(shortFilename);
			var replacementEnding = String.Format("{0}x{1}", width, height) + ext;
			return shortFilename.Replace(ext, replacementEnding);
		}

		private string GetRelativeUri(string pathToFile)
		{
			var rootPath = HttpContext.Current.Server.MapPath("~");
			return pathToFile.Replace(rootPath, "").Replace(@"\", "/");
		}

		private bool IsURLValid(string url)
		{
			var strRegex = "^(https?://)"
			               + "?(([0-9a-z_!~*'().&=+$%-]+: )?[0-9a-z_!~*'().&=+$%-]+@)?" //user@
			               + @"(([0-9]{1,3}\.){3}[0-9]{1,3}" // IP- 199.194.52.184
			               + "|" // allows either IP or domain
			               + @"([0-9a-z_!~*'()-]+\.)*" // tertiary domain(s)- www.
			               + @"([0-9a-z][0-9a-z-]{0,61})?[0-9a-z]\." // second level domain
			               + "[a-z]{2,6})" // first level domain- .com or .museum
			               + "(:[0-9]{1,4})?" // port number- :80
			               + "((/?)|" // a slash isn't required if there is no file name
			               + "(/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+/?)$";
			var re = new Regex(strRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

			if (re.IsMatch(url))
				return (true);
			else
				return (false);
		}

		private void SmallThumbnail()
		{
			ThumbWidth = 200;
			ThumbMaxHeight = 150;
		}

		private void MediumThumbnail()
		{
			ThumbWidth = 240;
			ThumbMaxHeight = 190;
		}

		private void LargeThumbnail()
		{
			ThumbWidth = 320;
			ThumbMaxHeight = 270;
		}

		private void Res_1024x768()
		{
			ThumbWidth = 1024;
			ThumbMaxHeight = 5000;
		}

		private void Res_1280x1024()
		{
			ThumbWidth = 1280;
			ThumbMaxHeight = 5000;
		}

		private void Res_1920x1200()
		{
			ThumbWidth = 1920;
			ThumbMaxHeight = 5000;
		}

		private void SetThumbnailSize()
		{
			if (ThumbNailSize == 1)
			{
				SmallThumbnail();
			}
			if (ThumbNailSize == 2)
			{
				MediumThumbnail();
			}
			if (ThumbNailSize == 3)
			{
				LargeThumbnail();
			}
			if (ThumbNailSize == 1024)
			{
				Res_1024x768();
			}
			if (ThumbNailSize == 1280)
			{
				Res_1280x1024();
			}
			if (ThumbNailSize == 1920)
			{
				Res_1920x1200();
			}
			else
			{
				ThumbNailSize = 1;
			}
		}
	}
}