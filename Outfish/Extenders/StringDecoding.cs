using System;
using System.Linq;
using System.Web;

namespace Outfish.Strings {

	/// <summary>
	/// Decodes a string that has been encoded.
	/// </summary>
	static public class StringDecoding {
	
		/// <summary>Calls HttpUtilityDecode on the string</summary>
		static public string DecodeHtml(this string thisString){ return HttpUtility.HtmlDecode( thisString ); }

		/// <summary>Calls HttpUtility.UrlDecode on the string</summary>
		static public string DecodeUrl(this string thisString){ return HttpUtility.UrlDecode(thisString); }

		/// <summary>Parses JSON-encoded string into original string. Encoded string must begin/end with ' or ".</summary>
		static public string DecodeJsonString( this string thisString ) {
		
			// eat whitespace
			int cur = 0;
			while( cur < thisString.Length && "\r\n\t ".Contains( thisString[cur] ) ){ ++cur; }
			if( cur == thisString.Length ){ throw new FormatException("Deserialize reached end of text reader without encountering any JSON objects."); }
	
			// check null
			if( thisString.IndexOf("null",cur)==cur ){ return null; }
			char quote = thisString[cur++];
			if( quote != '"' && quote != '\'' ){ throw new Exception("missing starting quote"); }
			var buf= new System.Text.StringBuilder();
			char k;
			while( cur<thisString.Length ) {	// allows closing quote to be missing
				k=thisString[cur++];	
				if( k == quote ){ break; }
				
				// handle normal case
				if(k != '\\') { buf.Append(k); continue; }
				
				// escape characters
				k = thisString[cur++];
				switch(k) {
					case 'b': k = '\b'; break;
					case 'f': k = '\f'; break;
					case 'n': k = '\n'; break;
					case 'r': k = '\r'; break;
					case 't': k = '\t'; break;
					case 'u': k = (char)int.Parse(thisString.Substring(cur,4), System.Globalization.NumberStyles.HexNumber); cur += 4;	break;
					default: break; // no action
				}
				buf.Append(k);
			}
			return buf.ToString();
		}
	
	}
}
