using System;
using System.Collections.Generic;
using System.Linq;

namespace Outfish.JavaScript {

//	http://www.rpatk.net/web/en/parsejavascript.php
//	http://tomcopeland.blogs.com/EcmaScript.html#prod2
//	http://goldparser.org/grammars/files/JavaScript.zip
//	http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-262.pdf

	// This LL(*) parser for Javascript is mostly correct but a bug was disconvered
	// while trying to parse prototype.js.  (See test class.)  
	// Since LL(*):
	//     - is so hard to debug I was unable to figure out what the problem
	//     - runs in exponential time (is very very slow)
	// 
	// I've decided not to include it in Outfish.
	// Bit it is an educational reference so I'll keep it in the repository


	/// <summary>
	/// Recusrive Descent LL Parser for Javascript with unknown predictivity
	/// </summary>
	/// <remarks>might take exponential time to parse</remarks>
	public class JsLLParser {
	
		public JsLLParser( string host ) {
			_host = host;
			_tokenList = new JsLexer().GetTokens(host).ToList();
			SemiColon = new Bool( Token(";") );
			Comma = new Bool( Token(",") );
		}

		#region private fields

		List<JsToken> _tokenList;

		public int _cur = 0;

		Bool SemiColon;
		Bool Comma;

		public string _host;

		#endregion

		public int Cur{ get{ return _cur; } }

		public int TokenCount{ get{ return _tokenList.Count; } }

		#region helpers

		public bool MatchAny(params string[] options){
			if( _cur < _tokenList.Count
				&& options.Contains( _tokenList[_cur].Text ) 
			){
				++_cur;
				return true;
			}
			return false;
		}

		public bool MatchAny(params JsTokenType[] options){
			if( _cur < _tokenList.Count 
				&& options.Contains( _tokenList[_cur].Type ) 
			){
				++_cur;
				return true;
			}
			return false;
		}


		public bool Sequence( params Func<bool>[] actions ){
			int start = _cur; // record start
			foreach( var f in actions ){
				if( f() == false ){
					_cur = start;
					return false;
				}
			}
			LogRange( start );
			return true;
		}

		void LogRange(int beginTokenIndex){
			int newEnd = _cur < this._tokenList.Count 
				? this._tokenList[_cur].End
				: this._host.Length;
			if( newEnd >= _end ){
				_begin = this._tokenList[beginTokenIndex].Begin;
				_end = newEnd;
			}
			//Console.WriteLine( _begin + " " + _end );
		}

		LineManager _lineManager = null;
		int _begin;
		int _end;
		public string LastMatchedSequence{
			get{
				int len = _end - _begin;
				int line, col;
				_lineManager = _lineManager ?? new LineManager( this._host );
				_lineManager.GetLineCol( _begin, out line, out col );
				return string.Format(@"(Ln {0} Col {1}) +{2} ""{3}"""
					, line+1, col+1
					, len
					, _host.Substring(_begin, len) 
				);
			}
		}

		#endregion

		#region Func<bool>

		public Bool MakeSeq( params Func<bool>[] actions ){
			return new Bool(() => this.Sequence( actions ));
		}

		public Func<bool> Token( params string[] options ){
			return () => MatchAny( options );
		}

		
		#endregion
		
		#region tokens

		//PostfixOperator 	::= 	( "++" | "--" )
		public bool PostfixOperation(){ return MatchAny( "++", "--" );	}

		//UnaryOperator 	::= 	( "delete" | "void" | "typeof" | "++" | "--" | "+" | "-" | "~" | "!" )
		public bool UnaryOperator(){ return MatchAny("delete","void","typeof","++","--","+","-","~","!"); }

		// Literal 	::= 	( <DECIMAL_LITERAL> | <HEX_INTEGER_LITERAL> | <STRING_LITERAL> | <BOOLEAN_LITERAL> | <NULL_LITERAL> | <REGULAR_EXPRESSION_LITERAL> )
		public bool Literal(){return MatchAny( JsTokenType.NumberLiteral, JsTokenType.StringLiteral, JsTokenType.BooleanLiteral, JsTokenType.RegexLiteral ); } // plus , JsTokenType.HexLit

