using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Outfish {

	static class CssLexer {
	
		public static List<CssToken> ParseTokens(string source){
		
			List<CssToken> tokens = new List<CssToken>();
		
			int index = 0;
			ConsumeWhiteSpace( source, ref index );
			
			while( index < source.Length ){
				char k = source[ index ]; 
				
				switch( k ){
	
					// need to record white space so we know when to start on new step
					case ' ': tokens.Add( new CssToken( " ", CssTokenType.Whitespace ) ); ConsumeWhiteSpace( source, ref index ); break;
	
					// single chars are their own token
					case '>': 
					case '~': 
					case '+': tokens.Add( new CssToken( k.ToString(), CssTokenType.Relationship ) ); ++index; break;
					case ',': tokens.Add( new CssToken( k.ToString(), CssTokenType.Comma ));  ++index; break;
						
					// class, id, pseudo
					case '#': tokens.Add( ConsumeNodeName( source, ref index, false, CssTokenType.Id     ) ); break; 
					case '.': tokens.Add( ConsumeNodeName( source, ref index, false, CssTokenType.Class  ) ); break; 
					case ':': tokens.Add( ConsumeNodeName( source, ref index, false, CssTokenType.Pseudo ) ); break;
	
					// 
					case '[': ConsumeOpenCloseGroup( source, ref index, tokens, ']', CssTokenType.Square ); break;
					case '(': ConsumeParameterGroup( source, ref index, tokens ); break;
	
					// node names
					default: tokens.Add( ConsumeNodeName( source, ref index, true, CssTokenType.Name ) ); break; 
				}
	
			}
			
			return tokens;
	
		}

		#region private ParseTokens helpers
	
		static void ConsumeOpenCloseGroup( string src, ref int index, List<CssToken> tokens, char closeChar, CssTokenType groupType ){
	
			char openChar = src[index++];
	
			int closeIndex = src.IndexOf( closeChar, index );
			if( closeIndex < index )
				throw new FormatException("'"+openChar+"' at "+(index-1)+" has no matching '"+closeChar+"'"); 
	
			// this works since () [] {} are non-nestable (I think)
			// return everything inside () as a single token. Let consumers parse them out
			tokens.Add( new CssToken( openChar.ToString(), groupType ) );
			tokens.Add( new CssToken( src.Substring( index, closeIndex - index ).Trim(), CssTokenType.Parameter)  );
			tokens.Add( new CssToken( closeChar.ToString(), groupType ));
			index = closeIndex+1;
		}

		static void ConsumeParameterGroup( string src, ref int index, List<CssToken> tokens ){
	
			char openChar = src[index++];
	
			var last = tokens.Last(); // grab this before we append the '(' token

			// open token
			tokens.Add( new CssToken( openChar.ToString(), CssTokenType.Round ) );

			// parameter
			if( last.Text == ":contains" ){	
				ConsumeContainsParameter( src, ref index, tokens );
			} else {
				int closeIndex = src.IndexOf( ')', index );
				if( closeIndex < index ){ 
					throw new FormatException("'"+openChar+"' at "+(index-1)+" has no matching ')'"); 
				}
				tokens.Add( new CssToken( src.Substring( index, closeIndex - index ).Trim(), CssTokenType.Parameter)  );
				index = closeIndex;
			}

			// close token
			ConsumeWhiteSpace( src, ref index );
			if( index == src.Length || src[index] != ')' ) throw new FormatException("didn't find expected ')'");
			tokens.Add( new CssToken( ")", CssTokenType.Round ));
			++index;

		}

		// returns a the :contains(...) parameter PRE-STRIPPING the quotes off of it.
		static void ConsumeContainsParameter( string src, ref int index, List<CssToken> tokens ){
			// skip whitespace
			ConsumeWhiteSpace( src, ref index );
			
			
			if( src[index] == ')' ) throw new FormatException(":contains parameter cannot be empty");
			char start = src[index];
			char endChar = ')'; // default
			if( start == '\'' || start == '"' ){
				index++; // skip starting char
				endChar = start; // change ending char to match starting char
			}

			int startIndex = index; // not including starting quote

			while( index < src.Length && src[index] != endChar ){
				++index;
			}
			
			// make sure we didn't 
			if( index == src.Length ){
				throw new Exception("Reached end of string without finding ending character "+endChar.ToString() );
			}
			
			// build token excluding the quotes
			var text = src.Substring( startIndex, index-startIndex );
			if( src[index] == ')' ){ text.Trim(); } // tear off any trailing spaces
			
			tokens.Add( new CssToken( text, CssTokenType.Parameter ) );

			if( src[index] != ')' ){ ++index; } // skip over closing quote

		}

		static CssToken ConsumeNodeName( string src, ref int index, bool validateFirstChar, CssTokenType type ){
		
			int start = index;
			 
			if( !validateFirstChar ){
				++index;
			}
			 
			while( index<src.Length && ">~+,:.#()[] ".IndexOf(src[index] ) == -1 ){
				++index;
			}
			if( index == start ){ 
				throw new Exception("how did we end up with a 0-length tag name starting at "+index+" for:"+src); 
			}
			return new CssToken( src.Substring( start, index-start ), type );
		}
		
		static void ConsumeWhiteSpace(string source, ref int cur){
			while( cur<source.Length && char.IsWhiteSpace( source[cur] ) ) ++cur;
		}
	
		#endregion
		
	}
	
}
