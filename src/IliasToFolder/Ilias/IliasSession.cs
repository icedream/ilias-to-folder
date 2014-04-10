using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using HtmlAgilityPack;

namespace IliasToFolder
{
	public class IliasSession
	{
		private CookieAwareWebClient _webClient;

		private string _iliasProtocol = "https";
		private string _iliasHost = "ilias3.uni-stuttgart.de";
		private ushort _iliasPort = 443;
		private string _iliasPath = "/";
		private string _iliasClientId = "Uni_Stuttgart";
		private string _iliasLang = "de";

		public IliasSession ()
		{
			_webClient = new CookieAwareWebClient ();
		}

		protected Uri LoginUri {
			get { return GetIliasRequestUri ("post", "ilstartupgui", "o6", "ilStartupGUI"); }
		}

		protected Uri LoginPageUri {
			get { return GetRequestUri ("login.php"); }
		}

		protected static string BuildQueryString (Dictionary<string, string> collection)
		{
			// We use .NET Framework's internal implementation of HttpValueCollection here
			// which in fact is a derivation of the NameValueCollection but we can't create
			// an instance of it with "new", instead we let the built-in functions do the
			// job for us. Dirty trick but required to work sanely with the stuff.
			var values = HttpUtility.ParseQueryString (string.Empty);

			// Copy everything over to the HttpValueCollection
			foreach (var i in collection)
				values.Add (i.Key, i.Value);

			// This will convert the NameValueCollection into a URI query string since
			// HttpValueCollection overrides ToString() with a proper implementation of
			// a query builder.
			return values.ToString ();
		}

		protected Uri GetRequestUri (string path, Dictionary<string, string> data = null)
		{
			if (data == null)
				data = new Dictionary<string, string> ();

			UriBuilder ub = new UriBuilder () {
				Host = _iliasHost,
				Scheme = _iliasProtocol,
				Port = _iliasPort,
				Path = string.Format("{0}/{1}", _iliasPath, path),
				Query = BuildQueryString(data)
			};

			return ub.Uri;
		}

		protected Uri GetIliasRequestUri (string command, string commandClass, string commandNode, string baseClass, Dictionary<string, string> data = null, string rtoken = "")
		{
			if (data == null)
				data = new Dictionary<string, string> ();

			// Building the GET data
			var outputGetData = new Dictionary<string, string> () {
				{ "lang", _iliasLang },
				{ "client_id", _iliasClientId },
				{ "cmd", command },
				{ "cmdClass", commandClass },
				{ "cmdNode", commandNode },
				{ "baseClass", baseClass },
				{ "rtoken", rtoken }
			};
			foreach (var item in data)
				outputGetData.Add (item.Key, item.Value);

			return GetRequestUri ("login.php", outputGetData);
		}

		// TODO: Implement exceptions for Login
		public bool Login (string username, string password)
		{
			// Visit the login page. We will receive a cookie with an auth challenge in it
			// which the server will use to validate our login request.
			try {
				_webClient.DownloadString (LoginPageUri);
				// TODO: Check if we even received an authchallenge cookie. If not, exception (see above todo).
			} catch (Exception e) {
				Console.Error.WriteLine("Exception while downloading login cookie");
				Console.Error.WriteLine (e);
				return false;
			}

			// We send over our stuff
			var responseHtml = _webClient.UploadString (
				LoginUri,
				BuildQueryString (new Dictionary<string, string> {
					{ "username", username },
					{ "password", password }
				})
			);

			// Use HtmlAgilityPack to parse the HTML properly
			HtmlDocument doc = new HtmlDocument ();
			doc.LoadHtml (responseHtml);

			// Check if login was successful
			var errorNode = doc.DocumentNode.SelectSingleNode ("//*[@id=\"il_startup_content\"]/div[1]");
			if (errorNode != null && errorNode.InnerText.Trim() != string.Empty) {
				Console.Error.WriteLine (errorNode.InnerText);
				return false; // wrong username or password
			}

			Console.WriteLine (responseHtml);
			Console.ReadKey ();

			return true;
		}

		public void Logout()
		{
		}
	}
}

