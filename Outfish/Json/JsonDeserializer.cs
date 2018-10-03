using System;
using System.Text;
//using JsonArray = System.Collections.Generic.List<dynamic>;
//using JsonObject = System.Collections.Generic.Dictionary<string, dynamic>;

namespace Outfish.JavaScript {

	public class JsonArray : System.Collections.Generic.List<dynamic> { };
	public class JsonObject : System.Collections.Generic.Dictionary<string, dynamic> { };


	/// <summary>
	/// The world doesn't know about this object. 
	/// Use through JsonSerializer.
	/// </summary>
	internal class JsonDeserializer 
	//	: IDisposable 
	{

		public JsonDeserializer( string json ) {
			//_reader = new StringReader( json );
			_json = json;
		}

		public dynamic DeserializeJsonVal() {
			EatWhiteSpace();
			char next = SafePeekChar();
			switch( next ) {
				case 'n':
					ReadNullIfThere();
					return null;

				case '[':
					return this.DeserializeArray();

				case '{':
					return this.DeserializeDictionary();

				case 't':
				case 'f':
					return this.DeserializeBoolean(false).Value;

				case '\'':
				case '"':
					return this.DeserializeString();

				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '-':
				case '.':
					return this.DeserializeNumber(false).Value;

				default:
					throw new FormatException( "Deserialize() encountered unexpected character: " + next );
			}
		}



		public double? DeserializeNumber(bool allowNulls) {
			EatWhiteSpace();
			if( ReadNullIfThere() ) {
				if( allowNulls ) return null;
				throw new FormatException("Found null instead of number.");
			}

			int start = _index;

			if(SafePeekChar()=='-') ++_index; // negative?
			while(char.IsDigit(SafePeekChar())) ++_index; // whole #
			if( SafePeekChar() == '.' ) {	// has decimal
				++_index;
				while( char.IsDigit(SafePeekChar() ) ) ++_index; // get decimal part
			}

			if(SafePeekChar()=='E') {
				++_index;
				if(SafePeekChar()=='-') ++_index;
				int startOfE = _index;
				while( char.IsDigit(SafePeekChar())) ++_index;
				if( _index == startOfE ) throw new FormatException("Number in scientific form but missing exponent.");
			}

			return double.Parse(_json.Substring(start,_index-start));
		}

		bool IsQuote(char k) => k=='\'' || k=='"';

		public string DeserializeString() {
			EatWhiteSpace();
			if( ReadNullIfThere() ) return null;

			char startChar = SafePeekChar();
			if( !IsQuote(startChar)) throw new FormatException("Did not find starting quote of string.");
			UnsafeReadChar();

			StringBuilder buf = new StringBuilder();
			char k;
			while( (k = ReadAnyCharOrThrowExceptionIfAtEnd()) != startChar ) {
				// handle normal case
				if( k != '\\' ) {
					buf.Append( k );
					continue;
				}
				// escape characters
				k = ReadAnyCharOrThrowExceptionIfAtEnd();
				switch( k ) {
					case 'b': k = '\b'; break;
					case 'f': k = '\f'; break;
					case 'n': k = '\n'; break;
					case 'r': k = '\r'; break;
					case 't': k = '\t'; break;
					case 'u': k = (char)int.Parse( ReadString( 4 ), System.Globalization.NumberStyles.HexNumber ); break;
					default: break; // no action
				}
				buf.Append( k );
			}
			return buf.ToString();
		}

		public JsonArray DeserializeArray() {
			EatWhiteSpace();
			if( ReadNullIfThere() ) return null;
			JsonArray jsonArray = new JsonArray();

			ReadSpecificChar( '[' );
			EatWhiteSpace();
			if( SafePeekChar() != ']' )
				do {
					jsonArray.Add( this.DeserializeJsonVal() );
				} while( TryReadChar( ',' ) );
			ReadSpecificChar( ']' );
			return jsonArray;
		}

		public JsonObject DeserializeDictionary() {
			EatWhiteSpace();
			if( ReadNullIfThere() ) return null;

			JsonObject jsonObject = new JsonObject();

			ReadSpecificChar( '{' );
			EatWhiteSpace();
			if( SafePeekChar() != '}' ) {
				do {
					string key = this.DeserializeString();
					ReadSpecificChar( ':' );
					var val = this.DeserializeJsonVal();
					jsonObject.Add( key, val );

				} while( TryReadChar( ',' ) );
			}
			ReadSpecificChar( '}' );
			return jsonObject;
		}

		public bool? DeserializeBoolean(bool allowNulls) {
			EatWhiteSpace();
			if(allowNulls && ReadNullIfThere() ) return null;
			switch( SafePeekChar() ) {
				case 't': ReadString( "true" ); return true;
				case 'f': ReadString( "false" ); return false;
				default: throw new FormatException( "invalid boolean value" );
			}
		}

		//public void Dispose() {
		//	if( _reader != null ) _reader.Dispose();
		//	_reader = null;
		//}

		#region private Helper methods

		//char PeekChar() => (char)_reader.Peek();
		//char ReadChar() => (char)_reader.Read();
		//bool AtEnd => _reader.Peek() == -1;
		//int ReadIntoBuffer( char[] buf, int start, int length ) => _reader.Read(buf,start,length);
		//StringReader _reader;

		char SafePeekChar() => AtEnd ? '\0' : _json[_index]; // if we are at the end, '\0' is returned
		char UnsafeReadChar() => _json[_index++]; // the index is not checked here, caller must check index before calling this.
		bool AtEnd => _index == _json.Length;
		int ReadIntoBuffer( char[] buf, int start, int length ) {
			int index = start;
			while(index<length && _index<_json.Length)
				buf[index++] = _json[_index++];
			return index-start;
		}

		string _json;
		int _index = 0;


		void EatWhiteSpace() {
			while( "\r\n\t ".IndexOf( SafePeekChar() ) >= 0 )
				UnsafeReadChar();

			if( AtEnd )
				throw new FormatException( "Deserialize reached end of text reader without encountering any JSON objects." );
		}

		void ReadString( string str ) {
			char[] buf = new char[str.Length];
			ReadIntoBuffer( buf, 0, str.Length );
			if( str != new string( buf ) )
				throw new FormatException( "didn't read expected string: " + str );
		}

		string ReadString( int length ) {
			char[] buf = new char[length];
			int bytesRead = ReadIntoBuffer( buf, 0, length );
			if( bytesRead != length )
				throw new FormatException( string.Format( "Only read {0} when expecting {1}.", bytesRead, length ) );
			return new String( buf );
		}

		// checks if there is a null and reads it in.
		bool ReadNullIfThere() {
			if( SafePeekChar() != 'n' ) return false;
			ReadString( "null" );
			return true;
		}

		char ReadAnyCharOrThrowExceptionIfAtEnd() {
			if( AtEnd ) throw new FormatException( "reached end of stream/string unexpectedly." );
			return UnsafeReadChar();
		}

		void ReadSpecificChar( char k ) {
			EatWhiteSpace();
			char readChar = ReadAnyCharOrThrowExceptionIfAtEnd();
			if( k != readChar )
				throw new FormatException( $"Expected {k} but found {readChar}" );
		}

		bool TryReadChar( char k ) {
			EatWhiteSpace();
			if( SafePeekChar() != k ) return false;
			UnsafeReadChar();
			return true;
		}

		#endregion

	}

}