		// Identifier 	::= 	<IDENTIFIER_NAME>
		public bool Identifier(){ return MatchAny( JsTokenType.Identifier ); }

		public bool StringLiteral(){ return MatchAny( JsTokenType.StringLiteral ); }
		
		public bool NumLiteral(){ return MatchAny( JsTokenType.NumberLiteral ); }

		//AdditiveOperator 	::= 	( "+" | "-" )
		public bool AdditiveOperator(){ return MatchAny("+","-"); }

		//AssignmentOperator 	::= 	( "=" | "*=" | <SLASHASSIGN> | "%=" | "+=" | "-=" | "<<=" | ">>=" | ">>>=" | "&=" | "^=" | "|=" )
		public bool AssignmentOperator(){ return MatchAny("=","*=","/=","%=", "+=", "-=", "<<=", ">>=", ">>>=", "&=", "^=", "|=" ); }

		//EmptyStatement 	::= 	";"
		public bool EmptyStatement(){ return MatchAny(";"); }

		//MultiplicativeOperator 	::= 	( "*" | <SLASH> | "%" )
		public bool MultiplicativeOperator(){ return MatchAny("*","/","%"); }
			
		//ShiftOperator 	::= 	( "<<" | ">>" | ">>>" )
		public bool ShiftOperator(){ return MatchAny("<<",">>",">>>"); }
			
		//RelationalOperator 	::= 	( "<" | ">" | "<=" | ">=" | "instanceof" | "in" )
		public bool RelationalOperator(){ return MatchAny("<",">","<=",">=","instanceof","in"); }

		//RelationalNoInOperator 	::= 	( "<" | ">" | "<=" | ">=" | "instanceof" )
		public bool RelationalNoInOperator(){ 	return MatchAny("<",">","<=",">=","instanceof");	}

		//EqualityOperator 	::= 	( "==" | "!=" | "===" | "!==" )
		public bool EqualityOperator(){ return MatchAny("==","!=","===","!=="); }

		//BitwiseANDOperator 	::= 	"&"
		public bool BitwiseANDOperator(){ return MatchAny("&"); }

		//BitwiseXOROperator 	::= 	"^"
		public bool BitwiseXOROperator(){ return MatchAny("^"); }
			
		//BitwiseOROperator 	::= 	"|"
		public bool BitwiseOROperator(){ return MatchAny("|"); }
			
		//LogicalANDOperator 	::= 	"&&"
		public bool LogicalANDOperator(){ return MatchAny("&&"); }
			
		//LogicalOROperator 	::= 	"||"
		public bool LogicalOROperator(){ return MatchAny("||"); }

		#endregion

		#region NoIn
		
		public bool VariableDeclarationListNoIn(){ 
			//VariableDeclarationListNoIn 	::= 	VariableDeclarationNoIn ( "," VariableDeclarationNoIn )*
			return Sequence( VariableDeclaratoinNoIn, MakeSeq( Token(","), VariableDeclaratoinNoIn ).Star );
		}

		public bool InitialiserNoIn(){ 
			//InitialiserNoIn 	::= 	"=" AssignmentExpressionNoIn
			return Sequence(Token("="), AssignmentExpressionNoIn );
		}

		public bool ExpressionNoIn(){ 
			//ExpressionNoIn 	::= 	AssignmentExpressionNoIn ( "," AssignmentExpressionNoIn )*
			return Sequence( AssignmentExpressionNoIn,MakeSeq( Token(","), AssignmentExpressionNoIn).Star );
		}

		public bool RelationalExpressionNoIn(){ 
			//RelationalExpressionNoIn 	::= 	ShiftExpression ( RelationalNoInOperator ShiftExpression )*
			return Sequence( ShiftExpression, MakeSeq( RelationalNoInOperator, ShiftExpression ).Star );
		}

