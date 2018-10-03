using System;
using System.Xml;

namespace Outfish {

	internal class ContainsTextSelector : INodeMatcher {
	
		#region constructor
	
		public ContainsTextSelector( string subtext ){
			this._subtext = subtext;
		}
		
		#endregion
		
		public bool IsMatch(HtmlNode node) => node.OuterHtml.Contains( this._subtext ); 

		public bool IsMatch( XmlNode node ) => node.OuterXml.Contains( this._subtext );

		public override string ToString() {	return ":contains('" + this._subtext + "')"; }

		#region private field

		string _subtext;
		
		#endregion

	}
	
}
