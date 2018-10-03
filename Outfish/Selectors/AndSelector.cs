using System.Collections.Generic;
using System.Xml;

namespace Outfish {

	/// <summary>Non-heirarchy (single-step) HtmlNode Predicate that fails when ANY of its parts fail .</summary>
	/// <remarks>a 'step' is a single element with matching attributes or pseudo classes</remarks>
	internal class AndSelector : INodeMatcher {

		#region constructor

		/// <summary>
		/// Create an empty AND matcher with no parts.
		/// </summary>
		public AndSelector() {
			this.Selectors = new List<INodeMatcher>();
		}

		public AndSelector(params INodeMatcher[] selectors ){
			this.Selectors = new List<INodeMatcher>( selectors );
		}

		public AndSelector(List<INodeMatcher> selectors){
			this.Selectors = selectors;
			
		}

		#endregion

		public bool IsMatch( HtmlNode node ){
			foreach(INodeMatcher matcher in this.Selectors){
				if( !matcher.IsMatch(node) ){ return false; }
			}
			return true;
		}

		public bool IsMatch( XmlNode node ) {
			foreach( INodeMatcher matcher in this.Selectors ) {
				if( !matcher.IsMatch( node ) ) { return false; }
			}
			return true;
		}

		/// <summary>
		/// Gets a list of INodeMatchers which must ALL match in order for this to match
		/// </summary>
		public List<INodeMatcher> Selectors{ get; private set; }

		public override string ToString() {
			return this.Selectors.Join(string.Empty);
		}

	}

}

