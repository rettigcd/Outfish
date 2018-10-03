using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Outfish.JavaScript;

namespace Outfish_Test {

	[TestFixture]
	public class JsLexer_Tests {
	
		void TestWhiteSpaceDelimitedTokens( string script, JsTokenType type ){
			TestScriptAgainstKnownTokens( script, type, script.Split(' ') );
		}

		void TestScriptAgainstKnownTokens( string script, JsTokenType type, params string[] knownTokens ){
			// use parser to find tokens
			var parsed = new JsLexer()
				.GetTokens( script )
				.ToArray();
			// compare size
			Assert.That( parsed.Length, Is.EqualTo( knownTokens.Length ) );
			// compare individuals
			for( int i=0; i<knownTokens.Length; ++i){
				Assert.That( parsed[i].Text, Is.EqualTo( knownTokens[i] ) );
				Assert.That( parsed[i].Type, Is.EqualTo( type ), parsed[i].Text );
			}
		}

		[Test]
		public void Identifier_Parsing(){
			// inserted the 5 / 3 so that it doesn't match as regex
			string script = @"A Z a z _ $ $1 $$ _azAz09$_";
			this.TestWhiteSpaceDelimitedTokens( script, JsTokenType.Identifier );
		}

		[Test]
		public void Keyword_Parsing(){
			// inserted the 5 / 3 so that it doesn't match as regex
			string script = @"for function new delete while with do";
			this.TestWhiteSpaceDelimitedTokens( script, JsTokenType.Keyword );
		}


		[Test]
		public void Numeric_Parsing(){
			// inserted the 5 / 3 so that it doesn't match as regex
			string script = @"1 1. .1 1.1 +1 +1. +.1 +1.1 -1 -.1 -1. -1.1";
			this.TestWhiteSpaceDelimitedTokens( script, JsTokenType.NumberLiteral );
		}

		[Test]
		public void Operator_Parsing(){
			// all operators except / and /= because they require context to distinguish from regex literal
			string script = @". - + ~ ! * % + - < > & ^ | ? : = , ++ -- << >> <= >= && || == != *= %= += -= &= ^= |= <<= >>= === !== >>> >>>=";
			this.TestWhiteSpaceDelimitedTokens( script, JsTokenType.Operator );
		}

		[Test]
		public void Div_Operator_Parsing(){
			
			// need special parsing to find / and /= since we must distinguish between operator and regex literal.
			
			string script = @"2 / 4, a /= 3";

			// use parser to find tokens
			var parsed = new JsLexer()
				.GetTokens( script )
				.ToArray();
				
			// compare size
			Assert.That( parsed.Length, Is.EqualTo( 7 ) );
			Assert.That( parsed[1].Text, Is.EqualTo("/") );
			Assert.That( parsed[1].Type, Is.EqualTo(JsTokenType.Operator) );
			Assert.That( parsed[5].Text, Is.EqualTo("/=") );
			Assert.That( parsed[5].Type, Is.EqualTo(JsTokenType.Operator) );
		}


		[Test]
		public void Bracket_Parsing(){
			// / at beginning so it doesn't parse as regex
			string script = @"{ } [ ] ( )";
			this.TestWhiteSpaceDelimitedTokens( script, JsTokenType.Bracket );
		}


		[Test]
		public void Semicolon_Parsing(){
			// / at beginning so it doesn't parse as regex
			string script = @"; ; ; ; ; ;";
			this.TestWhiteSpaceDelimitedTokens( script, JsTokenType.Semicolon );
		}

		[Test]
		public void StringLiteral_Parsing(){
			this.TestScriptAgainstKnownTokens( @"  "" \"" 'bob ""    ' \' ""bob"" '   """"   '' 'fff'  "
				,JsTokenType.StringLiteral
				,@""" \"" 'bob """
				,@"' \' ""bob"" '"
				,@""""""
				,@"''"
				,@"'fff'"
			);
		}
		

		[Test]
		public void RegexLiteral_Parsing(){
			var tokens = new JsLexer().GetTokens( "x = /2222/iga").ToArray();
			Assert.That( tokens.Length, Is.EqualTo( 3 ) );
			Assert.That( tokens[2].Type, Is.EqualTo(JsTokenType.RegexLiteral) );
		}

		[Test]
		public void WhiteSpace_And_Comments(){
			var tokens = new JsLexer().GetTokens( "/*bob*/ // fffff 1 3 5 ").ToArray();
			Assert.That( tokens.Length, Is.EqualTo( 0 ) );
		}

		[Test]
		public void Keywords_Embedded_In_Identifier(){
			// 'do' is embedded inside of 'doc'
			var tokens = new JsLexer().GetTokens( "var a = doc;" ).ToArray();
			Assert.That( tokens.Length, Is.EqualTo( 5 ) );
		}

		[Test]
		public void BoolLit_Embedded_In_Identifier(){
			// 'do' is embedded inside of 'doc'
			var tokens = new JsLexer().GetTokens( "var trues = [], falses = [];" ).ToArray();
			Assert.That( tokens.Length, Is.EqualTo( 11 ) );
		}

		[Test]
		public void DoubleBang(){
			var tokens = new JsLexer().GetTokens("x = !!y;").ToArray();
			Assert.That( tokens.Length, Is.EqualTo( 6 ) );
		}

		static public string ReadFile(string filename ){
			string resourceName = "Outfish_Test.scripts." + filename;
			using( var s = typeof(JsLexer_Tests).Assembly.GetManifestResourceStream( resourceName )){
				using( var reader = new System.IO.StreamReader( s ) ){ 	
					return reader.ReadToEnd();
			  	}
			}
		}

	}
}
