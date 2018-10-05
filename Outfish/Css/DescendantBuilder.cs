using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Outfish {


	class DescendantBuilder {

		public List<CssToken> _tokens;
		public int _index;

		public CssToken Peek => HasNext ? _tokens[_index] : null;
		public CssToken GetNext() => _tokens[ _index++ ];
		bool HasNext => _index < _tokens.Count;

		public DescendantBuilder( IEnumerable<CssToken> tokens ) {
			_tokens = tokens.ToList();
			_index = 0;
		}

		// consumes tokens until EOS or non-step token is reached
		public INodeMatcher ConsumeStep() {

			List<INodeMatcher> partialSelectorList = new List<INodeMatcher>();
			INodeMatcher partial;
			while( HasNext && (partial = ConsumePartialStep()) != null )
				partialSelectorList.Add( partial );

			switch( partialSelectorList.Count ) {
				case 0: throw new ArgumentOutOfRangeException( "No step-tokens provided" );
				case 1: return partialSelectorList[0];
				default: return new AndSelector( partialSelectorList );
			}

		}

		// consume exactly 1 partial-step
		INodeMatcher ConsumePartialStep() {

			CssToken t = GetNext();

			switch( t.Type ) {

				case CssTokenType.Name:		return new NameSelector( t.Text );
				case CssTokenType.Class:	return AttributeSelector.ContainsClass( t.Text.Substring( 1 ) );
				case CssTokenType.Id:		return AttributeSelector.IdIs( t.Text.Substring( 1 ) );
				case CssTokenType.Square:	{
						CssToken insideText = GetNext();
						GetNext(); // throw away closing token
						return ParseAttribute( insideText.Text );
					}
				case CssTokenType.Pseudo:

					switch( t.Text ) {
						case ":even":		return SiblingSelector.Even;
						case ":odd":		return SiblingSelector.Odd;
						case ":nth-child":	_index += 3;	return ParseSibling( _tokens[_index - 2].Text );
						case ":contains":	_index += 3;	return new ContainsTextSelector( _tokens[_index - 2].Text );
						default:			_index += 3;	return ParseColon( t.Text );
					}

			}

			--_index;// If didn't use it, we need to push it back onto the stream

			return null;

		}


		public IDescendantFinder Build() {

			List<CssPathSelector> paths = new List<CssPathSelector>();

			// get path
			paths.Add( ConsumePath() );

			while( HasNext ) {
				var t = GetNext();

				// consume comma
				if( t.Type != CssTokenType.Comma )
					throw new ArgumentOutOfRangeException( "Unexpected token:" + t.Text );

				// if they put a comma, there ought to be something following it...
				paths.Add( ConsumePath() );
			}

			return paths.Count == 1
				? (IDescendantFinder)paths[0]
				: new CssMultiPathSelector( paths );

		}

		static SiblingSelector ParseSibling( string s ) {
			Match m = Regex.Match( s, @"\s*((\d+)n\+(\d+)|(\d+)n|(\d))\s*", RegexOptions.IgnoreCase );
			if( m.Success == false ) throw new ArgumentOutOfRangeException( "can't parse nth-child for " + s );

			string divisor = m.Groups[4].Value + m.Groups[2].Value; // at most 1 will have a # 
			string remain = m.Groups[5].Value + m.Groups[3].Value;  // at most 1 will have a # 
			int d = divisor.Length > 0 ? int.Parse( divisor ) : 1;
			int r = remain.Length > 0 ? int.Parse( remain ) : 0;
			return new SiblingSelector( d, r );
		}

		// consumes steps and whitespace from token stream until EOS or ',' is reached
		CssPathSelector ConsumePath() {

			List<StepSelector> steps = new List<StepSelector>();

			bool bContinue = true;
			bool isImmediateChild = false;
			string siblingOp = null;
			StepSelector last = null;
			while( HasNext && bContinue ) {
				var t = Peek;
				switch( t.Type ) {

					case CssTokenType.Comma:
						bContinue = false;

						// TODO?? change the ',' to a terminator and consume it here instead of in the caller.

						break;

					case CssTokenType.Whitespace:
						++_index;
						break;

					case CssTokenType.Relationship:
						if( last == null ) throw new Exception( "Error starting selector path with relationship ( '>' '+' '~' ) operator." );
						if( t.Text == ">" ) {
							isImmediateChild = true; // set flag so next selector added, adds its self to parent as immediate
						} else {
							siblingOp = t.Text; // set flag so when next node gets added, it can strip off the previous node and use as a sibling instead
						}
						++_index;
						break;

					default:
						// add new step
						INodeMatcher ns = ConsumeStep();
						var cur = new StepSelector( ns );
						steps.Add( cur );

						// check last/cur have a relationship
						if( isImmediateChild ) {
							INodeMatcher pureParentSelector = last.Selector; // grab this before it gets soiled.
							last.AddChild( ns ); // this part helps pair down the tree faster
							cur.AddParent( pureParentSelector ); // this part is required to make it work
							isImmediateChild = false;
						}
						if( siblingOp != null ) {
							steps.Remove( last ); // remove last because it is not a parent
							cur.AddSibling( last.Selector, siblingOp == "~" ); // it is a sibling
							siblingOp = null;
						}

						last = cur;
						break;
				}
			}

			return new CssPathSelector( steps );
		}

		static AttributeSelector ParseAttribute( string src ) {
			string name = null;
			string op = null;
			string value = null;

			// first char:  _  : A-Za-z
			// remaining chars:  A-Za-z0-9_=:.
			Match m = Regex.Match( src
				, @"(	[A-Za-z:_][A-Za-z0-9\-_:\.]*	)	(([|*~$!^]?=)	(""[^""]+""|'[^']+'|[^""']+))?"
				, RegexOptions.IgnorePatternWhitespace
			);

			if( !m.Success ) throw new FormatException( "Can't parse Attribute selector " + src );

			// name
			name = m.Groups[1].Value;
			if( m.Groups[2].Success ) {
				// value - strip quotes
				op = m.Groups[3].Value;
				string v = m.Groups[4].Value;
				// strip off quotes
				if( v.Length > 2 && (v[0] == '\'' || v[0] == '"') ) { v = v.Substring( 1, v.Length - 2 ); }
				value = v;
			} else {
				op = AttributeSelector.ExistsOp;
			}
			return new AttributeSelector( name, op, value );
		}

		static INodeMatcher ParseColon( string src ) {

			switch( src ) {
				case FirstChildSelector.Css: return new FirstChildSelector();
				case LastChildSelector.Css: return new LastChildSelector();
				case OnlyChildSelector.Css: return new OnlyChildSelector();
				case ParentSelector.Css: return new ParentSelector();
				case EmptySelector.Css: return new EmptySelector();

				default: throw new ArgumentOutOfRangeException( "Colon selector " + src + " not implemented." );
			}

			#region don't need / not going to do

			//-- common attr short cuts --
			//case "checked":
			//case "selected":
			//case "disabled":
			//case "hidden":
			//case "visible":

			// -- don't need, just short cut for node name (why?)
			//case "button":
			//case "file":       //    elements of type file
			//case "header":		//         h1,h2,h3 etc
			//case "image":
			//case "input":
			//case "password":
			//case "radio":
			//case "reset":
			//case "submit":
			//case "text":
			//case "checkbox":

			//-- don't need, we have linq --
			//case "first":
			//case "last":
			//case "eq(n)":         the n element in the result set
			//case "gt(n)":
			//case "lt(n)":

			// --- not going to do			
			//:animated       element is animated
			//:focus      element has focus
			//:not(selector) negates selector

			#endregion

		}

	}



}
