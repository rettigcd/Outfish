using System.Collections.Generic;
using System.Collections.Specialized;

namespace Outfish {

	static public class NameValueCollection_Extensions {

		public static IEnumerable<KeyValuePair<string, string>> Flatten( this NameValueCollection nameValueCollection ) {
			for( int i = 0; i < nameValueCollection.Count; ++i ) {
				string key = nameValueCollection.GetKey( i );
				string[] values = nameValueCollection.GetValues( i ) ?? new string[0]; // might be null if all values are null
				foreach( string value in values ) {
					if( value == null ) throw new System.InvalidOperationException("Unexpectedly got a null value from key:"+key);
					yield return new KeyValuePair<string, string>( key, value );
				}
			}
		}

	}

}
