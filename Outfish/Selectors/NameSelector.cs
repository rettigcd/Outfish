using System;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Matches HtmlNodes of a given name a, h1, h2, etc
	/// </summary>
	internal class NameSelector : INodeMatcher {
	
		#region constructor
	
		/// <summary>
		/// creates a html node matcher that where the name must be equal
		/// </summary>
		/// <param name="name">tag/node name to match</param>
		public NameSelector(string name){
			if( name == null ){ throw new ArgumentNullException("name"); }
			this._name = name.ToLower();
		}
		
		#endregion
		
		public bool IsMatch(HtmlNode node) {
			return node.Name != null
				&& this._name == node.Name.ToLower();
		}

		public bool IsMatch( XmlNode node ) {
			return node.Name != null
				&& this._name == node.Name.ToLower();
		}

		public override string ToString() {
			return this._name;
		}

		#region private fields

		string _name;
		
		#endregion
	}
}