		public bool EqualityExpressionNoIn(){ 
			//EqualityExpressionNoIn 	::= 	RelationalExpressionNoIn ( EqualityOperator RelationalExpressionNoIn )*
			return Sequence( RelationalExpressionNoIn, MakeSeq( EqualityOperator, RelationalExpressionNoIn ).Star );
		}

		public bool BitwiseANDExpressionNoIn(){ 
			//BitwiseANDExpressionNoIn 	::= 	EqualityExpressionNoIn ( BitwiseANDOperator EqualityExpressionNoIn )*
			return Sequence( EqualityExpressionNoIn, MakeSeq( BitwiseANDOperator, EqualityExpressionNoIn ).Star );
		}

		public bool BitwiseXORExpressionNoIn(){ 
			//BitwiseXORExpressionNoIn 	::= 	BitwiseANDExpressionNoIn ( BitwiseXOROperator BitwiseANDExpressionNoIn )*
			return Sequence( BitwiseANDExpressionNoIn, MakeSeq( BitwiseXOROperator, BitwiseANDExpressionNoIn ).Star );
		}
			
		public bool BitwiseORExpressionNoIn(){
			//BitwiseORExpressionNoIn 	::= 	BitwiseXORExpressionNoIn ( BitwiseOROperator BitwiseXORExpressionNoIn )*
			return Sequence( BitwiseXORExpressionNoIn, MakeSeq( BitwiseOROperator, BitwiseXORExpressionNoIn ).Star );
		}
			
		public bool LogicalANDExpressionNoIn(){
			//LogicalANDExpressionNoIn 	::= 	BitwiseORExpressionNoIn ( LogicalANDOperator BitwiseORExpressionNoIn )*
			return Sequence( BitwiseORExpressionNoIn, MakeSeq( LogicalANDOperator, BitwiseORExpressionNoIn ).Star );
		}
			
		public bool LogicalORExpressionNoIn(){
			//LogicalORExpressionNoIn 	::= 	LogicalANDExpressionNoIn ( LogicalOROperator LogicalANDExpressionNoIn )*
			return Sequence( LogicalANDExpressionNoIn, MakeSeq( LogicalOROperator, LogicalANDExpressionNoIn ).Star );
		}

		public bool VariableDeclaratoinNoIn(){ 
			//VariableDeclarationNoIn 	::= 	Identifier ( InitialiserNoIn )?
			return Sequence( Identifier, new Bool( InitialiserNoIn ).Optional );
		}
			
		public bool ConditionalExpressionNoIn(){ 
			//ConditionalExpressionNoIn 	::= 	LogicalORExpressionNoIn ( "?" AssignmentExpression ":" AssignmentExpressionNoIn )?
			return Sequence( LogicalORExpressionNoIn, MakeSeq( Token("?"), AssignmentExpression, Token(":"), AssignmentExpressionNoIn ).Optional );
		}
			
		public bool AssignmentExpressionNoIn(){ 
			//AssignmentExpressionNoIn 	::= 	( LeftHandSideExpression AssignmentOperator AssignmentExpressionNoIn | ConditionalExpressionNoIn )
			return Sequence( LeftHandSideExpression, AssignmentOperator, AssignmentExpressionNoIn )
				|| ConditionalExpressionNoIn();
		}

		#endregion

		// ============================================
		// ============================================
		// ============================================
		
		public bool PrimaryExpression(){
			//		
			//PrimaryExpression 	::= 	"this"
			//	| 	ObjectLiteral
			//	| 	( "(" Expression ")" )
			//	| 	Identifier
			//	| 	ArrayLiteral
			//	| 	Literal		

			return MatchAny( "this" )
				||	ObjectLiteral()
				||	Sequence( Token("("), Expression, Token(")") ) 
				||	Identifier()
				||	ArrayLiteral()
				||	Literal();
		}

