using NUnit.Framework;
using Outfish;

namespace Outfish_Test {

	/// <summary>
	/// Tests that searching on name/values is case-INSENSITIVE
	/// But that node name/value retains original casing
	/// </summary>
	[TestFixture]
	public class TagsAndRelationships {
	
		HtmlNode Parse(string s){ return new HtmlDocument( s ).DocumentNode; }

		[Test]
		public void TagName_IsCaseInsensitive(){

			// all of our test strings must be mixed UPPER and LOWER case 
			// to make sure either casing works

			_checker.Parse( "<root><Bob/></root>"
				, "boB"
				, "Bob"
			);

		}

		[Test]
		public void Parent_Relationship(){

			// finds the spans that are immediate children of the div but not deeper
			_checker.Parse( "<root><div><SpAn/><SPAN/><p><spAN/></p></div><span/></root>"
				, "div>span" // no spaces
				, "SpAn","SPAN" // using casing to differentiate
			);

			// finds the spans that are immediate children of the div but not deeper
			_checker.Parse( "<root><div><SpAn/><SPAN/><p><spAN/></p></div><span/></root>"
				, "div > span" // with spaces
				, "SpAn","SPAN" // using casing to differentiate
			);

		}

		[Test]
		public void After_Relationship(){

			// finds the spans that are immediate children of the div but not deeper
			_checker.Parse( "<root><div><SpAn/><SPAN/></div><span/><p></p><spAN/></root>"
				, "div+span" // no spaces
				, "span" // using casing to differentiate
			);

			// finds the spans that are immediate children of the div but not deeper
			_checker.Parse( "<root><div><SpAn/><SPAN/></div><span/><p></p><spAN/></root>"
				, "div + span" // with spaces
				, "span" // using casing to differentiate
			);

		}

		[Test]
		public void Comma() {
			// finds the spans that are immediate children of the div but not deeper
			_checker.ParseHtml( "<root><spAN/><p></p><span/><div><SpAn/><SPAN/></div></root>"
				, "div,span" // all divs and all spans
				, "spAN","span","div","SpAn","SPAN" // using casing to differentiate
			); // !!! only works for Html
		}

		[Test]
		public void Before_Relationship(){

			// finds the spans that have a previous sibling that is a div
			_checker.ParseHtml( "<root><p/><SPAN/><div/><p/><span/></root>"
				, "div~span" // no spaces
				, "span" // using casing to differentiate
			);

			// finds the spans that have a previous sibling that is a div
			_checker.ParseHtml( "<root><p/><SPAN/><div/><p/><span/></root>"
				, "div ~ span" // with spaces
				, "span" // using casing to differentiate
			);

		}

		[Test]
		public void Asterisk() {
			// finds the spans that are immediate children of the div but not deeper
			_checker.ParseHtml( "<root><span/><p></p><div/></root>"
				, "*" // all nodes
				, "root","span","p","div" // using casing to differentiate
			);
		}

		CssChecker _checker = new CssChecker();
	}


}
