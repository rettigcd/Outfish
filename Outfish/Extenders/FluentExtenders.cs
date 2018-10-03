using System.Collections;
using System.Collections.Specialized;

namespace Outfish {

	/// <summary>
	/// Holds stuff to make parsing HTML pages more fluent
	/// </summary>
	static public class FluentExtenders {

		/// <summary>
		/// Fluent method for parsing a query string into NameValueCollection 
		/// by calling FormUrlPostData.GetFormCollection(..);
		/// </summary>
		static public NameValueCollection GetFormParameters(this string queryString){
			return UrlEncodedFormData.GetFormCollection( queryString );
		}
		
		// this for our internal use, internal so it doesn't collide with other types of Join
		static internal string Join(this IEnumerable parts, string glue) {
			var builder = new System.Text.StringBuilder();
			bool addGlue = false;
			foreach(var part in parts){ 
				if( addGlue )
					builder.Append( glue );
				else
					addGlue = true;
				builder.Append( part.ToString() ); 
			}
			return builder.ToString();
		}

	}

}
