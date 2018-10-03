using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Outfish {

	enum CssTokenType{ 
		Name,  			// node name
		Pseudo, 		// all the ':' something
		Id, 			// #
		Class, 			// .
		Square, 		// [ ]
		Round, 			// ( )
		Parameter, 		// suff inside a [ ] or ()
		Whitespace, 	// ' '  We NEED this token type to delimit steps
		Comma, 			// ','
		Relationship	// '>' '~' '+'
	};

	class CssToken{
		public CssToken( string text, CssTokenType type ){ this.Text = text; this.Type = type; }
		public string Text{ get; private set; }
		public CssTokenType Type{ get; private set; }
	}
	
}
