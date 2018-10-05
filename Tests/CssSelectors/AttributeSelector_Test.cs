using NUnit.Framework;
using Outfish;

namespace Outfish_Test {

	// https://www.w3schools.com/cssref/css_selectors.asp

	[TestFixture]
	public class AttributeSelector_Test {

		[Test]
		public void AttributeContainsSubstring(){
			_checker.Parse("<root><aa bb='cc1cc'/><d e='ff1ff'/><g bb=''/></root>"
				,"[bb*='1']"
				,"aa"
			);
		}

		[Test]
		public void Searching_AttributeNode_For_Attribute_DoesntCrash(){
			// some nodes with no attributes will have attributes dictionary == null
			_checker.Parse("<root><aa/><d/><g id='Street'/></root>"
				,"#Street"
				,"g"
			);
		
		}

		[Test]
		public void AttributeContainsPrefix(){

			// !!! Specification says value has to be a whole word, not just the start of a word

			_checker.Parse(@"<root>
							<a bb='pretable bbb'/>
							<d bb='ff pre1ff'/>
							<g bb='fdfdpre'/>
						</root>"
				,"[bb|='pre']"
				,"a","d"
			);
		}


		[Test]
		public void Attribute_StartsWith(){
			// Does not have to be whole word
			// !!! should return d too I think.

			_checker.Parse(@"<root>
							<a bb='startable bbb'/>
							<d bb='ff start1ff'/>
							<g bb='fdfstartdpre'/>
							<f bb=''/>
						</root>"
				,"[bb^='start']"
				,"a"
			);
		}

		[Test]
		public void Attribute_EndsWith(){

			// !!! the value does not have to be a whole value

			_checker.Parse(@"<root>
							<a bb='_suf'/>
							<b bb='ff start1ff_suf'/>
							<c bb='aaa_suf fdfs_suftdpre'/>
							<d bb=''/>
						</root>"
				,"[bb$='_suf']"
				,"a","b"
			);
		}

		[Test]
		public void Exists(){
			// (malformed ids don't parse using XmlNode)
			_checker.ParseHtml(@"<root>
							<a bb='_suf'/>
							<b id='ff  start1ff_suf'/>
							<c id />
							<d sd=''/>
						</root>"
				,"[id]"
				,"b", "c" // attributes with no values, use name as value
			);
		}

		[Test]
		public void EqualTo(){

			_checker.Parse(@"<root>
							<a id='_suf'/>
							<b id='Ten'/>
							<c id='ten ten' />
							<d id=''/>
						</root>"
				,"[id='teN']"
				,"b"
			);
		}

		[Test]
		public void NotEqualTo(){
			_checker.Parse(@"<root>
							<a id='_suf'/>
							<b id='ten'/>
							<c id='ten ten' />
							<d id=''/>
						</root>"
				,"[id!='ten']"
				,"a","c","d"
			);
		}

		[Test]
		public void ContainsWord(){
			_checker.Parse(@"<root>
							<a id='often'/>
							<b id='ten'/>
							<c id='fff ten' />
							<d id=''/>
						</root>"
				,"[id~='ten']"
				,"b","c"
			);
		}
		
		[Test]
		public void AttributesWithDashes(){
			// make sure we can find attributes with a dash in the name
			_checker.Parse("<root><inner data-src='bob' /></root>", "[data-src]", "inner" );
		
		}
		
		[Test]
		public void Parse_No_Quote_AttributeValue(){
			_checker.Parse(@"<root>
							<a a='z aza z'/>
							<b a='z'/>
							<c a='zzzz' />
							<d a=''/>
						</root>"
				,"[a=z]"
				,"b"
			);
		}

		[Test]
		public void AttributeValues_IsCaseInsensitive(){
		
			// all of our test strings must be mixed UPPER and LOWER case 
			// to make sure either casing works
			_checker.Parse("<root><bob a='Abc'/></root>"
				,"[a='aBc']"
				,"bob"
			);

		}

		[Test,Description( "searching by attribute-name is case-insensitive")]
		public void AttributeNames_IsCaseInsensitive(){

			//// all of our test strings must be mixed UPPER and LOWER case 
			//// to make sure both/either casing works
			_checker.ParseHtml("<root><bob Abc='123'/></root>"
				,"[aBc]"
				,"bob"
			);
	
			// !!! Doesn't work for Xml, works for html only

		}


		[Test]
		public void Easy_Id(){

			// (malformed ids don't parse using XmlNode)
			_checker.ParseHtml(@"<root>
							<a a='z aza z'/>
							<b id=bob />
							<c class='bob' />
							<d id='bob and son'/>
						</root>"
				,"#bob"
				,"b"
			);

		}

		[Test]
		public void Easy_Class(){

			// (malformed ids don't parse using XmlNode)
			_checker.ParseHtml(@"<root>
							<a class='z aza z'/>
							<b class=bob />
							<c id='bob' />
							<d class='bob and son'/>
						</root>"
				,".bob"
				,"b","d"
			);
			
		}

		[TestCase("[a]")]
		[TestCase("[a='b']")]
		[TestCase("[a*='b']")]
		[TestCase("[a|='b']")]
		[TestCase("[a^='b']")]
		[TestCase("[a~='b']")]
		[TestCase("[a$='b']")]
		[TestCase("[a!='b']")]
		[TestCase("#bob")]
		[TestCase(".myclass")]
		[TestCase("bob")]
		[TestCase("bob#tom.myclass")]
		[TestCase("bob,#tom,.myclass")]
		[TestCase("parent>child")]
		[TestCase("node+prevsib")]
		[TestCase("node~anyprevsib")]
		public void Selector_RoundTrip( string origCss ){ 
			// all of these selectors.ToString() should come back as the original string.
			// selectors return attribute-names in lowercase and attribute values quoted in single quotes (')
			var selector = new CssExpression( origCss );
			string andBack = selector.ToString();
			Assert.That( andBack , Is.EqualTo(origCss) );
		}

		CssChecker _checker = new CssChecker();

	}

}
