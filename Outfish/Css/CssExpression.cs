using System.Collections.Generic;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Uses a CSS-style pattern to find nodes in an HtmlDocument or HtmlNode.
	/// </summary>
	public class CssExpression{

		#region constructor

		/// <summary>
		/// Parses a CSS-like expression to build a strategy for finding or matching HtmlNodes
		/// </summary>
		public CssExpression( string cssPattern ){
			_pattern = cssPattern;
			_tokens = CssLexer.ParseTokens( _pattern );
		}

		#endregion

		/// <summary>
		/// Determines if the HtmlNode matches the CSS expression.
		/// </summary>
		public bool IsMatch( HtmlNode node ) => Matcher.IsMatch( node );

		/// <summary>
		/// Determines if the HtmlNode matches the CSS expression.
		/// </summary>
		public bool IsMatch( XmlNode node ) => Matcher.IsMatch( node );

		/// <summary>
		/// Finds descendants of the node that match the CSS expression.
		/// </summary>
		/// <param name="node">root node</param>
		public IEnumerable<HtmlNode> Find( HtmlNode node ){
			return DescendantFinder.FindDescendantNodes( node );
		}

		/// <summary>
		/// Finds descendants of the node that match the CSS expression.
		/// </summary>
		/// <param name="node">root node</param>
		public IEnumerable<XmlNode> Find( XmlNode node ) {
			return DescendantFinder.FindDescendantNodes( node );
		}

		/// <summary>
		/// Returns the string used to construct the CssExpression
		/// </summary>
		public override string ToString(){ return _pattern; }

		#region private fields

		string _pattern;
		List<CssToken> _tokens;
		INodeMatcher _matcher;
		INodeMatcher Matcher => _matcher ?? (_matcher = new DescendantBuilder( _tokens).ConsumeStep() );
		IDescendantFinder _descendantFinder;
		IDescendantFinder DescendantFinder => _descendantFinder ?? (_descendantFinder = new DescendantBuilder( _tokens ).Build() );

		#endregion

	}



}
