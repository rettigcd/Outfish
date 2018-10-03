using System.Linq;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Attached to Parent selector for parent > child selector
	/// Not required for parent > child but speeds up searching for items with relation ship because it pairs the tree down faster
	/// </summary>
	internal class HasChildSelector : INodeMatcher {
	
		public HasChildSelector( INodeMatcher childSelector ){
			this._childSelector = childSelector;
		}
		
		public bool IsMatch(HtmlNode node){
			return node.IsParent
				&& node.ChildNodes.Where( this._childSelector.IsMatch ).FirstOrDefault() != null;
		}

		public bool IsMatch( XmlNode node ) {
			return node.ChildNodes != null
				&& node.ChildNodes.Cast<XmlNode>().FirstOrDefault( this._childSelector.IsMatch ) != null;
		}

		INodeMatcher _childSelector;
	}

}
