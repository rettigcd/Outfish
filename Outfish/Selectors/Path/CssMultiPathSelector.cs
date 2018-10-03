using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// A multi-path descendant finder that 
	/// 	correlates the results from the independant path results
	/// 	and puts them in Document order.
	/// </summary>
	internal class CssMultiPathSelector : IDescendantFinder  {
	
		#region constructor
	
		/// <summary>
		/// Constructs a multi-path descendant finder.
		/// </summary>
		public CssMultiPathSelector(IEnumerable<CssPathSelector> paths){
			this._selectorPathList = paths.ToList();
		}
	
		#endregion

		public IEnumerable<HtmlNode> FindDescendantNodes( HtmlNode node ){
			var nodes = this._selectorPathList
				.SelectMany( selectorPath => selectorPath.FindDescendantNodes(node) );
			
			// clean up
			nodes = nodes
				.OrderBy( n => n.Begin )	// put them in document order
				.Distinct();					// remove duplicates
			
			return nodes;
		}
	
		public override string ToString() {
			return _selectorPathList.Join(",");
		}

		public IEnumerable<XmlNode> FindDescendantNodes( XmlNode rootNode ) {
			throw new NotImplementedException();
		}


		#region private fields

		List<CssPathSelector> _selectorPathList;
	
		#endregion
	
	}
}
