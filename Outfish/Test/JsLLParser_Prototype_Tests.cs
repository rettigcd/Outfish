using System;
using System.Linq;
using NUnit.Framework;

using Outfish.JavaScript;

namespace Outfish_Test {

	// this is a test class for the JsLLParser that has a bug in it.
	// I'm keeping these in the repository but removing them from Outfish
	// since they aren't begin used.


	[TestFixture]
	public class JsLLParser_Prototype_Tests {
	
		[Test]public void oring_object_props(){	
			// replacing || with + works
			this.Test(@"var constructor = window.Element || window.HTMLElement;"); 
			
		}
	
		[Test]
		public void xxxx(){
			this.Test(@"if ((pair = pair.split('='))[0]) {  x=1;  }");
		}
	
		[Test]
		public void yyyyy(){
			string s = @"var CONCAT_ARGUMENTS_BUGGY = (function() {return [].concat(arguments)[0][0] !== 1;})(1,2);	";
			this.Test( s );
		}
	
		[Test]
		public void zzz(){
			// something is failing with this test
			// but LL(*) is too hard to debug :-(
			// if we change extras[i+1] to extras[i]; it passes.  why?
			string s = @"{  headers = extras[i+1];
				$H(extras).each(
					function(pair) { }
				);
			}";
			this.Test( s );
		}


	
		// not running this test because LL(*) can't report location of error so I don't know how to fix it.
		[Test]
		public void parse_prototype(){	
			string script = JsLexer_Tests.ReadFile("prototype.js");
			
			int lineCount = 1402; // 1133; // 960
			int index = new Outfish.LineManager( script ).GetIndex( lineCount-1, 0 );
			
//			this.Test( script.Substring(0, index ) );
			this.Test( script );
		}
	
		void Test(string script){
			var x = new JsLLParser( script );
			x.Program();
			Assert.That( x.Cur, Is.EqualTo( x.TokenCount )
				, "Last Matched Sequence: " + x.LastMatchedSequence 
			);
		}
	
	}
}
