using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Outfish {

	// combines INodeMatcher and method of descendant search
	internal class StepSelector {
	
		#region constructor
	
		public StepSelector( INodeMatcher selector ){
		 	this.Selector = selector;
		}
	
		#endregion
	
		public INodeMatcher Selector;
		
		/// <summary>
		/// false => highest match in each true. (the default)
		/// true => all descendants (for last step on path)
		/// </summary>
		public bool SearchAllDescendants{ get; set; }
		
		/// <summary>
		/// used for when a parent node must have a specific child
		/// </summary>
		public void AddChild( INodeMatcher childSelector ){
			AndSelector a = this.Selector as AndSelector;
			if( a == null ){
				this.Selector = a = new AndSelector( this.Selector );
			}
			// add at begining so it .ToString()s in the correct place
			a.Selectors.Insert( 0, new HasChildSelector( childSelector ) );
			this.HasChildSelector = true;
		}
	
		public void AddParent( INodeMatcher parentSelector ){
			AndSelector a = this.Selector as AndSelector;
			if( a == null ){
				this.Selector = a = new AndSelector( this.Selector );
			}
			// add at begining so it .ToString()s in the correct place
			a.Selectors.Insert(0, new HasParentSelector( parentSelector ) );
		}
	
		public void AddSibling( INodeMatcher sibSelector, bool any ){
			AndSelector a = this.Selector as AndSelector;
			if( a == null ){
				this.Selector = a = new AndSelector( this.Selector );
			}
			// add at begining so it .ToString()s in the correct place
			a.Selectors.Insert(0, new HasPreviousSiblingSelector( sibSelector, any ) );
		}
	
		/// <summary>
		/// calls .Search on each node and returns results together in group.
		/// </summary>
		public IEnumerable<HtmlNode> Search( IEnumerable<HtmlNode> nodes ){
			return nodes.SelectMany<HtmlNode,HtmlNode>( this.SearchX );
		}

		/// <summary>
		/// calls .Search on each node and returns results together in group.
		/// </summary>
		public IEnumerable<XmlNode> Search( IEnumerable<XmlNode> nodes ) {
			return nodes.SelectMany<XmlNode, XmlNode>( this.SearchX );
		}

		public override string ToString() {
		
			// for parent > child, don't display the parent, since the child's HasParent will show it
			if( this.HasChildSelector ){
				throw new Exception("StepSelectors with HasChild selector should be invisible and not .ToString()ed");
			}
			
			// all others, just show them
			return this.Selector.ToString();
		}

		/// <summary>
		/// nodes that have child selectors are not necessary
		/// but help to pair the search tree down faster
		/// Also, the relationship is drawn by the child, so we don't want to display it twice
		/// </summary>
		internal bool HasChildSelector{ get; private set; }

		#region private
	
		IEnumerable<HtmlNode> SearchX( HtmlNode rootNode ){
		
			return this.SearchAllDescendants
				? rootNode.Descendants().Where( this.Selector.IsMatch )
				: this.HighestMatchedDescendants( rootNode ); // get the highest in each branch
				
			// !!! if we matched nth-result, we'd have to do it here.
				
		}

		IEnumerable<XmlNode> SearchX( XmlNode rootNode ) {

			return this.SearchAllDescendants
				? DescendantSearcherHelper.Descendants( rootNode ).Where( this.Selector.IsMatch )
				: this.HighestMatchedDescendants( rootNode ); // get the highest in each branch - doesn't search down a node branch if the node matches

			// !!! if we matched nth-result, we'd have to do it here.

		}

		// finds the highest match in the tree.
		// for css selectors, once we find a match (except for + and >) 
		// we are just wasting time searching deeper 
		// and generating duplicates
		// ** Once a matching node has been found, it will not search that nodes descendants **
		IEnumerable<HtmlNode> HighestMatchedDescendants( HtmlNode root ){
			Stack<HtmlNode> stack = new Stack<HtmlNode>();
			DescendantSearcherHelper.PushChildrenOntoStack( root, stack );
			while( stack.Count > 0 ){
				HtmlNode node = stack.Pop();
				if( this.Selector.IsMatch( node ) ){
					yield return node;
				} else {
					DescendantSearcherHelper.PushChildrenOntoStack( node, stack );
				}
			}
		}

		// finds the highest match in the tree.
		// for css selectors, once we find a match (except for + and >) 
		// we are just wasting time searching deeper 
		// and generating duplicates
		IEnumerable<XmlNode> HighestMatchedDescendants( XmlNode root ) {
			Stack<XmlNode> stack = new Stack<XmlNode>();
			DescendantSearcherHelper.PushChildrenOntoStack( root, stack );
			while( stack.Count > 0 ) {
				XmlNode node = stack.Pop();
				if( this.Selector.IsMatch( node ) ) {
					yield return node;
				} else {
					DescendantSearcherHelper.PushChildrenOntoStack( node, stack );
				}
			}
		}


		#endregion

	}
}
