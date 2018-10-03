using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Outfish.JavaScript {

	/// <summary>
	/// Scans a string and returns java-script tokens.
	/// </summary>
	public class JsLexer {

		#region constructor / setup

		/// <summary>
		/// Constructs a JsLexer
		/// </summary>
		public JsLexer() {

			string wsOrComment = @"(\s+ | //.*\s* | /\*(.|\s)*?\*/)"; // matches 1

			// this order of regex is fastest for parsing prototype.js

			this.Add( JsTokenType.Semicolon,     @"\G;" );
			this.Add( JsTokenType.Bracket,       @"\G[{}\[\]()]" );
			
			// keywords before identifiers
			this.Add( JsTokenType.Keyword,       @"\G(break|case|catch|continue|debugger|default|delete|do|else|finally|for|function|if|in|instanceof|new|return|switch|this|throw|try|typeof|var|void|while|with)\b");
			this.Add( JsTokenType.BooleanLiteral,@"\G(true|false)\b");
			this.Add( JsTokenType.Identifier,    @"\G[A-Za-z_$][A-Za-z0-9_$]*" );
			
			this.Add( JsTokenType.NumberLiteral, @"\G[+-]?(\d+\.?\d*|\.\d+)" );// should match these 4 patterns:  0 .1 1. 1.1
			this.Add( JsTokenType.Operator,      @"\G(>>>= | <<= | >>= | === | !== | >>>  | \+\+ | -- | << | >> | <= | >= | && | \|\| | == | != | \*= | %= | \+= | -= | &= | \^=| \|= | [.\-+~!*%<>&^|?:=,])" );

			// string literal is last of common stuff because *?  is slow but strings are very common.
			this.Add( JsTokenType.StringLiteral, @"\G(['""]) (\\.|.)*? \1" );// quote followed by escaped char or char followed by same quote

			// note! / operator before regex
			this.Add( JsTokenType.Operator,      @"\G (/=|/)"
				,(t)=> {
					if( t == null ){ return false; }
					switch( t.Type ){
				 	case JsTokenType.Identifier:
						case JsTokenType.StringLiteral:
						case JsTokenType.NumberLiteral: return true;
						case JsTokenType.Bracket: return "] )".Contains( t.Text );
					}
					return false;
				}
			);
			this.Add( JsTokenType.RegexLiteral,  @"\G / (\\/|[^/])+ / [gia]*" );

			this._whiteSpaceAndCommentsRegex = new Regex( 
				 @"\G" + wsOrComment + "+" // match 1 or more
				,RegexOptions.IgnorePatternWhitespace
			);

		}

		#endregion

		/// <summary>Scans string for all JavaScript tokens.</summary>
		/// <remarks>public for unit testing</remarks>
		public IEnumerable<JsToken> GetTokens(string src){
			return this.GetTokens(src,0,src.Length);
		}
		
		/// <summary>Scans string for all JavaScript tokens in the range begin(inclusive) to end(exclusive).</summary>
		/// <remarks>public for unit testing</remarks>
		public IEnumerable<JsToken> GetTokens(string src, int begin, int end){

			this.ConsumeWhiteSpaceAndComments(src, ref begin);

			JsToken t = null;
			while( begin < end ){
				// get token at next spot
				t = this.GetToken(src,begin,end,t);

				// throw exception
				if( t == null ){
					int len = Math.Min( src.Length - begin, 40 ); // limit msg to 40 characters
					throw new FormatException("unable to match token["+src.Substring(begin,len)+"] at pos " + begin, null);
				}
	
				begin = t.End; // advance pointer to end of current token
				this.ConsumeWhiteSpaceAndComments(src, ref begin);
				
				yield return t;
			}
		
		}

		#region private

		void Add( JsTokenType t, string pattern ){
			this.Add( t, pattern, null );
		}

		void Add( JsTokenType t, string pattern, Predicate<JsToken> allowedPrev ){
			Regex r = new Regex( pattern, RegexOptions.IgnorePatternWhitespace );
			this._orderedRegexList.Add( r );
			this._typeLookup.Add( r, t );
			this._allowedPrevToken.Add( allowedPrev );
		}

		void ConsumeWhiteSpaceAndComments(string src, ref int cur) {
			Match m = _whiteSpaceAndCommentsRegex.Match( src, cur );
			if( m.Success ){ cur += m.Length; }
		}

		JsToken GetToken(string src, int begin, int end, JsToken last){
			for(int i=0; i<this._orderedRegexList.Count; ++i ){
				// if previous token predicate fails, go to next
				var prevTokenPred = this._allowedPrevToken[i];
				if( prevTokenPred != null && prevTokenPred( last )==false ){ continue; }
				// check regex
				Regex r = this._orderedRegexList[i];
				Match m = r.Match( src, begin );
				if( m.Success ){
					return new JsToken( src, begin, begin + m.Length, this._typeLookup[r] );
				}
			}
			return null;
		}

		#endregion

		#region private fields
		
		List<Regex> _orderedRegexList = new List<Regex>(); // the order to run the comparisions in order for it to work

		List<Predicate<JsToken>> _allowedPrevToken = new List<Predicate<JsToken>>(); // the order to run the comparisions in order for it to work

		Dictionary<Regex,JsTokenType> _typeLookup = new Dictionary<Regex,JsTokenType>();

		Regex _whiteSpaceAndCommentsRegex;
		
		#endregion

	}

}
