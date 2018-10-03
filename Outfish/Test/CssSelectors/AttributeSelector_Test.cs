using System.Linq;
using System.Xml;
using NUnit.Framework;
using Outfish;

namespace Outfish_Test {

	[TestFixture]
	public class AttributeSelector_Test {

		[Test]
		public void Contains(){
			this.ParseBoth("<root><aa bb='cc1cc'/><d e='ff1ff'/><g bb=''/></root>"
				,"[bb*='1']"
				,"aa"
			);
		}

		[Test]
		public void Searching_AttributeNode_For_Attribute_DoesntCrash(){
			// some nodes with no attributes will have attributes dictionary == null
			this.ParseBoth("<root><aa/><d/><g id='Street'/></root>"
				,"#Street"
				,"g"
			);
		
		}

		[Test]
		public void ContainsPrefix(){
			this.ParseBoth(@"<root>
							<a bb='pretable bbb'/>
							<d bb='ff  pre1ff'/>
							<g bb='fdfdpre'/>
						</root>"
				,"[bb|='pre']"
				,"a","d"
			);
		}


		[Test]
		public void StartsWith(){
			this.ParseBoth(@"<root>
							<a bb='startable bbb'/>
							<d bb='ff  start1ff'/>
							<g bb='fdfstartdpre'/>
							<f bb=''/>
						</root>"
				,"[bb^='start']"
				,"a"
			);
		}

		[Test]
		public void EndsWith(){
			this.ParseBoth(@"<root>
							<a bb='_suf'/>
							<b bb='ff  start1ff_suf'/>
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
			this.ParseHtml(@"<root>
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
			this.ParseBoth(@"<root>
							<a id='_suf'/>
							<b id='ten'/>
							<c id='ten ten' />
							<d id=''/>
						</root>"
				,"[id='ten']"
				,"b"
			);
		}

		[Test]
		public void NotEqualTo(){
			this.ParseBoth(@"<root>
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
			this.ParseBoth(@"<root>
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
			this.ParseBoth("<root><inner data-src='bob' /></root>", "[data-src]", "inner" );
		
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

		[Test]
		public void Parse_No_Quote_AttributeValue(){
			this.ParseBoth(@"<root>
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
		public void Easy_Id(){

			// (malformed ids don't parse using XmlNode)
			this.ParseHtml(@"<root>
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
			this.ParseHtml(@"<root>
							<a class='z aza z'/>
							<b class=bob />
							<c id='bob' />
							<d class='bob and son'/>
						</root>"
				,".bob"
				,"b","d"
			);
			
		}

		void ParseBoth( string html, string cssSelector, params string[] expected ) {
			ParseXml( html, cssSelector, expected );
			ParseHtml( html, cssSelector, expected );
		}


		void ParseXml( string html, string cssSelector, params string[] expected ) {

			// Given
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml( html );

			// When
			string[] matchedTagNames = xmlDoc.DocumentElement
				.Find( cssSelector )
				.Select(node=>node.Name)
				.ToArray();

			Assert_ArrayElementsEqual( matchedTagNames, expected );
		}

		void ParseHtml( string html, string cssSelector, params string[] expected ) {

			// Given
			var node = new HtmlDocument( html ).DocumentNode;

			// When
			string[] matchedTagNames = node.Find( cssSelector )
				.Select(x=>x.Name)
				.ToArray();

			Assert_ArrayElementsEqual( matchedTagNames, expected );
		}

		void Assert_ArrayElementsEqual( string[] actual, string[] expected ) {
			Assert.That( actual.Length, Is.EqualTo( expected.Length ) );

			for( int i = 0; i < expected.Length; ++i )
				Assert.That( actual[i], Is.EqualTo( expected[i] ) );
		}

	}



}
