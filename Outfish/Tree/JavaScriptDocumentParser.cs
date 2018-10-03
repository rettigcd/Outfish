using System;
using System.Collections.Generic;

using Outfish.JavaScript;

namespace Outfish {

	internal class JavaScriptDocumentParser {

		string _source;
		ContainerNode _currentNode;
		int _beginOfTextNode;
		List<JsToken> _tokenList;
		Stack<int> _openTokensIndexStack;
		JsToken _currentToken; // current

		public JavaScriptDocumentParser( string content ) {
			_source = content;
		}

		internal ContainerNode BuildStructureTree() {
			try { 
				return TryBuildStructureTree();
			} catch( Exception ex ) {
				throw new ParseException("Can't parse JavaScript document.", _source, ex);
			}
		}

		ContainerNode TryBuildStructureTree() {

			ContainerNode documentNode = BuildPhantomRootNode();

			_tokenList = BuildTokenList( documentNode );

			if( _tokenList.Count > 0 ) {
				_currentNode = documentNode;
				StackLoop();
				ConsumeText( documentNode.Footer.Begin );
				FixStructure( _currentNode );
			}

			return documentNode;
		}

		void StackLoop() {

			_beginOfTextNode = _tokenList[0].Begin;

			_openTokensIndexStack = new Stack<int>();

			for( int i = 0; i < _tokenList.Count; ++i ) {
				_currentToken = _tokenList[i];

				switch( _currentToken.Type ) {
					case JsTokenType.Semicolon:
						ConsumeText( _currentToken.End ); // include semicolon in text block
						break;
					case JsTokenType.Keyword:
						i = DoKeyword( i ); // might skip over some
						break;
					case JsTokenType.StringLiteral:
					case JsTokenType.NumberLiteral:
					case JsTokenType.BooleanLiteral:
					case JsTokenType.RegexLiteral:
						DoLiteral( i );
						break;
					case JsTokenType.Bracket:
						i = DoOpenOrClose( i );
						break;
				}// switch

			}
		}

		int DoOpenOrClose( int i ) {
			if( IsOpen )
				MoveDownIntoSubSection( i );
			else
				CloseAndMoveUpToParent();
			// happens if we have an extra close, 
			// causes exception if we try to keep going
			if( _currentNode == null ) i = _tokenList.Count;
			return i;
		}

		void DoLiteral( int i ) {
			// create node for it
			ConsumeText( _currentToken.Begin );
			string name = _currentToken.Type.ToString();
			name = name.Substring( 0, name.Length - 7 ); // strip 'Literal'
			var literalHeader = new HeaderFooter( _currentNode.Source, _currentToken.Begin, _currentToken.End, name, HeaderFooterType.Single );
			var literalNode = new ContainerNode( _source, literalHeader, new Dictionary<string, string>() );
			_currentNode.AddChild( literalNode );
			_beginOfTextNode = _currentToken.End;

			IdentifyContainerNode( i, literalNode );
		}

		int DoKeyword( int i ) {

			JsToken next = (i + 1) < _tokenList.Count ? _tokenList[i + 1] : null;

			// create node for it
			ConsumeText( _currentToken.Begin );
			var keywordHeader = new HeaderFooter( _source, _currentToken.Begin, _currentToken.End, _currentToken.Text, HeaderFooterType.Single );
			var keywordNode = new ContainerNode( _source, keywordHeader, new Dictionary<string, string>() );
			_currentNode.AddChild( keywordNode );
			_beginOfTextNode = _currentToken.End;

			// name function
			if( _currentToken.Text == "function" && next != null && next.Type == JsTokenType.Identifier ) {
				IdentifyNode( keywordNode, next.Text );
				++i; // skip id token
				_beginOfTextNode = next.End;
			}

			return i;
		}

		void CloseAndMoveUpToParent() {
			// add previous text node
			ConsumeText( _currentToken.Begin );

			// validate, pop, validate
			if( _openTokensIndexStack.Count > 0 ) {
				var openIndex = _openTokensIndexStack.Pop();
				if( !Match( _tokenList[openIndex], _currentToken ) ) { throw new Exception( "mismatch" ); }
			} else {
				// throw new Exception("missing close token."); }
			}

			// add footer
			_currentNode.Footer = new HeaderFooter( _currentNode.Source, _currentToken.Begin, _currentToken.End, "block", HeaderFooterType.Tail );

			FixStructure( _currentNode );

			// move up to parent
			_currentNode = _currentNode.ParentNode;

			// set new text-node start
			_beginOfTextNode = _currentToken.End;

		}

		void MoveDownIntoSubSection( int i ) {
			_openTokensIndexStack.Push( i );

			// add previous text node
			ConsumeText( _currentToken.Begin );

			// add new node
			var name = BracketName( _currentToken.Text );
			var header = new HeaderFooter( _currentNode.Source, _currentToken.Begin, _currentToken.End, "block", HeaderFooterType.Head );
			var blockNode = new ContainerNode( _source, header, new Dictionary<string, string> { { "class", name } } );
			_currentNode.AddChild( blockNode );

			IdentifyContainerNode( i, blockNode );

			// set new text-node start
			_beginOfTextNode = _currentToken.End;

			// move down to new node
			_currentNode = blockNode;
		}

