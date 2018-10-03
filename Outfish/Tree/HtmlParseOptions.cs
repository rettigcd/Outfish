using System;
using System.Linq;

namespace Outfish {
	



	/// <summary>
	/// Specifies options on how an HTML string is parsed.
	/// </summary>
	[Flags]
	public enum HtmlParseOptions{ 
	
		/// <summary> Default, strips whitespace between consecutive open or close tags to keep desired nth-child behaviour - .</summary>
		Clean = 0,
		
		/// <summary>
		/// Allows text between empty tags to be included as nodes.
		/// Thus hosing the desirable behavior of nth-child.
		/// </summary>
		KeepEmptyText = 1,
		
	};

}
