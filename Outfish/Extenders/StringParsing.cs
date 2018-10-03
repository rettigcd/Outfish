using System;
using System.Text.RegularExpressions;

namespace Outfish.Strings {

	/// <summary>
	/// Extention methods that convert the string to another type.
	/// </summary>
	static public class StringConvert {
	
		/// <summary>finds first integer using regex (\d+) and calls int.Parse()</summary>
		static public int ParseInt( this string thisString ){
			return int.Parse( thisString.MatchFirst(@"\d+").Value );
		}
	
		/// <summary>finds first integer using regex (\d+) and calls long.Parse()</summary>
		static public long ParseLong( this string thisString ){
			return long.Parse( thisString.MatchFirst(@"\d+").Value );
		}
	
		/// <summary>finds first number using regex ([0-9\.]+) and calls double.Parse()</summary>
		static public double ParseDouble( this string thisString ){
			return double.Parse( thisString.MatchFirst( @"[0-9\.]+").Value );
		}

		/// <summary>Finds first boolean string (true,TRUE,False,FaLSe) and parses it to true or false;</summary>
		static public bool ParseBool( this string thisString ){
			return thisString.MatchFirst(new Regex("true|false",RegexOptions.IgnoreCase)).Value.ToLower() == "true";
		}
		
		/// <summary> Parses out the first decimal regardless of what comes after it. </summary>
		static public decimal ParseDecimal( this string thisString ){
			return decimal.Parse( thisString.MatchFirst(new Regex(@"[0-9,\.]+")).Value.Replace(",","") );
		}
	
	}
	

}
