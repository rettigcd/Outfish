using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Outfish;

namespace Outfish_Test {

	[TestFixture]
	public class ParseTags	{
	
		HtmlNode Parse( string s ){ return new HtmlDocument( s ).DocumentNode; }
	
		[Test]
		public void SingleTag(){
		
			string html = "<body />";
			var node = this.Parse(html);
			
			Assert.True( node.IsEmpty );
			Assert.That( node.OuterHtml, Is.EqualTo(html) );
			Assert.That( node.InnerHtml, Is.EqualTo(string.Empty) );
			Assert.That( node.Name, Is.EqualTo("body") );
		
		}

		[Test]
		public void OpenCloseTag(){
		
			string html = "<body></body>";
			var node = this.Parse(html);
			
			Assert.That( node.ChildNodes.Count, Is.EqualTo(0) );
			Assert.That( node.OuterHtml, Is.EqualTo(html) );
			Assert.That( node.InnerHtml, Is.EqualTo(string.Empty) );
			Assert.That( node.Name, Is.EqualTo("body") );
		
		}
	
		[Test]
		public void OpenCloseTag_With_TextBody(){
		
			string html = "<body>hi</body>";
			var node = this.Parse(html);
			
			Assert.That( node.ChildNodes.Count, Is.EqualTo(1) );
			Assert.That( node.OuterHtml, Is.EqualTo(html) );
			Assert.That( node.InnerHtml, Is.EqualTo("hi") );
			Assert.That( node.Name, Is.EqualTo("body") );
		
		}

		[Test]
		public void OpenTag_With_TextBody(){
		
			string html = "<body>hi"; // missing tail
			var node = this.Parse(html);
			
			Assert.That( node.ChildNodes.Count, Is.EqualTo(1) );
			Assert.That( node.ChildNodes[0].GetType(), Is.EqualTo(typeof(TextNode)) );
			Assert.That( node.OuterHtml, Is.EqualTo(html) );
			Assert.That( node.InnerHtml, Is.EqualTo("hi") );
			Assert.That( node.Name, Is.EqualTo("body") );
		
		}

		[Test]
		public void OpenCloseTag_With_TagBody(){
		
			string inner = "<h1/>";
			string html = "<body>"+inner+"</body>";
			var node = this.Parse(html);
			
			Assert.That( node.ChildNodes.Count, Is.EqualTo(1) );
			Assert.That( node.ChildNodes[0].GetType(), Is.EqualTo(typeof(ContainerNode)) );
			Assert.That( node.ChildNodes[0].OuterHtml, Is.EqualTo(inner) );
			Assert.That( node.OuterHtml, Is.EqualTo(html) );
			Assert.That( node.InnerHtml, Is.EqualTo(inner) );
			Assert.That( node.Name, Is.EqualTo("body") );
		
		}

		[Test]
		public void CommentTag(){
			string html = "<root><!-- comment --></root>";
			var node = this.Parse( html );
			
			Assert.That( node.ChildNodes.Count, Is.EqualTo(1) );
			Assert.That( node.ChildNodes[0].GetType(), Is.EqualTo(typeof(CommentNode)) );
		}

		[Test]
		public void OpenCloseTag_With_ManyChildren(){
			List<string> children = new List<string>();
			children.Add("\r\n");
			children.Add("<h1/>");
			children.Add("<h2>bob</h2>");
			children.Add("   ");
			children.Add("<span name=ddd>fdfdfd</span>");
			children.Add("\r\n");
			string inner = string.Join("",children.ToArray());
			string html = "<body>"+inner+"</body>"; 
			var node = new HtmlDocument(html,HtmlParseOptions.KeepEmptyText).DocumentNode;
			
			Assert.That( node.InnerHtml, Is.EqualTo(inner) );
			Assert.That( node.OuterHtml, Is.EqualTo(html) );
			
			Assert.That( node.ChildNodes.Count, Is.EqualTo(children.Count) );
			int[] tagNodeIndex = new[]{1,2,4};
			for(int i=0;i<children.Count;++i){
				var childNode = node.ChildNodes[i];
				// check outter html
				Assert.That( childNode.OuterHtml, Is.EqualTo(children[i]) );
				// Check type
				if( tagNodeIndex.Contains(i) ){
					Assert.That( childNode, Is.InstanceOf(typeof(ContainerNode)));
				} else {
					Assert.That( childNode, Is.InstanceOf(typeof(TextNode)));
				}
			}
		}

		[Test]
		public void TailTag_Missing(){
			string html = "<root><script><bob>111111fff</script></root>";
			var node = this.Parse( html );
			var script = node.Find("script").First();
			
			// the /script tag should not be part of the inner
			Assert.True( script.InnerHtml.Contains("</script>") == false );
			// but is part of the outer
			Assert.True( script.OuterHtml.Contains("</script>") );
		}

		[Test]
		public void TailTag_Extra(){
			string html = "<root><script>111111fff</bob></script></root>";
			var node = this.Parse( html );
			var script = node.Find("script").First();
			
			// the /script tag should not be part of the inner
			Assert.True( script.InnerHtml.Contains("</script>") == false );
			// but is part of the outer
			Assert.True( script.OuterHtml.Contains("</script>") );
		}


		[Test]
		public void ScriptNodesHaveAtMost1TextChild(){
			// make sure errant <tag> in script comments don't become nodes
			string html = "<root><script> f <bob/> f </script></root>";
			var node = this.Parse( html );
			var script = node.Find("script").First();
			
			Assert.That( script.ChildNodes.Count, Is.EqualTo( 1 ) );
			Assert.That( script.FirstChild, Is.InstanceOf(typeof(TextNode)) );
		}

	}
}
