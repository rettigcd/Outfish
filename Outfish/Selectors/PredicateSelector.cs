using System;
using System.Xml;

namespace Outfish {

	internal class FirstChildSelector : INodeMatcher {
		public bool IsMatch( XmlNode node ) => node.ParentNode==null || node.ParentNode.FirstChild == node;
		public bool IsMatch( HtmlNode node ) => node.SiblingIndex == 0;
		public override string ToString() => Css;
		public const string Css = ":first-child";
	}

	internal class LastChildSelector : INodeMatcher {
		public bool IsMatch( XmlNode node ) => node.ParentNode == null || node.ParentNode.LastChild == node;
		public bool IsMatch( HtmlNode node ) => node.SiblingIndex == node.SiblingCount - 1;
		public override string ToString() => Css;
		public const string Css = ":last-child";
	}

	internal class OnlyChildSelector : INodeMatcher {
		public bool IsMatch( XmlNode node ) => node.ParentNode == null || node.ParentNode.ChildNodes.Count == 1;
		public bool IsMatch( HtmlNode node ) => node.SiblingCount == 1;
		public override string ToString() => Css ;
		public const string Css = ":only-child";
	}

	internal class ParentSelector : INodeMatcher {
		public bool IsMatch( XmlNode node ) => node.ChildNodes.Count > 0;
		public bool IsMatch( HtmlNode node ) => node.IsParent;
		public override string ToString() => Css;
		public const string Css = ":parent";
	}

	internal class EmptySelector : INodeMatcher {
		public bool IsMatch( XmlNode node ) => node.ChildNodes.Count > 0;
		public bool IsMatch( HtmlNode node ) => node.ChildNodes == null || node.ChildNodes.Count==0;
		public override string ToString() => Css;
		public const string Css = ":empty";
	}

}