		/// <summary>
		/// try to name the node using '=' or ':'
		/// </summary>
		void IdentifyContainerNode( int i, ContainerNode node ){
			
			if( i<2 ){ return; }
			
			string prevText = _tokenList[i-1].Text;
			if( prevText != "=" && prevText != ":" ){ return; }
			
			// get name
			JsToken identityToken = _tokenList[i-2];
			string identity = identityToken.Text;
			if( identityToken.Type == JsTokenType.StringLiteral ){
				identity = identity.Substring( 1, identity.Length-2 ); // strip ending quotes
			}
			// set it
			IdentifyNode( node, identity );
		
		}

		static void MoveNextSiblingToChild( List<HtmlNode> nodes, int pivotIndex ){
			if( nodes.Count <= pivotIndex + 1 ){ return; }
			ContainerNode pivotNode = nodes[pivotIndex] as ContainerNode;
			if( pivotNode == null ){ throw new Exception("pivot node is null"); }
			pivotNode.AddChild( nodes[pivotIndex+1] );
			nodes.RemoveAt(pivotIndex+1);
		}

		static string BracketName( string text ){
			switch( text ){
				case "[":
				case "]": return SquareName;
				case "{":
				case "}": return CurlyName;
				case "(":
				case ")": return RoundName;
			}
			throw new Exception("Bracket name for "+text);
		}

		void ConsumeText( int end ){
			// add previous text node
			if( _beginOfTextNode < end ){
				TextNode textNode = new TextNode( _source, _beginOfTextNode, end );
				if( textNode.IsWhiteSpace == false ){
					_currentNode.AddChild(textNode);
				}
				_beginOfTextNode = end; // advance
			}
		
		}

		static void FixStructure( ContainerNode node ){

			// maybe rename nodes to "condition" and "body"  or att attributes
			// see how variable names and functions pan out

			var isRound = AttributeSelector.ContainsClass( RoundName );
			var isCurly = AttributeSelector.ContainsClass( CurlyName );

			for(int i=0; i<node.ChildNodes.Count; ++i){
				HtmlNode child = node.ChildNodes[i];
				
				// restructure
				switch( child.Name ){
					case "function":
					case "while":
					case "with":  
					case "switch":
					case "for":
					case "if":
						// attach (...)
						if( Test( node, i+1, isRound.IsMatch )==false ){ break; }
						MoveNextSiblingToChild( node.ChildNodes, i );
						goto case "else"; // need body
					case "do":
					case "try":
					case "finally":
					case "else":
						// attach {}
						if( Test( node, i+1, isCurly.IsMatch )==false ){ break; }
						MoveNextSiblingToChild( node.ChildNodes, i );
						break;
				}
			}

		}

		static void IdentifyNode( ContainerNode node, string name ){
			// option 1 - harder to write/read css selector rules
//			node.SetAttr( "name", name ); 
			// option 2 - violates 1 id per doc rule.
			node.SetAttr( "id", name );
		}

		static bool Test( HtmlNode parent, int childIndex, Predicate<HtmlNode> pred ){
			if( childIndex < 0 || parent.ChildNodes.Count <= childIndex ){ return false; }
			var node = parent.ChildNodes[ childIndex ];
			return pred( node );
		}

		bool IsOpen => _currentToken.Type == JsTokenType.Bracket && "[({".Contains( _currentToken.Text );
	
		static bool Match( JsToken op, JsToken cl ){
			return "[]{}()".Contains( op.Text + cl.Text );
		}

		List<JsToken> BuildTokenList( ContainerNode node ){

			IEnumerable<JsToken> tokens = new JsLexer().GetTokens( 
				  _source
				, node.Header.End		// begin
				, node.Footer.Begin 		// end
			);
			
			// simple way to make list but it crashes if we hit invalid token
			// var tokenList = tokens.ToList();
			
			// more complicated but it survives invalid token exceptions
			List<JsToken> tokenList = new List<JsToken>();
			try{ 
				foreach( var token in tokens )
					tokenList.Add( token );
			}catch{} // incase we hit invalid javascript, we don't want the whole document to die
			return tokenList;
		}

		// builds/returns a phantom root node for string content
		ContainerNode BuildPhantomRootNode(){
			string source = _source;
			// create header at begining, footer at end
			HeaderFooter header = new HeaderFooter( source, 0, 0, "script", HeaderFooterType.Head );
			HeaderFooter footer = new HeaderFooter( source, source.Length, source.Length, "script",HeaderFooterType.Tail);

			// make node
			ContainerNode rootNode = new ContainerNode( _source, header, new Dictionary<string,string>{{"type","text/javascript"}} );
			rootNode.Footer = footer;
			return rootNode;
		}

		const string RoundName = "round";
		const string CurlyName = "curly";
		const string SquareName = "square";

	}
	
}
