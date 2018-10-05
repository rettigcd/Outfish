using System;
using System.Collections.Generic;
using System.Xml;

namespace Outfish {

	// combines INodeMatcher and method of descendant search
	abstract internal class BaseStepSelector {
	
		#region constructor
	
		public BaseStepSelector( INodeMatcher selector ){
		 	this.Selector = selector;
		}
	
		#endregion
	
		public INodeMatcher Selector;

		/// <summary>
		/// used for when a parent node must have a specific child
		/// </summary>
		internal void AddChild( INodeMatcher childSelector ){
			AndSelector a = this.Selector as AndSelector;
			if( a == null )
				this.Selector = a = new AndSelector( this.Selector );

			// add at begining so it .ToString()s in the correct place
			a.Selectors.Insert( 0, new HasChildSelector( childSelector ) );
			this.HasChildSelector = true;
		}
	
		internal void AddParent( INodeMatcher parentSelector ){
			AndSelector a = this.Selector as AndSelector;
			if( a == null )
				this.Selector = a = new AndSelector( this.Selector );

			// add at begining so it .ToString()s in the correct place
			a.Selectors.Insert(0, new HasParentSelector( parentSelector ) );
		}
	
		internal void AddSibling( INodeMatcher sibSelector, bool any ){
			AndSelector a = this.Selector as AndSelector;
			if( a == null )
				this.Selector = a = new AndSelector( this.Selector );

			// add at begining so it .ToString()s in the correct place
			a.Selectors.Insert(0, new HasPreviousSiblingSelector( sibSelector, any ) );
		}
	
		public override string ToString() {
		
			// for parent > child, don't display the parent, since the child's HasParent will show it
			if( this.HasChildSelector )
				throw new Exception("StepSelectors with HasChild selector should be invisible and not .ToString()ed");
			
			// all others, just show them
			return this.Selector.ToString();
		}

		/// <summary>
		/// Steps that have child selectors are not necessary
		/// but help to pair the search tree down faster
		/// Also, the relationship is drawn by the child, so we don't want to display it twice
		/// </summary>
		internal bool HasChildSelector{ get; private set; }

		abstract public IEnumerable<HtmlNode> FindDescendantNodes( HtmlNode rootNode );

		abstract public IEnumerable<XmlNode> FindDescendantNodes( XmlNode rootNode );

	}

}
