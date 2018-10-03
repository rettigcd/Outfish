using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Outfish;

namespace Outfish_Test {

	/// <summary>
	/// Tests that searching on name/values is case-INSENSITIVE
	/// But that node name/value retains original casing
	/// </summary>
	[TestFixture]
	public class Capitalization {
	
		HtmlNode Parse(string s){ return new HtmlDocument( s ).DocumentNode; }
	
		[Test,Description( "searching by attribute-name is case-insensitive")]
		public void AttributeNames(){
		
			// all of our test strings must be mixed UPPER and LOWER case 
			// to make sure both/either casing works
		
			// Given: attribute with mixed casing
			var node = this.Parse("<root><bob Abc='123'/></root>");
			// And: a css selector with mixed casing that doesn't match the attribute
			string incorrectlyCasedCss = "[aBc]";

			// When: search 
			List<HtmlNode> matches = node.Find( incorrectlyCasedCss ).ToList();

			// Then: we find the match.
			Assert.That( matches.Count, Is.EqualTo(1) );
			Assert.That( matches[0]["abC"], Is.EqualTo("123") );	// access attribute with differnent case
		}

		[Test]
		public void AttributeValues(){
		
			// all of our test strings must be mixed UPPER and LOWER case 
			// to make sure either casing works
		
			var node = this.Parse("<root><bob a='Abc'/></root>");

			// searching attribute-value is case insensitive
			List<HtmlNode> matches = node.Find("[a='aBc']").ToList();	// search with different cased Attr
			Assert.That( matches.Count, Is.EqualTo(1) );
			
			// attribute retains its original casing
			Assert.That( matches[0]["a"], Is.EqualTo("Abc") );
		}

		[Test]
		public void TagNames(){
		
			// all of our test strings must be mixed UPPER and LOWER case 
			// to make sure either casing works
			var node = this.Parse("<root><Bob/></root>");
		
			// searching on tag-name is case-insensitive
			List<HtmlNode> matches = node.Find("boB").ToList();	// search with different cased Attr
			Assert.That( matches.Count, Is.EqualTo(1) );

			// node retains original casing on tag-name
			Assert.That( matches[0].Name, Is.EqualTo("Bob") );		// make sure node still has original capitalization
		}


	}


}
