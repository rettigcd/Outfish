using System;
using System.Collections.Specialized;

namespace Outfish {

	/// <summary>
	/// Encapsulates all info (except cookies) required to make a web request
	/// </summary>
	public class ScrapeRequest {

		#region constructors

		/// <summary>Creates a new GET Request</summary>
		/// <param name="uri">URI to retrieve</param>
		public ScrapeRequest( Uri uri, params Action<RequestSettings>[] adjustSettings )
			: this( uri, (IPostData)null, adjustSettings ) { } // GET:

		/// <summary>Creates a new GET Request</summary>
		public ScrapeRequest( string uriString, params Action<RequestSettings>[] adjustSettings ) 
			:this(ToUri(uriString), (IPostData)null, adjustSettings) { } // GET: ToUri
		
		/// <summary>Creates a new GET or POST request</summary>
		/// <param name="postData">null = GET, non-null = POST</param>
		public ScrapeRequest( Uri uri, NameValueCollection postData, params Action<RequestSettings>[] adjustSettings )
			:this(uri, ToPost(postData), adjustSettings ) { } // GET/POST: ToPost

		/// <summary>Creates a new GET or POST request</summary>
		/// <param name="uriString">URI to retrieve</param>
		/// <param name="postData">null = GET, non-null = POST</param>
		public ScrapeRequest( string uriString, NameValueCollection postData, params Action<RequestSettings>[] adjustSettings )
			: this( ToUri( uriString ), ToPost(postData), adjustSettings ) { } // GET/POST: ToUri, ToPost

		/// <summary>Create a new GET or POST request</summary>
		/// <param name="uriString">URI to retrieve</param>
		/// <param name="postData">null = GET, non-null = POST</param>
		public ScrapeRequest( string uriString, IPostData postData, params Action<RequestSettings>[] adjustSettings )
			: this( ToUri( uriString), postData, adjustSettings ) { } // GET/POST: ToUri

		/// <summary>Create a new GET or POST request</summary>
		/// <param name="uri">URI to retrieve</param>
		/// <param name="postData">null = GET, non-null = POST</param>
		public ScrapeRequest( Uri uri, IPostData postData, params Action<RequestSettings>[] adjustSettings ) {
			if( uri == null ) throw new ArgumentNullException(nameof(uri));
			this.Uri = uri;
			this.PostData = postData;
			this.AdjustSettings = adjustSettings;
		}

		#region private static constructor-helper methods

		static Uri ToUri( string uri ) {
			try { return new Uri( uri ); } catch( UriFormatException ex ) { throw new ArgumentException( "Invalid URI: " + uri, ex ); }
		}

		static IPostData ToPost( NameValueCollection postData ) {
			return postData == null ? null : new UrlEncodedFormData( postData );
		}

		#endregion

		#endregion

		/// <summary>
		/// Gets the URI the request will go to
		/// </summary>
		public Uri Uri{ get; private set; }
		
		/// <summary>
		/// Gets the POST data that will be submitted with a POST request.
		/// </summary>
		/// <returns>null for GET requests</returns>
		public IPostData PostData{ get; private set; }
		
		/// <summary>
		/// Gets the request method (GET or POST) depending if there is POST data present.
		/// </summary>
		public string Method{ get{ return this.PostData != null ? "POST" : "GET"; } }
		
		/// <summary>
		/// Never null. Contains Optional request headers and configurations.
		/// </summary>

		/// <summary>
		/// Adjust request headers, timeout, etc of an individual request.
		/// </summary>
		public Action<RequestSettings>[] AdjustSettings { get; private set; }

		/// <summary>
		/// Displays the uri, query string, and any post data.
		/// </summary>
		public override string ToString() {

			if( this.PostData == null ){
				return "GET " + this.Uri.ToString();
			} else {
				return "POST " + this.Uri.ToString() 
					+ "\r\nPostData  --> "
					+ this.PostData.ToString();
			}
		}

		/// <summary> Converts NameValueCollection to query string that can be appended to a base url. </summary>
		/// <remarks> Convenience short cut to FormUrlPostData.GetQueryString(...) </remarks>
		/// <param name="queryParameters">parameters to construct into a string</param>
		/// <returns>query string (without the leading '?')</returns>
		static public string ToQuery( NameValueCollection queryParameters ){
			return UrlEncodedFormData.GetQueryString( queryParameters );
		}
	
	}

}
