using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Outfish.JavaScript {

	// rename some classes show preferred types
	//	using JsonArray = System.Collections.Generic.List<dynamic>;
	//	using JsonObject = System.Collections.Generic.Dictionary<string, dynamic>;

	/// <summary>
	/// LL(1), recursive-descent, predictive parser for serializing/deserializing:
	///        objects,enumerables,nulls,numbers, etc to JavaScript Object Notation
	/// </summary>
	/// <remarks>
	/// Deserialize methods are thread-safe but the the Serialize methods are not.
	/// </remarks>
	public class JsonSerializer {

		#region public Deserialize methods

		public dynamic    Deserialize          ( string json ) => new JsonDeserializer( json ).DeserializeJsonVal();
		public JsonArray  DeserializeArray     ( string json ) => new JsonDeserializer( json ).DeserializeArray();
		public string     DeserializeString    ( string json ) => new JsonDeserializer( json ).DeserializeString();
		public JsonObject DeserializeDictionary( string json ) => new JsonDeserializer( json ).DeserializeDictionary();
		public bool       DeserializeBoolean   ( string json ) => new JsonDeserializer( json ).DeserializeBoolean(false).Value;
		public double     DeserializeDouble    ( string json ) => new JsonDeserializer( json ).DeserializeNumber(false).Value;

		public bool?      DeserializeNullableBoolean( string json ) => new JsonDeserializer( json ).DeserializeBoolean(true);
		public double?    DeserializeNullableNumber ( string json ) => new JsonDeserializer( json ).DeserializeNumber(true);

		#endregion

		#region public Serialization methods

		public string Serialize( object obj ) {

			if( obj == null || obj == DBNull.Value ) {
				return "null";
			} else if( obj is string ) {
				// string
				return this.SerializeString( (string)obj );
			} else if( obj is IDictionary ) {
				// Object / Hash / Dictionary
				return this.SerializeDictionary( (IDictionary)obj );
			} else if( obj is IEnumerable && !(obj is char[]) ) {
				// Array / Enumberable
				return this.SerializeArray( (IEnumerable)obj );
			} else if( obj is bool ) {
				// Bool
				return this.SerializeBoolean( (bool)obj );
			} else if( obj is int || obj is uint
				|| obj is short || obj is ushort
				|| obj is byte || obj is sbyte
				|| obj is float || obj is double
				|| obj is decimal || obj is long
				|| obj is ulong
			) {
				// number
				return obj.ToString();
			} else if( obj is char )
				return this.SerializeString( new String( (char)obj, 1 ) );
			else if( obj is char[] )
				return this.SerializeString( new String( (char[])obj ) );
			else if( obj is Enum )
				return this.SerializeString( obj.ToString() );
			else
				throw new Exception( "Cannot serialize object of type " + obj.GetType().Name );

		}

		public string SerializeArray( IEnumerable enumerable ) {
			if( enumerable == null ) { return "null"; }
			List<string> parts = new List<string>();
			this._tabIndex++;
			foreach( object obj in enumerable ) {
				parts.Add( this.Serialize( obj ) );
			}
			this._tabIndex--;
			string breaker = this.GetBreaker();
			return this.ObjectJoin( "[]", parts );
		}

		public string SerializeBoolean( bool b ) {
			return b.ToString().ToLower();
		}

		public string SerializeDictionary( IDictionary dict ) {
			if( dict == null ) { return "null"; }
			List<string> parts = new System.Collections.Generic.List<string>();
			this._tabIndex++;
			foreach( DictionaryEntry entry in dict ) {
				string key = entry.Key as string;
				if( key == null )
					throw new ArgumentException( "Dictionary keys must be strings but key: " + entry.Key.ToString() + " of type:" + entry.Key.GetType().ToString() + " was found." );
				parts.Add( this.SerializeString( key ) + ":" + this.Serialize( entry.Value ) );
			}
			this._tabIndex--;
			string breaker = this.GetBreaker();
			return this.ObjectJoin( "{}", parts );
		}

		public string SerializeString( string s ) {

			if( s == null ) { return "null"; }

			StringBuilder sb = new StringBuilder( s.Length + 4 );

			sb.Append( '"' );
			for( int i = 0; i < s.Length; i += 1 ) {
				char c = s[i];
				switch( c ) {
					case '"': sb.Append( @"\""" ); break;
					case '\\': sb.Append( @"\\" ); break;
					case '/': sb.Append( @"\/" ); break;
					case '\b': sb.Append( @"\b" ); break;
					case '\f': sb.Append( @"\f" ); break;
					case '\n': sb.Append( @"\n" ); break;
					case '\r': sb.Append( @"\r" ); break;
					case '\t': sb.Append( @"\t" ); break;
					default:
						if( c < (char)32 || c > (char)127 ) {
							sb.Append( "\\u" );
							sb.Append( ((int)c).ToString( "x" ).PadLeft( 4, '0' ) );
						} else {
							sb.Append( c );
						}
						break;
				}

			}
			sb.Append( '"' );
			return sb.ToString();

		}

		#endregion

		#region public Serialization properties

		public int TabMax { get; set; } = 3;

		public bool UseBreakers { get; set; } = false;

		#endregion

		#region private helper Serialize methods

		// constructs an object
		// builds arrays [] and ojbects {}
		string ObjectJoin( string openClose, List<string> parts ) {
			if( parts.Count == 0 ) { return openClose; } // no parts --> 1 line

			string breaker = this.GetBreaker();
			StringBuilder builder = new StringBuilder();

			// open
			builder.Append( openClose[0] );

			foreach( string part in parts ) {
				builder.Append( part );
				builder.Append( breaker );
				builder.Append( ',' );
			}
			builder.Length--;// remove last comma (this feels dirty but seems to be most efficient

			if( parts.Count == 1 )
				builder.Length -= breaker.Length;

			// close
			builder.Append( openClose[1] );

			return builder.ToString();
		}

		// retuns the \n\t\t... combo form making json easier to read
		string GetBreaker() {
			if( this.UseBreakers == false || this._tabIndex > this.TabMax )
				return string.Empty;

			StringBuilder builder = new StringBuilder( this._tabIndex );
			builder.Append( '\r' );
			builder.Append( '\n' );
			builder.Append( '\t', this._tabIndex );
			return builder.ToString();
		}

		#endregion

		#region private fields
		int _tabIndex = 0;
		#endregion

	}

}
