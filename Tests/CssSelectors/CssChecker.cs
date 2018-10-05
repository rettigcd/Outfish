using System.Linq;
using System.Xml;
using NUnit.Framework;
using Outfish;

namespace Outfish_Test {

	public class CssChecker {

		public void Parse( string html, string cssSelector, params string[] expected ) {
			ParseXml( html, cssSelector, expected );
			ParseHtml( html, cssSelector, expected );
		}

		public void ParseXml( string html, string cssSelector, params string[] expected ) {

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

		public void ParseHtml( string html, string cssSelector, params string[] expected ) {

			// Given
			var node = new HtmlDocument( html ).DocumentNode;

			// When
			string[] matchedTagNames = node.Find( cssSelector )
				.Select(x=>x.Name)
				.ToArray();

			Assert_ArrayElementsEqual( matchedTagNames, expected );
		}


		public void Assert_ArrayElementsEqual( string[] actual, string[] expected ) {
			Assert.That( actual.Length, Is.EqualTo( expected.Length ) );

			for( int i = 0; i < expected.Length; ++i )
				Assert.That( actual[i], Is.EqualTo( expected[i] ) );
		}
	}



}