		public bool ArrayLiteral(){	
			// ArrayLiteral 	::= 	"[" (
			//		Elision? | ElementList Elision | ElementList?
			//	) "]"
			//ElementList 	::= 	( Elision )? AssignmentExpression ( Elision AssignmentExpression )*
			if( Token("[")() == false ){ 
				return false; 
			}
			var close = Token("]");
			return Sequence( Comma.Star, close )
				||	Sequence( Comma.Star, AssignmentExpression
					, MakeSeq( Comma.Plus, AssignmentExpression).Star, Comma.Star, close );
		}

		public bool Elision(){
			//Elision 	::= 	( "," )+
			return new Bool( Token(",") ).Plus();
		}
		
		public bool FunctionalDeclaration(){
			//FunctionDeclaration 	::= 	"function" Identifier ( "(" ( FormalParameterList )? ")" ) FunctionBody
			return Sequence( Token("function"), Identifier, Token("("), new Bool(FormalParameterList).Optional, Token(")"), FunctionBody );
		}	
			
		public bool FormalParameterList (){ 
			//FormalParameterList 	::= 	Identifier ( "," Identifier )*
			return Sequence( Identifier, MakeSeq( Token(","), Identifier ).Star );
		}
			
		public bool FunctionBody(){ 
			//FunctionBody 	::= 	"{" ( SourceElements )? "}"
			return Sequence( Token("{"), new Bool(SourceElement).Star, Token("}") );
		}
			
		public bool Program(){
			//Program 	::= 	( SourceElements )? <EOF>
			return new Bool(SourceElement).Star();
		}
		
		public bool MemberExpression(){ 
			//MemberExpression 	::= 	( ( FunctionExpression | PrimaryExpression ) ( MemberExpressionPart )* )
			//	| 	AllocationExpression
			if( AllocationExpression() ){ return true; }
			if( !FunctionExpression() && !PrimaryExpression() ){ return false; }
			while( MemberExpressionPart() );
			return true;
		}
			
		public bool CallExpression(){ 
			//CallExpression 	::= 	MemberExpression Arguments ( CallExpressionPart )*
			return Sequence( MemberExpression, Arguments, new Bool( CallExpressionPart ).Star );
		}
			
		public bool CallExpressionForIn(){ 
			//CallExpressionForIn 	::= 	MemberExpressionForIn Arguments ( CallExpressionPart )*
			if( !Sequence( MemberExpressionForIn, Arguments ) ){ return false; }
			while( CallExpressionPart() );
			return true;
		}
			
		public bool CallExpressionPart(){ 
			//CallExpressionPart 	::= 	Arguments
			//	| 	( "[" Expression "]" )
			//	| 	( "." Identifier )
			return Arguments()
				|| Sequence(Token("["), Expression, Token("]"))
				|| Sequence(Token("."), Identifier);
		}
			
		public bool Arguments(){ 
			//Arguments 	::= 	"(" ( ArgumentList )? ")"
			return Sequence( Token("("), new Bool( ArgumentList ).Optional, Token(")") );
		}
			
		public bool LeftHandSideExpression(){ 
			//LeftHandSideExpression 	::= 	CallExpression
			//	| 	MemberExpression
			return CallExpression()
				|| MemberExpression();
		}
			
		public bool AdditiveExpression(){ 
			//AdditiveExpression 	::= 	MultiplicativeExpression ( AdditiveOperator MultiplicativeExpression )*
			return Sequence( MultiplicativeExpression, MakeSeq( AdditiveOperator, MultiplicativeExpression ).Star );
		}

		public bool ConditionalExpression(){ 
			//ConditionalExpression 	::= 	LogicalORExpression ( "?" AssignmentExpression ":" AssignmentExpression )?
			return Sequence( LogicalORExpression, MakeSeq( Token("?"), AssignmentExpression, Token(":"), AssignmentExpression ).Optional );
		}
			
		public bool AssignmentExpression(){
			//AssignmentExpression 	::= 	( LeftHandSideExpression AssignmentOperator AssignmentExpression | ConditionalExpression )
			return Sequence( LeftHandSideExpression, AssignmentOperator, AssignmentExpression )
				|| ConditionalExpression();
		}
		
