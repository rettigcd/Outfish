using System.Xml;

namespace Outfish {

	/// <summary>
	/// Attched to the node you want, scans previous sibling since
	/// </summary>
	internal class HasPreviousSiblingSelector : INodeMatcher {
	
		#region constructor
	
		/// <param name="sibSelector"></param>
		/// <param name="any">true=>any sibling can match, false=>only immediate sibling can match</param>
		public HasPreviousSiblingSelector( INodeMatcher sibSelector, bool any ){
			this._sibSelector = sibSelector;
			this._any = any;
		}
	
		#endregion

		public bool IsMatch(HtmlNode node){
			
			int end = node.SiblingIndex;
			if( end == 0 ) return false; // required so we don't try to check node [-1]
			
			int i =  this._any 
				? 0 // check all nodes, starting with first one
				: end - 1;  // check only sibling immediately before
			
			for(;i<end;++i)
				if( _sibSelector.IsMatch( node.ParentNode.ChildNodes[i] ) )
					return true; 

			return false;
		}

		public bool IsMatch( XmlNode node ) {

			// if we are only checking the previous
			if( !_any )
				return node.PreviousSibling != null && _sibSelector.IsMatch( node.PreviousSibling );

			// need to check all
			if( node.ParentNode == null || node.ParentNode.ChildNodes == null )
				return false;

			var siblings = node.ParentNode.ChildNodes;
			for(int i=0;i<siblings.Count;++i) {
				var sib = siblings[i];
				if( sib == node ) break;
				if( _sibSelector.IsMatch(sib) ) return true;
			}
			return false;
		}

		public override string ToString() {
			return _sibSelector.ToString() + (this._any ? "~" : "+" );
		}

		#region private fields

		bool _any; // true searches all previous, false searches immediate
		INodeMatcher _sibSelector;
		
		#endregion
	}
}
