
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Implements a predicate for HtmlNodes
	/// </summary>
	public interface INodeMatcher{

		/// <summary>Determines if the node matches the predicate.</summary>
		bool IsMatch( HtmlNode node );

		/// <summary>Determines if the node matches the predicate.</summary>
		bool IsMatch( XmlNode node );

	}
	
}