		public bool Expression(){ 
			//Expression 	::= 	AssignmentExpression ( "," AssignmentExpression )*
			return Sequence( AssignmentExpression, MakeSeq( Token(","), AssignmentExpression ).Star	);
		}
		
		// this is the magic method that allows the parser to survive errors and report issues
		public bool Statement_Star(){
			// get all valid statements
			int start = this._cur;
			while( this.Statement() ){
				this.LogRange(start);
				start = _cur;
			}
			// addvance to next "case" "default" or "}"
			
			// if we had an error, advance to next statement_star termeinal
			var terminals = new string[]{"case","default","}"};
			while( this._cur < this._tokenList.Count && !terminals.Contains( this._tokenList[this._cur].Text ) ){ 
				this._cur ++;
			}
			return true;
		}
			
		public bool Statement(){
			//Statement 	::= 	Block
			//	| 	JScriptVarStatement
			//	| 	VariableStatement
			//	| 	EmptyStatement
			//	| 	LabelledStatement
			//	| 	ExpressionStatement
			//	| 	IfStatement
			//	| 	IterationStatement
			//	| 	ContinueStatement
			//	| 	BreakStatement
			//	| 	ImportStatement
			//	| 	ReturnStatement
			//	| 	WithStatement
			//	| 	SwitchStatement
			//	| 	ThrowStatement
			//	| 	TryStatement
			return Block()
				|| JScriptVarDeclaration()
				|| VariableStatement()
				|| EmptyStatement()
				|| LabelledStatement()
				|| ExpressionStatement()
				|| IfStatement()
				|| IterationStatement()
				|| ContinueStatement()
				|| BreakStatement()
				|| ImportStatement()
				|| ReturnStatement()
				|| WithStatement()
				|| SwitchStatement()
				|| ThrowStatement()
				|| TrySTatement();
		}
			
		public bool Block(){ 
			//Block 	::= 	"{" ( StatementList )? "}"
			return Sequence( Token("{"), Statement_Star, Token("}") );
		}
						
		public bool VariableStatement(){ 
			//VariableStatement 	::= 	"var" VariableDeclarationList ( ";" )?
			return Sequence(Token("var"), VariableDeclarationList, Token(";") );
		}
			
		public bool VariableDeclarationList(){ 
			//VariableDeclarationList 	::= 	VariableDeclaration ( "," VariableDeclaration )*
			return Sequence( VariableDeclaration, MakeSeq( Token(","), VariableDeclaration ).Star );
		}
		
		public bool VariableDeclaration(){ 
			//VariableDeclaration 	::= 	Identifier ( Initialiser )?
			return Sequence( Identifier, new Bool(Initialiser).Optional );
		}
			
		public bool Initialiser(){
			//Initialiser 	::= 	"=" AssignmentExpression
			return Sequence( Token("="), AssignmentExpression );
		}
			
		public bool SourceElement(){
			//SourceElement 	::= 	FunctionDeclaration
			//	| 	Statement
			return FunctionalDeclaration()
				|| Statement();
		}
			
		public bool ExpressionStatement(){
			//ExpressionStatement 	::= 	Expression ( ";" )?
			return Sequence( Expression, Token(";") );
		}
		
		public bool ReturnStatement(){ 
			//ReturnStatement 	::= 	"return" ( Expression )? ( ";" )?
			return Sequence( Token("return"), new Bool(Expression).Optional, SemiColon.Optional );
		}

		public bool Name(){ 
			//Name 	::= 	<IDENTIFIER_NAME> ( "." <IDENTIFIER_NAME> )*
			return Sequence( Identifier, MakeSeq( Token("."), Identifier ).Star );
		}

		public bool ObjectLiteral(){ 
			//ObjectLiteral 	::= 	"{" ( PropertyNameAndValueList )? "}"
			return Sequence(Token("{"), new Bool( PropertyNameAndValueList ).Optional, Token("}") );
		}

