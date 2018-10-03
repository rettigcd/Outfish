using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Outfish;

namespace Outfish_Test {

	[TestFixture]
	public class HtmlDocument_Test 	{

		[Test]
		public void NullSourceString_ThrowsArgumentNullException() {
			Assert.Throws<ArgumentNullException>(()=>new HtmlDocument(null));
		}

		[Test]
		public void FindIndexInText(){

			string html = "<node><b><td>fdfdf</td><br/>bob</b></node>";
			string needle = "bob";
			var doc = new HtmlDocument(html);
			int index = html.IndexOf(needle);
			
			var node = doc.FindNodeAt( index );
			
			Assert.That( node.OuterHtml, Is.EqualTo( needle ) );

		}

		[Test]
		public void ParseSimple_BPD_Node(){
			var html = @"<tokens><BPDToken></BPDToken><BPDToken></BPDToken></tokens>";
			var node = new HtmlDocument( html ).DocumentNode.FindFirst( "BPDToken" );
			Assert.False( node.InnerText.Contains("BPDToken") );
		}

		[TestCase("<root><bob></bob></root>")]
		[TestCase("<root><BOB></bob></root>")]
		[TestCase("<root><bob></BOB></root>")]
		[TestCase("<root><BOB></BOB></root>")]
		public void ParseSimple_BPD_Node(string html){
			var node = new HtmlDocument( html ).DocumentNode.FindFirst( "bob" );
			Assert.That( node.Begin, Is.EqualTo(6) );
			Assert.That( node.End, Is.EqualTo(17) );
		}

		[Test]
		public void ParseOutterHtml(){
			
			string html1 = @"<TRANSACTION ITEM_GROUP=""FFreedom""  ITEM_IDENTIFIER=""MY LOAN NUMBER""  TRANSACTION_DATE=""1/1/0001""  RECIPIENT_ID=""LPS""  SENDER_ID=""LSR""  TRANSACTION_TYPE=""STD_EVENT"" ><ATTACHMENTS><ATTACHMENT><DOCUMENT_ID>234232</DOCUMENT_ID>
<DOC_REF_NUMBER>MY LOAN NUMBER</DOC_REF_NUMBER></ATTACHMENT></ATTACHMENTS></TRANSACTION>";
			var root1 = new HtmlDocument( html1 ).DocumentNode;
			Assert.That( root1.OuterHtml, Is.EqualTo( html1 ) );

			string html = @"<TRANSACTION ITEM_GROUP=""FFreedom""  ITEM_IDENTIFIER=""MY LOAN NUMBER""  TRANSACTION_DATE=""1/1/0001""  RECIPIENT_ID=""LPS""  SENDER_ID=""LSR""  TRANSACTION_TYPE=""STD_EVENT"" ><ATTACHMENTS><ATTACHMENT><DOCUMENT_ID>234232</DOCUMENT_ID>
<DOC_REF_NUMBER>MY LOAN NUMBER</DOC_REF_NUMBER></ATTACHMENT></ATTACHMENTS></TRANSACTION>";
			var root = new HtmlDocument( html ).DocumentNode;
			Assert.That( root.OuterHtml, Is.EqualTo( html ) );
			
			var attachments = root.Find( "ATTACHMENTS ATTACHMENT" ).ToArray();
			Assert.That( attachments.Length, Is.EqualTo(1) );
			var attachment = attachments[0];
			var docIdNode = attachment.FindFirst("DOCUMENT_ID");
			var docRefNumberNode = attachment.FindFirst("DOC_REF_NUMBER");
			
			Assert.That( attachment.OuterHtml, Is.EqualTo(@"<ATTACHMENT><DOCUMENT_ID>234232</DOCUMENT_ID>
<DOC_REF_NUMBER>MY LOAN NUMBER</DOC_REF_NUMBER></ATTACHMENT>"));
			Assert.That( docIdNode.OuterHtml, Is.EqualTo(@"<DOCUMENT_ID>234232</DOCUMENT_ID>"));
			Assert.That( docRefNumberNode.OuterHtml, Is.EqualTo(@"<DOC_REF_NUMBER>MY LOAN NUMBER</DOC_REF_NUMBER>"));
						
			
		}
		
		[Test]
		public void FindIndexInTagAttribute(){

			string html = "<node><b><b>fdfd</b><td id='x' a='susie'>fdfdf</td><br/>bob</b></node>";
			string needle = "susie";
			var doc = new HtmlDocument(html);
			int index = html.IndexOf(needle);
			
			var node = doc.FindNodeAt( index );
			
			Assert.That( node["id"], Is.EqualTo( "x" ) );

		}

		[Test]
		public void TextNodesHaveParentNode(){
			HtmlNode textNode = new HtmlDocument("<root>blah</root>").FindNodeAt(7);
			Assert.That( textNode.OuterHtml, Is.EqualTo("blah") );
			Assert.That( textNode.ParentNode, Is.Not.Null );
		}

		[Test]
		public void TagNodesHaveParentNode(){
			HtmlNode tagNode = new HtmlDocument("<root><br aaa=11 /></root>").FindNodeAt(9);
			Assert.That( tagNode.Name, Is.EqualTo("br") );
			Assert.That( tagNode.ParentNode, Is.Not.Null );
		}

		[Test]
		public void CommentNodesHaveParentNode(){
			HtmlNode commentNode = new HtmlDocument("<root><!-- aaa --></root>").FindNodeAt(9);
			Assert.That( commentNode, Is.InstanceOf(typeof(CommentNode)) );
			Assert.That( commentNode.ParentNode, Is.Not.Null );
		}

		[Test]
		public void EmptyContentGeneratesNode(
			[Values(" ","")] string content
		){
			// make sure if they pass null or an empty string,
			// root node is non-null
			HtmlDocument doc = new HtmlDocument(content);
			Assert.That( doc.DocumentNode, Is.Not.Null );
		}

	}
	
}
