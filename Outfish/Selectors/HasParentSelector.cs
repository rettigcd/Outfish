using System;
using System.Linq;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Attached to Child selector for parent > child selector
	/// </summary>
	internal class HasParentSelector : INodeMatcher {
	
		public HasParentSelector( INodeMatcher childSelector ){
			this._parentSelector = childSelector;
		}
		
		public bool IsMatch(HtmlNode node){
			return node.ParentNode != null
				&& _parentSelector.IsMatch( node.ParentNode );
		}

		public bool IsMatch( XmlNode node ) {
			return node.ParentNode != null
				&& _parentSelector.IsMatch( node.ParentNode );
		}

		public override string ToString() {
			var x = _parentSelector.GetType().ToString();
			return _parentSelector.ToString() + ">";
		}

		INodeMatcher _parentSelector;
	}

}
