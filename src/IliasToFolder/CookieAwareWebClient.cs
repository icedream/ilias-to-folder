// Idea: http://stackoverflow.com/questions/14551345/accept-cookies-in-webclient

using System;
using System.Net;

namespace IliasToFolder
{
	public class CookieAwareWebClient : WebClient
	{
		public CookieContainer CookieContainer { get; set; }
		public Uri Uri { get; set; }

		public CookieAwareWebClient()
			: this(new CookieContainer())
		{
		}

		public CookieAwareWebClient(CookieContainer cookies)
		{
			this.CookieContainer = cookies;
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);
			if (request is HttpWebRequest)
			{
				(request as HttpWebRequest).CookieContainer = this.CookieContainer;
			}
			HttpWebRequest httpRequest = (HttpWebRequest)request;
			httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			return httpRequest;
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			WebResponse response = base.GetWebResponse(request);
			String setCookieHeader = response.Headers[HttpResponseHeader.SetCookie];
			Console.WriteLine (setCookieHeader);

			if (setCookieHeader != null)
			{
				this.CookieContainer.SetCookies (new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority)), setCookieHeader);
			}

			return response;
		}
	}
}

