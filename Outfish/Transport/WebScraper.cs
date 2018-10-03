using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Outfish {

	/// <summary>
	/// A web scraping utility designed to simplify web scraping, parsing, and validation.
	/// Primary abilities are: 
	///    - getting a WebResponse
	///    - getting a WebPage
	///    - saving a webpage to a file.
	///    - generating an event when transprort or parse/validation exception occurs.
	/// ExceptionEvents and exceptions will all contain the REQUEST and CONTENT in the Data dictionary
	/// </summary>
	public class WebScraper : IWebScraper {

		#region constructor

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We don't declare this static object so we can't initialize it there.")]
		static WebScraper() {
			ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback( ValidateServerCertificate );
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 
			//	| SecurityProtocolType.Tls11 
			//	| SecurityProtocolType.Tls; // comparable to modern browsers
			//System.Net.ServicePointManager.Expect100Continue = false;
		}

		/// <summary>
		/// Creates a new ScreenScraper object.
		/// </summary>
		public WebScraper():this(10*1000) {}

		/// <summary>
		/// Creates a new ScreenScraper object.
		/// </summary>
		/// <param name="defaultTimeoutMs">the time out to use when one isn't provided with the request</param>
		public WebScraper(int defaultTimeoutMs) {

			this.Defaults = new RequestSettings {
				Timeout = defaultTimeoutMs,
				KeepAlive = true,
				ProtocolVersion = new Version("1.1"),
				Accept = "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.9,image/png,image/webp,*/*;q=0.8",
				UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.1.3) Gecko/20070309 Firefox/2.0.0.3",
				Headers = new Dictionary<string, string>{
					{"Accept-Language","en-us,en;q=0.8"},
					{"Accept-Charset","ISO-8859-1,utf-8;q=0.7,*;q=0.7"},
				},
			};

			this.CachePolicy = new HttpRequestCachePolicy( HttpRequestCacheLevel.NoCacheNoStore ); // might be better to use BypassCache instead
			this.ResponseReader = new ResponseReader();
			this.MaxForwards = 3;

		}

		#endregion

		public RequestSettings Defaults { get; set; }

		#region public Configuration

		/// <summary>
		/// The caching policy used by the ScreenScraper, 
		/// Defaults to NoCacheNoStore
		/// Overwrites HttpWebRequestDefaultCachePolicy
		/// </summary>
		public HttpRequestCachePolicy CachePolicy { get; set; }

		/// <summary>
		/// Use default .NET redirection mechanism (that misses some cookies.)
		/// When this value is false (default), the scraper will perform the redirection updating the cookies itself.
		/// </summary>
		public bool UseFrameworkRedirect {get; set;}

		/// <summary>
		/// Gets/sets the object that knows how to read the response's content type and equivalent string.
		/// </summary>
		public ResponseReader ResponseReader{ get; set; }

		/// <summary>
		/// If true, WebException with Response will swallow the exception and just returns the response text
		/// If false, throws the WebException with Data["CONTENT"] and Data["REQUEST"] 
		/// </summary>
		public bool ReturnWebExceptionResponse{ get; set; }
		
		/// <summary>
		/// Get/Set the maximum forwards/redirections allowed before we throw an exception.
		/// </summary>
		public int MaxForwards { get; set; }

		public bool IgnoreRedirection { get; set; }

		/// <summary> The # of historic items to track.  Defaults to 0. </summary>
		public int MaxHistory { get; set; } = 0;

		#endregion
		
		#region public Cookies

		/// <summary> The cookies for the session. </summary>
		public CookieContainer CookieJar {
			get { return this._cookieJar; }
			set { if(value == null) throw new ArgumentNullException(nameof(CookieJar),"can't set cookiejar to null.");
				_cookieJar = value;
			}
		}

		#endregion

		#region public Get / Write methods

		/// <summary>
		/// Gets response body as a string.
		/// wraps any WebException with a ScrapeException
		/// </summary>
		public virtual string GetResponseBody( ScrapeRequest request ) {

			try {
				using( HttpWebResponse response = this.GetFinalWebResponseWithRedirects( request ) ) {
					return this.ReadNonNullResponse( response );
				}
			} catch(WebException wex){

				string content = ReadPossibleNullResponse( wex.Response );
				if( content != null && this.ReturnWebExceptionResponse )
					return content;
				throw new ScrapeException( wex, request, content );
			}

		}

		/// <summary>
		/// Gets response body as a string.
		/// wraps any WebException with a ScrapeException
		/// </summary>
		/// <returns>a string.  Never null.</returns>
		public virtual async Task<string> GetResponseBodyAsync( ScrapeRequest request ) {

			try {
				using( HttpWebResponse response = await this.GetFinalWebResponseWithRedirectsAsync( request ) ) {
					return await ReadNonNullResponseAsync( response );
				}
			} catch(WebException wex){

				string content = ReadPossibleNullResponse( wex.Response );
				if( content != null && this.ReturnWebExceptionResponse )
					return content;
				throw new ScrapeException( wex, request, content );
			}

		}

		/// <summary>
		/// wraps any WebException with a ScrapeException
		/// throws ParseException if problem parsing HtmlDocument
		/// </summary>
		/// <remarks>
		/// Thin wrapper around GetResponseBody().
		/// This isn't virtual because you can override GetResponseBody.
		/// </remarks>
		public HtmlDocument GetDocument( ScrapeRequest request ) {
			string responseBody = this.GetResponseBody( request );
			return new HtmlDocument( responseBody, request.Uri ); // no need to try/catch because parsing is deferred.
		}

		/// <summary>
		/// wraps any WebException with a ScrapeException
		/// throws ParseException if problem parsing HtmlDocument
		/// </summary>
		/// <remarks>
		/// Thin wrapper around GetResponseBodyAsync().
		/// This isn't virtual because you can override GetResponseBodyAsync.
		/// </remarks>
		public async Task<HtmlDocument> GetDocumentAsync( ScrapeRequest request ) {
			string responseBody = await this.GetResponseBodyAsync( request );
			return new HtmlDocument( responseBody ); // no need to try/catch because parsing is deferred.
		}

		/// <summary>
		/// wraps any WebException with a ScrapeException
		/// </summary>
		/// <summary>Writes the response to a Stream</summary>
		/// <returns>Response content type of successful attempts, or response body of unsuccessful attempts..</returns>
		public virtual string WriteResponseToStream( ScrapeRequest request, Stream oStream ){

			try {
				using( var response = this.GetFinalWebResponseWithRedirects( request ) ) {
					using( var responseStream = response.GetResponseStream() )
						responseStream.CopyTo(oStream);
					return response.ContentType;
				}
			}
			catch(WebException wex){
				string responseBody = ReadPossibleNullResponse( wex.Response );
				if( this.ReturnWebExceptionResponse ) return responseBody;
				throw new ScrapeException( wex, request, responseBody );
			}

		}

		/// <summary>
		/// Writes the response to a Stream
		/// wraps any WebException with a ScrapeException
		/// </summary>
		/// <returns>Response content type of successful attempts, or response body of unsuccessful attempts..</returns>
		public virtual async Task<string> WriteResponseToStreamAsync( ScrapeRequest request, Stream oStream ){

			try {
				using( var response = await this.GetFinalWebResponseWithRedirectsAsync( request ) ) {
					using( var responseStream = response.GetResponseStream() )
						await responseStream.CopyToAsync(oStream);
					return response.ContentType;
				}
			}
			catch(WebException wex){
				string responseBody = ReadPossibleNullResponse( wex.Response );
				if( this.ReturnWebExceptionResponse ) return responseBody;
				throw new ScrapeException( wex, request, responseBody );
			}

		}

		/// <summary>
		/// Saves the document retrieved from the web response to a flie.
		/// wraps any WebException with a ScrapeException
		/// </summary>
		/// <returns>HTML formatted Content type (image/jpeg, application/xml, etc)</returns>
		/// <example>SaveResponse("http://www.google.com/", null, 10000, "C:\\temp\\temp.html", new Regex("www.google.com"),null );</example>
		public virtual string WriteResponseToFile( ScrapeRequest request, string fileName ) {

			using( System.IO.FileStream fileStream = new FileStream(fileName, FileMode.Append) ){
				return this.WriteResponseToStream( request, fileStream );
			} // no need to try/catch because GetResponseBody does that for us.

		}

		/// <summary>
		/// Saves the document retrieved from the web response to a flie.
		/// wraps any WebException with a ScrapeException
		/// </summary>
		/// <returns>HTML formatted Content type (image/jpeg, application/xml, etc)</returns>
		/// <example>SaveResponse("http://www.google.com/", null, 10000, "C:\\temp\\temp.html", new Regex("www.google.com"),null );</example>
		public virtual async Task<string> WriteResponseToFileAsync( ScrapeRequest request, string fileName ) {

			using( System.IO.FileStream fileStream = new FileStream(fileName, FileMode.Append) ){
				return await this.WriteResponseToStreamAsync( request, fileStream );
			} // no need to try/catch because GetResponseBody does that for us.

		}

		#endregion

		#region private static

		void SaveCookies( HttpWebResponse response ){
			// save cookies (for authentication)
			if(response.Cookies != null)
				foreach(Cookie c in response.Cookies)
					if(!c.Expired)
						this._cookieJar.Add(c);

			this._cookieJar.Add(WebScraper.GetMissingCookies(response));
		}

		/// <summary>
		/// puts cookies in the header that httpwebresponse failed to process properly
		/// </summary>
		/// <remarks>finds cookies in the response from Countrywide that HttpWebRequest failed to detect.
		/// County wide uses a .net.blahblahblah cookie that isn't detected, so we need to find in manually</remarks>
		/// <param name="response"></param>
		/// <returns></returns>
		static CookieCollection GetMissingCookies(HttpWebResponse response) {

			// stores cookies that are not detected by response
			CookieCollection cc = new CookieCollection();

			string cookies_str = response.GetResponseHeader("Set-Cookie");

			//   res_countrywide_com=anakincookieookie;  domain=.countrywide.com; expires=Wed, 29-Mar-2006 21:01:06 GMT; path=/,			
			MatchCollection matches = Regex.Matches(cookies_str, @"(.*?)=(.*?);\s?domain=(.*?);\s?expires=(.*?);\s?path=([^,]*),?");

			foreach(Match match in matches) {
				string name = match.Groups[1].Value;
				string value = match.Groups[2].Value;
				string domain = match.Groups[3].Value;
				string expires = match.Groups[4].Value;
				string path = match.Groups[5].Value;

				Cookie c = new Cookie(name, value, path, domain);
				c.Expires = DateTime.Parse(expires);

				if(response.Cookies[c.Name] == null && !c.Expired) {
					cc.Add(c);
				}
			}

			return cc;
		}

		/// <summary>
		/// invoked by the RemoteCertificateValidationDelegate to accept all certificates
		/// </summary>
		static bool ValidateServerCertificate(
			  object sender,
			  System.Security.Cryptography.X509Certificates.X509Certificate certificate,
			  System.Security.Cryptography.X509Certificates.X509Chain chain,
			  System.Net.Security.SslPolicyErrors sslPolicyErrors
		) {
			return true; // accept all
		}

		#endregion

		#region private methods

		/// <summary>
		/// Wraps the call to the ResponseREader.ReadAllText to ensure derived/passed in readers don't return null.
		/// </summary>
		string ReadNonNullResponse( HttpWebResponse response ) {
			if( response == null ) throw new ArgumentNullException( nameof( response ) );
			string responseBody = this.ResponseReader.ReadAllText( response );
			if( responseBody != null ) return responseBody;
			throw new InvalidOperationException( "ResponseReader returned null response body." ); // won't ever happen unless ResponseReader has bug.
		}

		/// <summary>
		/// Wraps the call to the ResponseREader.ReadAllTextAsync to ensure derived/passed in readers don't return null.
		/// </summary>
		async Task<string> ReadNonNullResponseAsync( HttpWebResponse response ) {
			if( response == null ) throw new ArgumentNullException( nameof( response ) );
			string responseBody = await this.ResponseReader.ReadAllTextAsync( response );
			if( responseBody != null ) return responseBody;
			throw new InvalidOperationException( "ResponseReader returned null response body." ); // won't ever happen unless ResponseReader has bug.
		}

		string ReadPossibleNullResponse( WebResponse response ) {
			var httpResponse = response as HttpWebResponse;
			return httpResponse != null 
				? ReadNonNullResponse( httpResponse ) 
				: null;
		}

		async Task<string> ReadResponseAsync( WebResponse response ) {
			var httpResponse = response as HttpWebResponse;
			return httpResponse != null 
				? await ReadNonNullResponseAsync( httpResponse ) 
				: null;
		}

		/// <summary>
		/// Performs redirects as needed and returns the response of the last page.
		/// </summary>
		HttpWebResponse GetFinalWebResponseWithRedirects( ScrapeRequest scrapeRequest ) {
			ScrapeRequest currentScrapeRequest = scrapeRequest; // changes value, doesn't feel right to change parameter

			HttpWebResponse response = null;
			try {

				response = GetSimpleResponse( currentScrapeRequest );

				int tries = this.MaxForwards;
				while( !IgnoreRedirection && response.Headers[ "Location" ] != null ) {
					if( --tries == 0 )
						throw new System.Net.WebException( "Too many page forwarding" );

					// build next request from response
					currentScrapeRequest = new ScrapeRequest( new Uri( currentScrapeRequest.Uri, response.Headers[ "Location" ] ) );

					// close request
					response.Close();
					response = null;

					response = GetSimpleResponse( currentScrapeRequest );

				}

				return response;

			} catch {
				// cleanup response if this is an exception only
				if(response != null) {
					response.Close();
					response = null;
				}
				throw;
			}

		}

		/// <summary>
		/// Performs redirects as needed and returns the response of the last page.
		/// </summary>
		async Task<HttpWebResponse> GetFinalWebResponseWithRedirectsAsync( ScrapeRequest scrapeRequest ) {
			ScrapeRequest currentScrapeRequest = scrapeRequest; // changes value, doesn't feel right to change parameter

			HttpWebResponse response = null;
			try {

				response = await GetSimpleResponseAsync( currentScrapeRequest );

				int tries = this.MaxForwards;
				while( !IgnoreRedirection && response.Headers[ "Location" ] != null ) {
					if( --tries == 0 )
						throw new System.Net.WebException( "Too many page forwarding" );

					// build next request from response
					currentScrapeRequest = new ScrapeRequest( new Uri( currentScrapeRequest.Uri, response.Headers[ "Location" ] ) );

					// close request
					response.Close();
					response = null;

					response = await GetSimpleResponseAsync( currentScrapeRequest );

				}

				return response;

			} catch {
				// cleanup response if this is an exception only
				if(response != null) {
					response.Close();
					response = null;
				}
				throw;
			}

		}

		HttpWebResponse GetSimpleResponse( ScrapeRequest scrapeRequest ) {
			HttpWebRequest httpWebRequest = this.BuildHttpWebRequest( scrapeRequest );
			var response = httpWebRequest.GetResponse();
			return TrackResponseAndPromote( response );
		}

		async Task<HttpWebResponse> GetSimpleResponseAsync( ScrapeRequest scrapeRequest ) {
			HttpWebRequest httpWebRequest = this.BuildHttpWebRequest( scrapeRequest );
			var response = await httpWebRequest.GetResponseAsync();
			return TrackResponseAndPromote( response );
		}

		HttpWebResponse TrackResponseAndPromote( WebResponse response ) {
			var httpResponse = (HttpWebResponse)response;
			this._lastPage = response.ResponseUri.AbsoluteUri;
			this.SaveCookies( httpResponse );
			return httpResponse;
		}

		HttpWebRequest BuildHttpWebRequest( ScrapeRequest request ) {

			// build next request
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create( request.Uri );
			httpRequest.AllowAutoRedirect = this.UseFrameworkRedirect;  // we can do this manually
			httpRequest.CookieContainer = this._cookieJar;
			httpRequest.CachePolicy = this.CachePolicy;

			RequestSettings settings = ApplyCustomSettingsToDefaultSettings( request );
			httpRequest.Accept          = settings.Accept;
			httpRequest.UserAgent       = settings.UserAgent;
			httpRequest.ProtocolVersion = settings.ProtocolVersion;
			httpRequest.KeepAlive       = settings.KeepAlive;
			httpRequest.Timeout         = settings.Timeout;
			httpRequest.Credentials     = settings.Credentials;
			httpRequest.Referer         = settings.Referrer ?? this._lastPage;

			foreach( var pair in settings.Headers )
				httpRequest.Headers[pair.Key] = pair.Value;

			// httpRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
			// request.Headers["Accept-Encoding"] = "gzip,deflate"; // don't set this directly without using the above line(I guess)


			// convert it to a post message if needed
			IPostData postData = request.PostData;
			if( postData != null ) {
				byte[] postBytes = postData.PostBytes;

				// set request headers as appropriat
				httpRequest.Method = "POST";
				httpRequest.ContentLength = postBytes.Length;
				httpRequest.ContentType = postData.ContentType;

				// feed post data into the request
				using( System.IO.Stream requestStream = httpRequest.GetRequestStream() ) {
					requestStream.Write( postBytes, 0, postBytes.Length );
				}
			}

			return httpRequest;
		}

		private RequestSettings ApplyCustomSettingsToDefaultSettings( ScrapeRequest request ) {
			var settings = this.Defaults.Clone();
			foreach( var adjustment in request.AdjustSettings )
				adjustment( settings );
			return settings;
		}

		#endregion

		#region private fields

		// stores cookies needed for this site
		CookieContainer _cookieJar = new CookieContainer();

		/// <summary> stores the last web page requested. Used to auto-calc Referer header. </summary>
		string _lastPage;

		#endregion

	}



}