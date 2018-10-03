using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Outfish {

	public class MultiPartFormData : IPostData {

		public const string ContentTypeStr = "multipart/form-data";

		public string Boundary { get; private set; }

		/// <summary>
		/// Generates a random boundary.
		/// </summary>
		public MultiPartFormData( NameValueCollection formData ) {
			Boundary = "----WebKitFormBoundary"+ _rnd.GetString(16);
			ContentType = ContentTypeStr + "; boundary=" +Boundary;

			RequestBody = formData
				.Flatten()
				.Select(Format)
				.Join("\r\n") + "\r\n--" + Boundary + "--\r\n";

		}

		string Format(KeyValuePair<string,string> pair) {
			return "--" + Boundary + "\r\n"
				+ "Content-Disposition: form-data; name=\""+pair.Key+"\"\r\n"
				+ "\r\n"
				+ pair.Value;

		}

		public string RequestBody { get; private set; }

		class RandomFactory {
			Random _rnd = new Random();
			public RandomFactory() { }
			public string GetString( int length ) {
				var buf = new StringBuilder( length );
				for( int i = 0; i < length; ++i )
					buf.Append( GetCharacter() );
				return buf.ToString();
			}
			public char GetCharacter() {
				int i = _rnd.Next( 10 + 26 + 26 - 1 );
				if( i < 10 ) return (char)(i + 48); i -= 10; // 0..9
				if( i < 26 ) return (char)(i + 65); i -= 26; // A..Z
				return (char)(i + 97); // a..z
			}
		}

		static RandomFactory _rnd = new RandomFactory();

		public string ContentType { get; private set; }

		public byte[] PostBytes {
			get {
				if( RequestBody.Length == 0 ) { return new byte[0]; }

				const int WesternEuropeanWindowsCodePage = 1252;
				return Encoding.GetEncoding( WesternEuropeanWindowsCodePage )
					.GetBytes( RequestBody );
			}
		}

		public override string ToString() {
			return nameof(MultiPartFormData)+":"+RequestBody;
		}
	}

}
