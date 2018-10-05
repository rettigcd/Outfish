using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Outfish;

namespace Outfish_Test {

	[TestFixture]
	public class ColonSelectors_Test {

		void Select(string html, string cssSelector, params string[] expected ){
			var root = new HtmlDocument( html ).DocumentNode;
			var matches = root.Find( cssSelector ).ToList();
			
			Assert.That( matches.Count, Is.EqualTo( expected.Length ) );
			for(int i=0;i<expected.Length;++i){
				Assert.That( matches[i].OuterHtml, Is.EqualTo( expected[i] ) );
			}
		}

		[Test]
		public void Even(){
			this.Select("<root><a/><b/><c/><d/><e/></root>"
				, ":even"
				, "<b/>", "<d/>"
			);
		}

		[Test]
		public void Odd(){
			this.Select("<root><a/><b/><c/><d/><e/></root>"
				, ":odd"
				,"<a/>","<c/>","<e/>"
			);

		}

		[Test]
		public void FirstChild(){
			this.Select("<root><a/><b/><c/><d/><e/></root>"
				, ":first-child"
				, "<a/>"
			);
		}

		[Test]
		public void LastChild(){
			this.Select("<root><a/><b/><c/><d/><e/></root>"
				, ":last-child"
				, "<e/>"
			);
		}


		[Test]
		public void OnlyChild(){
			// don't select anything if not only child
			this.Select("<root><a/><b/><c/><d/><e/></root>"
				, ":only-child"
			);

			this.Select("<root><c/></root>"
				, ":only-child"
				, "<c/>"
			);
		}

		[Test]
		public void Empty(){
		
			// text nodes are empty too according to http://api.jquery.com/empty-selector/
			this.Select("<root><a>111</a><b/><c></c><d>222</d><e/></root>"
				, ":empty"
				, "111", "<b/>", "<c></c>", "222", "<e/>"
			);
		}

		[Test]
		public void Parent(){
			this.Select("<root><a>111</a><b/><c></c><d>222</d><e/></root>"
				, ":parent"
				, "<a>111</a>", "<d>222</d>"
			);
		}

		[Test]
		public void NthChild(){
			this.Select("<root><a/><b/><c/><d/><e/><f/><g/></root>"
				, ":nth-child(3n+2)" 
				, "<b/>", "<e/>"
			);
		}

		[Test]
		public void Contains(){
			this.Select("<root><a/><b/><c/><d id=AAA /><e id=aaa /><f/><g/></root>"
				, ":contains(aaa)" 
				, "<e id=aaa />"
			);
		}

		[Test]
		public void Contains_With_Spaces(){
			this.Select("<root><a/><b/><c/><d>the cat</d><e id='the cat' /><f/><g/></root>"
				, ":contains(the cat)" 
				, "<d>the cat</d>", "<e id='the cat' />"
			);
		}

		[Test]
		public void Contains_With_Comma(){
			this.Select("<root><a/><b/><c/><d>Mac, the knife</d><e id='Mac, the knife' /><f/><g/></root>"
				, ":contains(Mac, the knife)" 
				, "<d>Mac, the knife</d>", "<e id='Mac, the knife' />"
			);
		}

		[Test]
		public void Contains_SingleQuoted(){
			this.Select("<root><a/><b/><c/><d>bobandtom</d><e id='the cat' /><f/><g/></root>"
				, ":contains('and')" 
				, "<d>bobandtom</d>"
			);
		}

		[Test]
		public void Contains_DoubleQuoted(){
			this.Select("<root><a/><b/><c/><d>bobandtom</d><e id='the cat' /><f/><g/></root>"
				, @":contains(""and"")" 
				, "<d>bobandtom</d>"
			);
		}

		[Test]
		public void Contains_With_Close_Paren(){
			// make sure ) embedded inside string doesn't cut off string
			this.Select("<root><a/><b/><c/><d>bob(and)tom</d><e id='the cat' /><f/><g/></root>"
				, @":contains("")"")" 
				, "<d>bob(and)tom</d>"
			);
		}


		[Test]
		public void Child_Of(){
		
			// test that we can node that is immediate child of parent
			// the <b> embeded inside the <c> should not be returned
			this.Select("<root><a><b>this one</b><c><b>not this one</b></c></root>"
				, @"a>b" // finds b that is immediately child of a
				, "<b>this one</b>"
			);
			
			// NOTE!  This test makes sure that the a & b are tightly coupled.
			// First, the <a> matches because it has a <b> child
			// Second, only the <b> that has an <a> parent should match

			// In original implementation, this test failed
			// because <a>, matched but the Second condition (<b> has <a> parent) wasn't implemented
			// so the grandchild <b> matched too.
		}

		[Test]
		public void Sibling_Imediate(){
		
			// test that we can node that is immediate child of parent
			// the <b> embeded inside the <c> should not be returned
			this.Select("<root><b>1</b><b>2</b><a/><b>3</b><b>4</b></root>"
				, @"a+b" // finds b that is immediately child of a
				, "<b>3</b>"
			);
			
		}

		[Test]
		public void Sibling_Previous(){
		
			// test that we can node that is immediate child of parent
			// the <b> embeded inside the <c> should not be returned
			this.Select("<root><b>1</b><b>2</b><a/><b>3</b><b>4</b></root>"
				, @"a~b" // finds b that is immediately child of a
				, "<b>3</b>", "<b>4</b>"
			);
			
		}



	}
}
