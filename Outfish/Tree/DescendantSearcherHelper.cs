using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Outfish {

	static class DescendantSearcherHelper {

		// pushes children onto the stack in reverse order so they pop off in correct order
		// !!! TODO - this doesn't go in here, should be in the thing that calls it
		static internal void PushChildrenOntoStack( HtmlNode root, Stack<HtmlNode> stack ) {
			// add children to stack (in reverse 
			for( int i = root.ChildNodes.Count; i > 0; ) {
				stack.Push( root.ChildNodes[--i] );
			}
		}

		static internal IEnumerable<HtmlNode> Descendants( HtmlNode root ) {
			Stack<HtmlNode> stack = new Stack<HtmlNode>();

			DescendantSearcherHelper.PushChildrenOntoStack( root, stack );

			while( stack.Count > 0 ) {
				HtmlNode node = stack.Pop();
				// yield this node
				yield return node;
				// add children to stack
				DescendantSearcherHelper.PushChildrenOntoStack( node, stack );
			}
		}

		// pushes children onto the stack in reverse order so they pop off in correct order
		// !!! TODO - this doesn't go in here, should be in the thing that calls it
		static internal void PushChildrenOntoStack( XmlNode root, Stack<XmlNode> stack ) {
			// add children to stack (in reverse 
			for( int i = root.ChildNodes.Count; i > 0; ) {
				stack.Push( root.ChildNodes[--i] );
			}
		}

		static internal IEnumerable<XmlNode> Descendants( XmlNode root ) {
			Stack<XmlNode> stack = new Stack<XmlNode>();

			DescendantSearcherHelper.PushChildrenOntoStack( root, stack );

			while( stack.Count > 0 ) {
				XmlNode node = stack.Pop();
				// yield this node
				yield return node;
				// add children to stack
				DescendantSearcherHelper.PushChildrenOntoStack( node, stack );
			}
		}


	}

}
