using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Outfish {

	/// <summary>
	/// Reads a WebResponse by trying to detect the character encoding from the Response Header
	/// </summary>
	public class ResponseReader{
	
		#region constructor
	
		/// <summary>
		/// Constructs a ResponseReader with a default encoding of #1252
		/// </summary>
		public ResponseReader():this(Encoding.GetEncoding(1252)){}
	
		/// <summary>
		/// Constructs a ResponseReader using a custom default encoding.
		/// </summary>
		/// <param name="defaultEncoding">The character encoding to use if not specified by web page</param>
		public ResponseReader( Encoding defaultEncoding ){
			if( defaultEncoding == null ){ throw new ArgumentNullException(); }
			this.DefaultEncoding = defaultEncoding;
		}
	
		#endregion
	
		///<summary>the default encoding to be used if none is found.</summary>
		public Encoding DefaultEncoding{ get; private set; }
	
		/// <summary>
		/// Reads the response content as a string.
		/// Tries to detect the encoding but falls back on the default if not successful.
		/// </summary>
		virtual public string ReadAllText( System.Net.HttpWebResponse webResponse ){
			var encoding = this.GetContentEncoding( webResponse );
			using(System.IO.StreamReader reader = new System.IO.StreamReader( webResponse.GetResponseStream(), encoding )) {
				return reader.ReadToEnd(); // content
			}
		}

		/// <summary>
		/// Reads the response content as a string.
		/// Tries to detect the encoding but falls back on the default if not successful.
		/// </summary>
		virtual async public Task<string> ReadAllTextAsync( System.Net.HttpWebResponse webResponse ){
			var encoding = this.GetContentEncoding( webResponse );
			using(System.IO.StreamReader reader = new System.IO.StreamReader( webResponse.GetResponseStream(), encoding )) {
				return await reader.ReadToEndAsync(); // content
			}
		}


		/// <summary>
		/// Returns the Encoding used for the response.  We have to do this by hand because sometimes
		/// HttpWebResponse doesn't get the character encoding embedded in the Content-Type header
		/// </summary>
		/// <param name="response">The http response containing an encoding.</param>
		virtual protected Encoding GetContentEncoding( HttpWebResponse response ) {
	
			// if it is present, use it
			if(response.ContentEncoding != null && response.ContentEncoding.Trim().Length != 0) {
				try {
					return this.GetEncodingFromName( response.ContentEncoding );
				}
				catch(ArgumentException) {
					// eat it
				}
			}
	
			// otherwise, extract manually
			foreach(string s in response.ContentType.Split(';')) {
				if(s.StartsWith("charset=")) {
					try{
						return this.GetEncodingFromName( s.Substring(8) );
					}
					catch(ArgumentException){ 
						// eat it
					}
				}
			}
	
			// last option - use the default
			return this.DefaultEncoding;
		}
		
		///<summary>override to to do custom name figure-outing</summary>
		virtual protected Encoding GetEncodingFromName( string encodingName ){
			return Encoding.GetEncoding( encodingName );
		}
		
	}
}
