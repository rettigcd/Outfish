using System.Collections.Generic;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Finds the closest matching node/descendant.  Stops searching when first match is found.
	/// </summary>
	internal class StepSelector : BaseStepSelector {
		public StepSelector( INodeMatcher selector ):base(selector) { }

		// finds the highest match in the tree.
		// for css selectors, once we find a match (except for + and >) 
		// we are just wasting time searching deeper and generating duplicates
		// ** Once a matching node has been found, it will not search that nodes descendants **
		override public IEnumerable<HtmlNode> FindDescendantNodes( HtmlNode rootNode ){
			Stack<HtmlNode> stack = new Stack<HtmlNode>();
			DescendantSearcherHelper.PushChildrenOntoStack( rootNode, stack );
			while( stack.Count > 0 ){
				HtmlNode node = stack.Pop();
				if( this.Selector.IsMatch( node ) )
					yield return node;
				else
					DescendantSearcherHelper.PushChildrenOntoStack( node, stack );
			}
			// !!! if we matched nth-result, we'd have to do it here.
		}

		// finds the highest match in the tree.
		// for css selectors, once we find a match (except for + and >) 
		// we are just wasting time searching deeper and generating duplicates
		// ** Once a matching node has been found, it will not search that nodes descendants **
		override public IEnumerable<XmlNode> FindDescendantNodes( XmlNode rootNode ) {
			Stack<XmlNode> stack = new Stack<XmlNode>();
			DescendantSearcherHelper.PushChildrenOntoStack( rootNode, stack );
			while( stack.Count > 0 ) {
				XmlNode node = stack.Pop();
				if( this.Selector.IsMatch( node ) )
					yield return node;
				else
					DescendantSearcherHelper.PushChildrenOntoStack( node, stack );
			}
			// !!! if we matched nth-result, we'd have to do it here.
		}

	}

}