		public bool PropertyNameAndValueList(){ 
			//PropertyNameAndValueList 	::= 	PropertyNameAndValue ( "," PropertyNameAndValue | "," )*
			if( PropertyNameAndValue()==false){ return false; }
			while( this.Sequence( Token(","), new Bool( PropertyNameAndValue ).Optional ) );
			return true;
		}

		public bool PropertyNameAndValue(){ 
			//PropertyNameAndValue 	::= 	PropertyName ":" AssignmentExpression
			return Sequence( PropertyName, Token(":"), AssignmentExpression );
		}

		public bool PropertyName(){ 
			//PropertyName 	::= 	Identifier
			//	| 	<STRING_LITERAL>
			//	| 	<DECIMAL_LITERAL>
			return Identifier()
				|| StringLiteral()
				|| NumLiteral();
		}

		public bool WithStatement(){ 
			//WithStatement 	::= 	"with" "(" Expression ")" Statement
			return Sequence( Token("with"), Token("("), Expression, Token(")"), Statement );
		}

		public bool SwitchStatement(){
			//SwitchStatement 	::= 	"switch" "(" Expression ")" CaseBlock
			return Sequence( Token("switch"), Token("("), Expression, Token(")"), CaseBlock );
		}

		public bool CaseBlock(){ 
			//CaseBlock 	::= 	"{" ( CaseClauses )? ( "}" | DefaultClause ( CaseClauses )? "}" )
			Func<bool> caseClauses = new Bool( CaseClause ).Star;
			return Sequence( Token("{"), caseClauses, MakeSeq( DefaultClause, caseClauses ).Optional, Token("}") );
		}

		public bool CaseClause(){ 
			//CaseClause 	::= 	( ( "case" Expression ":" ) ) ( StatementList )?
			return Sequence( Token("case"), Expression, Token(":"), Statement_Star );
		}

		public bool DefaultClause(){
			//DefaultClause 	::= 	( ( "default" ":" ) ) ( StatementList )?
			return Sequence( Token("default"), Token(":"), Statement_Star );
		}

		public bool LabelledStatement(){ 
			//LabelledStatement 	::= 	Identifier ":" Statement
			return Sequence( Identifier, Token(":"), Statement );
		}

		public bool ThrowStatement(){ 
			//ThrowStatement 	::= 	"throw" Expression ( ";" )?
			return Sequence( Token("throw"), Expression, SemiColon.Optional );
		}

		public bool TrySTatement(){ 
			//TryStatement 	::= 	"try" Block ( ( Finally | Catch ( Finally )? ) )
			return Sequence( Token("try"), Block, new Bool( Catch ).Optional, new Bool( Finally ).Optional );
			// ! note - I said the catch and the Finally are both optional but production above says 1 is required
		}
			
		public bool Catch(){ 
			//Catch 	::= 	"catch" "(" Identifier ")" Block
			return Sequence( Token("catch"), Token("("), Identifier, Token(")"), Block );
		}
			
		public bool Finally(){
			//Finally 	::= 	"finally" Block
			return Sequence( Token("finally"), Block );
		}

		public bool FunctionExpression(){ 
			//FunctionExpression 	::= 	"function" ( Identifier )? ( "(" ( FormalParameterList )? ")" ) FunctionBody
			return Sequence( Token("function"), new Bool( Identifier ).Optional
				, Token("("), new Bool( FormalParameterList ).Optional, Token(")")
				, FunctionBody 
			);
		}
		
		public bool ImportStatement(){ 
			//ImportStatement 	::= 	"import" Name ( "." "*" )? ";"
			return Sequence( Token("import"), Name, MakeSeq( Token("."), Token("*") ).Optional, Token(";") );
		}

		public bool JScriptVarStatement(){ 
			//JScriptVarStatement 	::= 	"var" JScriptVarDeclarationList ( ";" )?
			return Sequence( Token("var"), JScriptVarDeclarationList, SemiColon.Optional );
		}
			
