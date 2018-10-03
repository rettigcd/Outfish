using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Outfish {

	/// <summary>
	/// Facilitates converting between index and line/column
	/// </summary>
	internal class LineManager{

		internal LineManager( string src ){
			this._lineStartIndexes = _startOfLineMatcher.Matches( src )
				.Cast<Match>()
				.Select(m=>m.Index)
				.ToArray();
			this._end = src.Length;
		}
	
		/// <summary>
		/// Perform 0-based conversion.  1st line is 0, 1st column is 0, 1st index is 0
		/// </summary>
		public void GetLineCol( int index, out int line, out int col ){
			if( index < 0 ){ throw new ArgumentOutOfRangeException("index"); }
		
			line =  _lineStartIndexes.Length-1; // start at end
			while( line > 0 && index <  _lineStartIndexes[line] ){
				--line;
			}
			
			col = index - _lineStartIndexes[line];
		
		}
	
		/// <summary>
		/// Perform 0-based conversion.  1st line is 0, 1st column is 0, 1st index is 0
		/// </summary>
		public int GetIndex( int line, int col ){
			if( line < 0 || line >=  _lineStartIndexes.Length ){ 
				throw new ArgumentOutOfRangeException("line", "line does not exist"); 
			}
			
			int index =  _lineStartIndexes[line] + col;
			
			if( index < 0 || index >= this._end ){ 
				throw new IndexOutOfRangeException("Resulting index of "+index+" was out of range for source string" );
			}
			return index;
		}

		#region private fields
	
		int[] _lineStartIndexes;
		int _end; // length of original string
	
		#endregion

		static internal IEnumerable<string> SplitToLines( string src ){
			return _startOfLineMatcher
				.Split( src )
				.Skip(1); // first line is always empty.
		}

		static Regex _startOfLineMatcher = new Regex("^",RegexOptions.Multiline);



	}
}
