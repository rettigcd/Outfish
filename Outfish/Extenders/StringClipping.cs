using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Outfish.Strings {

	/// <summary>
	/// Extension methods that reduce the string to string of interest or extract sub strings.
	/// </summary>
	static public class StringClipping {

		/// <summary>
		/// Clips the Beginning off of the string.
		/// </summary>
		/// <param name="str">String to clip.</param>
		/// <param name="begining">Location of begining of new string.</param>
		/// <returns>String that has its ends clipped.</returns>
		static public string Clip(this string str, Clip begining){
			return Clip( str, begining, null );
		}

		/// <summary>
		/// Clips the Beginning and Ending off of the string.
		/// </summary>
		/// <param name="str">String to clip.</param>
		/// <param name="begining">Location of begining of new string.  Leave null for no clipping at the begining.</param>
		/// <param name="ending">Location of end of new string.</param>
		/// <returns>String that has its ends clipped.</returns>
		static public string Clip(this string str, Clip begining, Clip ending){
			int begin = (begining != null) ? begining.GetIndex(str,0)      : 0;
			int end   = (ending != null)   ? ending  .GetIndex(str,begin ) : str.Length;
			return str.Substring( begin, end-begin );
		}

		/// <summary>Collapses multple white space (\s{2,}) to a single space</summary>
		static public string CollapseWhiteSpace(this string thisString){
			return System.Text.RegularExpressions.Regex.Replace(thisString,@"\s{2,}"," ");
		}

		#region public Regex methods

		/// <summary> Matches the first occurance. </summary>
		/// <returns>If no match, throws ItemNotFoundException containing regex pattern and string being searched.</returns>
		static public Match MatchFirst(this string haystack, Regex regex){
			Match m = regex.Match( haystack );
			if( m != null && m.Success ) return m;

			throw new ItemNotFoundException(regex.ToString(), haystack);
		}

		/// <summary>
		/// Matches the first occurance. Throws exception if no match is found or not successful.  
		/// </summary>
		/// <returns>If no match, throws ItemNotFoundException containing regex pattern and string being searched.</returns>
		static public Match MatchFirst(this string haystack, string regexPattern ){
			Match m = Regex.Match( haystack, regexPattern );
			if( m != null && m.Success ) return m;

			throw new ItemNotFoundException( regexPattern, haystack );
		}

		/// <summary>
		/// Fluent method for finding RegEx Matches
		/// </summary>
		static public IEnumerable<Match> Matches( this string thisString, Regex regex ) 
			=> regex.Matches( thisString ).Cast<Match>();
	
		/// <summary>
		/// Fluent method for finding RegEx Matches
		/// </summary>
		static public IEnumerable<Match> Matches( this string thisString, string regexPattern )
			=> Regex.Matches(thisString, regexPattern ).Cast<Match>();

		#endregion


	}
}