		public bool JScriptVarDeclarationList(){ 
			//JScriptVarDeclarationList 	::= 	JScriptVarDeclaration ( "," JScriptVarDeclaration )*
			return Sequence( JScriptVarDeclaration, MakeSeq( Token(","), JScriptVarDeclaration ).Star );
		}
			
		public bool JScriptVarDeclaration(){ 
			//JScriptVarDeclaration 	::= 	Identifier ":" <IDENTIFIER_NAME> ( Initialiser )?
			return Sequence( Identifier, Token(":"), Identifier, new Bool( Initialiser ).Optional );
			// ??? whats the difference between Identifier and <IDENTIFIER_NAME>?
		}
			
		public bool MemberExpressionForIn(){ 
			//MemberExpressionForIn 	::= 	( ( FunctionExpression | PrimaryExpression ) ( MemberExpressionPart )* )
			if( !FunctionExpression() && !PrimaryExpression() ){ return false; }
			while( MemberExpressionPart() );
			return true;
		}
			
		public bool AllocationExpression(){ 
			//AllocationExpression 	::= 	( "new" MemberExpression ( ( Arguments ( MemberExpressionPart )* )* ) )
			return Sequence( Token("new"), MemberExpression, MakeSeq( Arguments, new Bool( MemberExpressionPart ).Star ).Star );
		}
			
		public bool MemberExpressionPart(){ 
			//MemberExpressionPart 	::= 	( "[" Expression "]" )
			//	| 	( "." Identifier )
			return Sequence( Token("["), Expression, Token("]") )
				|| Sequence( Token("."), Identifier );
		}


		public bool ContinueStatement(){ 
			//ContinueStatement 	::= 	"continue" ( Identifier )? ( ";" )?
			return Sequence( Token("continue"), new Bool( Identifier ).Optional, SemiColon.Optional );
		}
			
		public bool BreakStatement(){ 
			//BreakStatement 	::= 	"break" ( Identifier )? ( ";" )?
			return Sequence( Token("break"), new Bool( Identifier ).Optional, SemiColon.Optional );
		}

		public bool ArgumentList(){ 
			//ArgumentList 	::= 	AssignmentExpression ( "," AssignmentExpression )*
			return Sequence( AssignmentExpression, MakeSeq( Token(","), AssignmentExpression ).Star );
		}
		
		public bool LeftHandSideExpressionForIn(){
			//LeftHandSideExpressionForIn 	::= 	CallExpressionForIn
			//	| 	MemberExpressionForIn
			return CallExpressionForIn() 
				|| MemberExpressionForIn();
		}
		
		public bool PostfixExpression(){ 
			//PostfixExpression 	::= 	LeftHandSideExpression ( PostfixOperator )?
			return Sequence( LeftHandSideExpression, new Bool( PostfixOperation ).Optional );
		}

		public bool UnaryExpression(){ 
			//UnaryExpression 	::= 	( PostfixExpression | ( UnaryOperator UnaryExpression )+ )
			return PostfixExpression()
				|| MakeSeq( UnaryOperator, UnaryExpression ).Plus();
		}
		
		public bool MultiplicativeExpression(){ 
			//MultiplicativeExpression 	::= 	UnaryExpression ( MultiplicativeOperator UnaryExpression )*
			return Sequence( UnaryExpression, MakeSeq( MultiplicativeOperator, UnaryExpression ).Star );
		}
		
		public bool ShiftExpression(){ 
			//ShiftExpression 	::= 	AdditiveExpression ( ShiftOperator AdditiveExpression )*
			return Sequence( AdditiveExpression, MakeSeq( ShiftOperator, AdditiveExpression ).Star );
		}

		public bool RelationalExpression(){
			//RelationalExpression 	::= 	ShiftExpression ( RelationalOperator ShiftExpression )*
			return Sequence( ShiftExpression, MakeSeq( RelationalOperator, ShiftExpression ).Star );
		}
		
