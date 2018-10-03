using System;
using System.Linq;
using NUnit.Framework;

using Outfish.JavaScript;

namespace Outfish_Test {

	[TestFixture]
	public class JsLLParser_Tests {
	
		void Test(string script){
			var x = new JsLLParser( script );
			x.Program();
			Assert.That( x.Cur, Is.EqualTo( x.TokenCount )
				, "Last Matched Sequence: " + x.LastMatchedSequence 
			);
		}
	
		[Test]
		public void var(){	this.Test("var x,y;");		}

		[Test]
		public void function(){	this.Test("function bob(){return ;}");		}

		[Test]public void empty_statements(){	this.Test(";;;");		}

		[Test]public void assignment(){	this.Test("a=1;");		}

		[Test]public void obj(){	this.Test("var p={a:true,b:null,c:'2'};");		}
		
		[Test]public void assign_to_null(){	this.Test("var p=null;");		}
		[Test]public void assign_to_int(){	this.Test("var p=1;");		}
		[Test]public void assign_to_string(){	this.Test("var p='null';");		}
		[Test]public void assign_to_bool(){	this.Test("var p=true;");		}


		[Test]public void CallExpression1(){	this.Test("aa.bb;"); }
		[Test]public void CallExpression2(){	this.Test("aa[1];"); }
		[Test]public void CallExpression3(){	this.Test("aa();"); }

		[Test]public void empty_for(){	this.Test("for(;;);");		}

		[Test]public void function_call(){	this.Test("fff();");		}
		
		[Test]public void id_change(){	this.Test("this.bob;");		}
		
		[Test]public void if_then(){	this.Test("if(true) a=1;");		}

		[Test]public void empty_block(){	this.Test("{}");		}
		
		[Test]public void return_no_semi(){	this.Test("return 1"); }

		[Test]public void p1(){	this.Test("var x={a:!!b,c:!!d};"); }
	}
	

	
}
