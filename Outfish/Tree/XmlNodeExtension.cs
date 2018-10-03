using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Outfish {

	static public class XmlNodeExtension {

		/// <summary> Finds Descendant nodes that match the CSS selector. </summary>
		/// <param name="cssSelector"> 1 or more css paths, sepearted by commas(,) </param>
		static public IEnumerable<XmlNode> Find( this XmlNode root, string cssSelector ) {
			return new CssExpression( cssSelector ).Find( root );
		}

		/// <summary>
		/// Finds the first node matching the selector.  Throws exception with including CSS-Selctor and OuterHtml if no node is found.
		/// </summary>
		///<remarks>Find first is used so many times that it would be super handy 
		/// to make it available and to report the CssSelecotr and OuterHtml when it fails.
		/// </remarks>
		static public XmlNode FindFirst( this XmlNode root, string cssSelector ) {
			XmlNode node = root.Find( cssSelector ).FirstOrDefault();
			if( node != null ) return node;

			throw new ItemNotFoundException( cssSelector, root.OuterXml );
		}

		/// <summary>
		/// Search immediate child nodes for a match.
		/// </summary>
		static public IEnumerable<XmlNode> Children( this XmlNode root, string cssSelector ) {
			// !!! assumes cssSelector is is single-step, single path
			// the single-step assumption is good because children are a single step
			// the single-path assumption is bad because we should be able to separate with a comma
			return root.ChildNodes
				.Cast<XmlNode>()
				.Where( new CssExpression( cssSelector ).IsMatch );
		}

		///// <summary>
		///// Steps up the tree from the closest parent to the root node returning each node that matches
		///// </summary>
		//static public IEnumerable<HtmlNode> Parents( this XmlNode root, string cssSelector ) {
		//	return root.Parents().Where( new CssExpression( cssSelector ).IsMatch );
		//}

		/// <summary>
		/// Searches up ancestor tree, starting with self, for first node that matches
		/// </summary>
		static public XmlNode Closest( this XmlNode root, string cssSelector ) {
			var cssExpression = new CssExpression( cssSelector );
			XmlNode cur = root;
			while( cur != null ) {
				if( cssExpression.IsMatch( cur ) ) { return cur; }
				cur = cur.ParentNode;
			}
			return null;
		}


	}

}
