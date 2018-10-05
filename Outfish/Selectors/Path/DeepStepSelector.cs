using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Searches descendants of match node - SLOW!
	/// </summary>
	/// <remarks>It might be appropriate to do this on the LAST step, 
	/// then we wouldn't generate duplicates like we would if we used this at the beggining or middle steps.</remarks>
	internal class DeepStepSelector : BaseStepSelector {
		public DeepStepSelector( INodeMatcher selector ):base(selector) { }

		override public IEnumerable<HtmlNode> FindDescendantNodes( HtmlNode rootNode ){
			return rootNode.Descendants().Where( this.Selector.IsMatch );
			// !!! if we matched nth-result, we'd have to do it here.
		}

		override public IEnumerable<XmlNode> FindDescendantNodes( XmlNode rootNode ) {
			return DescendantSearcherHelper.Descendants( rootNode ).Where( this.Selector.IsMatch );
			// !!! if we matched nth-result, we'd have to do it here.
		}

	}

}
