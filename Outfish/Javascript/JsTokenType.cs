using System;

namespace Outfish.JavaScript {

	/// <summary>possible Javascript token types</summary>
	public enum JsTokenType { 
	
		/// <summary>String litteral. Surrounded by single or double quotes.</summary>
		StringLiteral, 
		/// <summary>Represents a number.</summary>
		NumberLiteral, 
		/// <summary>A regulat expression litteral. Surrounded by '/'</summary>
		RegexLiteral,
		/// <summary>true or false</summary>
		BooleanLiteral,
		/// <summary>any of the operators</summary>
		Operator, 
		/// <summary>a variable or function name.</summary>
		Identifier, 
		/// <summary>() {} []</summary>
		Bracket,
		/// <summary>for, if, while, function, var, etc.</summary>
		Keyword,
		/// <summary>;</summary>
		Semicolon,

		//HexLiteral, - need to add, not currently parsing these...
	};

}
