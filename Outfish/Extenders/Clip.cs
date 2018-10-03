using System;
using System.Text.RegularExpressions;

namespace Outfish.Strings {

	/// <summary>
	/// Encapsulates a Strategy for finding indexes in strings, usually for clipping.
	/// </summary>
	public class Clip{

		/// <summary>Find needle beginning. (Index of first character of needle.)</summary>
		static public Clip At(string needle){ return new Clip(needle,false); }

		/// <summary>Find needle beginning. (Index of first character of needle.)</summary>
		static public Clip At(Regex regex){ return new Clip(regex,false); }

		/// <summary>Find needle end. (Index of first character following needle.)</summary>
		static public Clip After(string needle){ return new Clip(needle,true); }

		/// <summary>Find needle end. (Index of first character following needle.)</summary>
		static public Clip After(Regex regex){ return new Clip(regex,true); }

		/// <summary>Skips skipCount characters.</summary>
		static public Clip Skip(int skipCount){ return new Clip(skipCount,false); }
		
		/// <summary>Starts at end of haystack and backs up trimLength characters.</summary>
		static public Clip FromEnd(int trimLength){ return new Clip(trimLength,true); }

		#region private constructor
	
		Clip(Regex regex, bool after){
			this._regex = regex;
			this._after = after;
		}
	
		Clip(int skipCount, bool fromEnd){
			this._skipCount = skipCount;
			this._skipFromEnd = fromEnd;
		}
		
		Clip(string needle, bool after){
			this._needle = needle;
			this._after = after;
		}
		
		#endregion
		
		/// <summary>
		/// Applies the strategy for finding an index in a string (haystack).
		/// </summary>
		/// <returns>Index of result.  Throws ItemNotFoundException if no index found.</returns>
		public int GetIndex( string haystack, int startIndex ){
		
			if( _needle != null ){
				// searching for a string
				int index = haystack.IndexOf( _needle );
				if( index == -1 )
					throw new ItemNotFoundException( _needle, haystack ); // !! would be helpful to include index in exception
				if( _after ){ index += _needle.Length; }
				return index;
			}
			
			if( _regex != null ) {
				// do regex
				Match m = _regex.Match( haystack, startIndex );
				if( m == null || !m.Success )
					throw new ItemNotFoundException( _regex.ToString(), haystack ); // !! would be helpful to include index in exception
				int index = m.Index;
				if( _after ){ index += m.Length; }
				return index;
			}
			
			// skip from begin/end
			int result = _skipFromEnd
				? haystack.Length - _skipCount
				: startIndex + _skipCount;
			if( result < startIndex || result > haystack.Length ){
				string msg = string.Format("{0} {1} was out of bounds while parsing {2}.", this._skipCount, this._skipFromEnd?"before":"after", haystack );
				throw new Exception( msg );
			}
			return result;
			
		}
	
		#region private fields
		
		// for finding substring
		Regex _regex;
		string _needle;
		bool _after;
		
		// for trimming left or right
		int _skipCount;
		bool _skipFromEnd;
	
		#endregion
	
	
	}
}
