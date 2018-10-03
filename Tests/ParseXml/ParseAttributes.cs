using System.Reflection;
using NUnit.Framework;
using Outfish;

namespace Outfish_Test {

	[TestFixture]
	public class ParseAttributes {

		HtmlNode Parse(string s){ 
			return new HtmlDocument(s).DocumentNode;
		}

		[Test]
		public void Quoting(
			[Values("'","\"","")]string quoteType
		){
			string html = string.Format("<body name={0}bob{0}>",quoteType);
			var node = this.Parse( html );
			Assert.That( node["name"], Is.EqualTo("bob") );
		}

		[Test]
		public void AttributesWithNoValues_UsesNameAsValue() {
			string html = "<option selected>";
			var node = this.Parse( html );
			Assert.That( node["selected"], Is.EqualTo( "selected" ) );
		}

		[Test]
		public void NameCharacters( 
		 [Values("_name",":name","a1_-:.")] string name 
		){
			string html = @"<body "+name+"='bob'>";
			var node = this.Parse( html );
			Assert.That( node[name], Is.EqualTo( "bob"), "name was ["+name+"]" );
		}
		
		[Test]
		public void DuplicateAttributes_Dont_Throw_Exception(){
			string html = "<body id='bob' id='ted'>";
			var node = this.Parse( html ); 
			Assert.True(true); // shouldn't throw before we get here
			
		}

		[Test]
		public void Values(
			[Values("bob","a b","html://","\"double quotes\"")]string v
		){
			string html = "<body class='"+v+"'>";
			var node = this.Parse( html );
			Assert.That( node["class"], Is.EqualTo( v ) );
		}

		[Test]
		public void MissingCloseOfTag(){
			string html = "<body class=bob"; // end of tag is missing
			var node = this.Parse( html );
			Assert.That( node["class"], Is.EqualTo( "bob" ) );
		}

		[Test]
		public void NodeNameComesFirst(){
			// parsing attributes is slowww.......
			// we are delaying parsing them until something actually tries to read the attributes
			// therefore, if we are doing css path selection, we should always test name before 
			// we test attributes.
			
			// Given: a node that has attributes
			var node = this.Parse("<root a='a' b='b' ></rot>");
			//   And: a selector that does not match the nodes name
			var cssExpression = new CssExpression("tag.class#id:first-child");

			// When: the select checks the node
			bool isMatch = cssExpression.IsMatch( node );
			
			// Then: is not a match
			Assert.False( isMatch );
			//  And: attributes were never parsed/loaded
			object internalAttributes = node
				.GetType()
				.GetField("_attributes", BindingFlags.NonPublic|BindingFlags.Instance )
				.GetValue( node );
			Assert.IsNull( internalAttributes );
		}

	}
	
}
