using System.Collections.Generic;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Implements a rule/strategy for finding descendants of a given node.
	/// </summary>
	public interface IDescendantFinder {
		// only public so we can test it

		/// <summary>
		/// Finds the decendant using the internal strategy.
		/// </summary>
		IEnumerable<HtmlNode> FindDescendantNodes( HtmlNode rootNode );

		/// <summary>
		/// Finds the decendant using the internal strategy.
		/// </summary>
		IEnumerable<XmlNode> FindDescendantNodes( XmlNode rootNode );

	}

}
