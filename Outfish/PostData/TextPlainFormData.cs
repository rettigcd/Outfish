using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outfish {

	class TextPlainFormData : IPostData {

		NameValueCollection _formData;

		public TextPlainFormData(NameValueCollection formData) { _formData = formData; }

		public const string ContentTypeStr = "text/plain";

		public string ContentType => ContentTypeStr;

		public byte[] PostBytes => EncodeBytes( _formData );

		static public byte[] EncodeBytes( NameValueCollection postData ) {

			if( postData.Count == 0 ) return new byte[0];

			string postDataString = GetQueryString( postData );

			const int WesternEuropeanWindowsCodePage = 1252;
			return Encoding.GetEncoding( WesternEuropeanWindowsCodePage )
				.GetBytes( postDataString );
		}

		/// <summary>
		/// converts the name-value collection to a string that can be "?" appended to the url
		/// </summary>
		static public string GetQueryString( NameValueCollection nameValueCollection ) {
			return nameValueCollection.Flatten()
				.Select( x=>x.Key+"="+x.Value+"\r\n" ) // documentation says convert ' ' to '+' but Chrome seems to url-encode it when used as query string.
				.Join(string.Empty);
		}

	}
}
