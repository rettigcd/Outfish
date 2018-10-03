using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Outfish.Strings;
using Encoding = System.Text.Encoding;

namespace Outfish {

	/// <summary>
	/// Url-Encodes the keys and values using the application/x-www-form-urlencoded content type. 
	/// See http://www.w3.org/TR/html401/interact/forms.html#h-17.13.4.1 for more information.
	/// </summary>
	public class UrlEncodedFormData : IPostData {
		
		#region constructor
	
		/// <summary>
		/// Creates an empty NameValueCollection form data.
		/// Handy for not having to include System.Collections.Specialized.
		/// </summary>
		public UrlEncodedFormData(){
			this._postData = new NameValueCollection();
		}

		/// <summary> Creates FormUrlPostData from a NameValueCollection. </summary>
		public UrlEncodedFormData( NameValueCollection postData ){
			if( postData == null ) throw new ArgumentNullException(nameof(postData));
			this._postData = postData;
		}
		
		#endregion

		/// <summary>
		/// Get the bytes to be used as the WebRequest body
		/// </summary>
		public byte[] PostBytes => UrlEncodedFormData.EncodeBytes( this._postData );
		
		/// <summary> The enctype attribute on the form element for this encoding. </summary>
		public const string ContentTypeStr = "application/x-www-form-urlencoded";

		/// <summary>
		/// Returns "application/x-www-form-urlencoded" for use in header.
		/// </summary>
		public string ContentType => ContentTypeStr;
		
		/// <summary>
		/// Gets the storage mechanism for the PostData
		/// </summary>
		public NameValueCollection Collection{ get{ return this._postData; } }
		
		/// <summary>
		/// Returns a friendly string containing all entries in the post data.
		/// </summary>
		public override string ToString(){
			return  this.ContentType +"\r\n" + UrlEncodedFormData.GetQueryString( this._postData );
		}
		
		#region private
		
		private NameValueCollection _postData;

		#endregion

		/// <summary>
		/// Encodes the post information in the application/form-url-encoded format
		/// </summary>
		/// <param name="postData">The post data to encode.</param>
		/// <returns>The encoded post data.</returns>
		static public byte[] EncodeBytes( NameValueCollection postData ) {
			
			if(postData.Count == 0){ return new byte[0]; }
			
			string postDataString = GetQueryString( postData );
			
			const int WesternEuropeanWindowsCodePage = 1252;
			return Encoding.GetEncoding( WesternEuropeanWindowsCodePage )
				.GetBytes( postDataString );
		}

		/// <summary>
		/// converts the name-value collection to a string that can be "?" appended to the url
		/// </summary>
		static public string GetQueryString( NameValueCollection nameValueCollection ){
			return nameValueCollection.Flatten()
				.Select( ImplodeKeyValuePair )
				.Join( "&" );
		}

		static string ImplodeKeyValuePair( KeyValuePair<string, string> pair ) => HttpUtility.UrlEncode( pair.Key ) + "=" + HttpUtility.UrlEncode( pair.Value );

		/// <summary>
		/// Converts a "?" query string to the Name-value pair collection.
		/// </summary>
		static public NameValueCollection GetFormCollection( string queryString ){
			System.Collections.Specialized.NameValueCollection parameters = new System.Collections.Specialized.NameValueCollection();
			if( string.IsNullOrEmpty( queryString ) ){ return parameters; } // empty

			// tear off the leading '?' if it is there
			if( queryString[1] == '?' ){ queryString = queryString.Substring(1); }
			
			foreach( string pair in queryString.Split('&',';')){
				string[] parts = pair.Split('=');
				string key = parts[0].DecodeUrl();
				string value = parts[1].DecodeUrl();
				parameters[ key ] = value;
			}
			
			return parameters;
		}

	}
	
	
}