		public bool EqualityExpression(){ 
			//EqualityExpression 	::= 	RelationalExpression ( EqualityOperator RelationalExpression )*
			return Sequence( RelationalExpression, MakeSeq( EqualityOperator, RelationalExpression ).Star );
		}
		
		public bool BitwiseANDExpression(){ 
			//BitwiseANDExpression 	::= 	EqualityExpression ( BitwiseANDOperator EqualityExpression )*
			return Sequence( EqualityExpression, MakeSeq( BitwiseANDOperator, EqualityExpression ).Star );
		}
		
		public bool BitwiseXORExpression(){ 
			//BitwiseXORExpression 	::= 	BitwiseANDExpression ( BitwiseXOROperator BitwiseANDExpression )*	
			return Sequence( BitwiseANDExpression, MakeSeq( BitwiseXOROperator, BitwiseANDExpression ).Star );
		}
		
		public bool BitwiseORExpression(){ 
			//BitwiseORExpression 	::= 	BitwiseXORExpression ( BitwiseOROperator BitwiseXORExpression )*
			return Sequence( BitwiseXORExpression, MakeSeq( BitwiseOROperator, BitwiseXORExpression ).Star );
		}
			
		public bool LogicalANDExpression(){ 
			//LogicalANDExpression 	::= 	BitwiseORExpression ( LogicalANDOperator BitwiseORExpression )*
			return Sequence( BitwiseORExpression, MakeSeq( LogicalANDOperator, BitwiseORExpression ) .Star );
		}
			
		public bool LogicalORExpression(){ 
			//LogicalORExpression 	::= 	LogicalANDExpression ( LogicalOROperator LogicalANDExpression )*
			return Sequence( LogicalANDExpression, MakeSeq( LogicalOROperator, LogicalANDExpression ).Star );
		}
			
		public bool IfStatement(){ 
			//IfStatement 	::= 	"if" "(" Expression ")" Statement ( "else" Statement )?
			return Sequence( Token("if"), Token("("), Expression, Token(")"), Statement, MakeSeq( Token("else"), Statement ).Optional );
		}

		public bool IterationStatement(){ 
			//IterationStatement 	::= 	( "do" Statement "while" "(" Expression ")" ( ";" )? )
			//	| 	( "while" "(" Expression ")" Statement )
			//	| 	( "for" "(" ( ExpressionNoIn )? ";" ( Expression )? ";" ( Expression )? ")" Statement )
			//	| 	( "for" "(" "var" VariableDeclarationList ";" ( Expression )? ";" ( Expression )? ")" Statement )
			//	| 	( "for" "(" "var" VariableDeclarationNoIn "in" Expression ")" Statement )
			//	| 	( "for" "(" LeftHandSideExpressionForIn "in" Expression ")" Statement )
			Func<bool> semi = Token(";");
			Func<bool> open = Token("(");
			Func<bool> close = Token(")");
			Func<bool> optionalExpression = new Bool( Expression ).Optional;
			return Sequence(Token("do"), Statement, Token("while"), open, Expression, close, SemiColon.Optional )
				|| Sequence(Token("while"), open, Expression, close, Statement)
				|| Sequence(Token("for"), open, new Bool( ExpressionNoIn ).Optional, semi, optionalExpression, semi, optionalExpression, close, Statement )
				|| Sequence(Token("for"), open, Token("var"), VariableDeclarationList, semi, optionalExpression, semi, optionalExpression, close, Statement )
				|| Sequence(Token("for"), open, Token("var"), VariableDeclarationListNoIn, Token("in"), Expression, close, Statement )
                || Sequence(Token("for"), open, LeftHandSideExpressionForIn, Token("in"), Expression, close, Statement );
		}

	}

	public class Bool{
		public Bool(Func<bool> f){ One = f; }
		public bool Star(){ while( One() ); return true; }
		public bool Optional(){ One(); return true; }
		public bool Plus(){ if( !One() ){ return false; } while( One() ); return true; }
		public Func<bool> One{ get; private set; }
	}

}
