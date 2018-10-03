using System;
using System.Linq;

namespace Outfish {

	internal class HtmlDocumentParser {

		string _source;
		ContainerNode _curNode; // this updates as we parse the document
		HtmlParseOptions _options;
		int _curIndex;

		public HtmlDocumentParser( string source, HtmlParseOptions options ) {
			if(source == null) throw new ArgumentNullException(nameof(source));
			_source = source;
			_options = options;
		}

		/// <summary>
		/// Throws a ParseException if unable to parse document.
		/// </summary>
		public ContainerNode Parse() {
			try {
				return TryParse();
			} catch(Exception ex) {
				throw new ParseException("Unable to parse HtmlDocument",_source,ex);
			}
		}

		ContainerNode TryParse() {

			// find root node
			_curIndex = _source.IndexOf( '<' );
			if( _curIndex < 0 ) throw new FormatException( "No root node found." );

			// setup root node
			HeaderFooter rootHeader = ParseHeaderFooter();
			if( rootHeader.Type == HeaderFooterType.Tail )
				throw new FormatException( "root node cannot be ending tag." );

			ContainerNode root = new ContainerNode( _source, rootHeader );
			if( rootHeader.Type != HeaderFooterType.Single ) _curNode = root;
			_curIndex = rootHeader.End;

			// 
			while( _curIndex < _source.Length && _curNode != null ) {

				if( _source[_curIndex] != '<' )
					ConsumeText();
				else if( CurrentIndexPointsAt( "<!--" ) )
					ConsumeComment();
				else
					ConsumeTag();

			}

			return root;
		}

		HeaderFooter ParseHeaderFooter() {

			if( _curIndex >= _source.Length ) throw new FormatException( "begin is out of bounds." );
			if( _source[_curIndex] != '<' ) throw new FormatException( "first char must be a '<'" );

			int end = FindEndOfTag();
			string name = GetTagName( end );
			HeaderFooterType type = PickHeaderFooterType( end );

			return new HeaderFooter( _source, _curIndex, end, name, type );

		}

		int FindEndOfTag() {
			// skip first char because:
			//  must be good and
			//  don't want our detector to stop on it
			int end = _curIndex + 1;

			while( end < _source.Length // runs out of string
				&& _source[end] != '<'  // finds new tag
				&& _source[end - 1] != '>'// properly closes
			) {
				++end;
			}

			return end;
		}

		string GetTagName( int endOfTag ) {
			// skip /
			int nameBegin = _curIndex + 1; if( nameBegin < endOfTag && _source[nameBegin] == '/' ) nameBegin++;
			// Skip whitespace
			while( nameBegin < endOfTag && char.IsWhiteSpace( _source[nameBegin] ) ) nameBegin++;

			int nameEnd = nameBegin;
			while( nameEnd < endOfTag && (char.IsLetterOrDigit( _source[nameEnd] ) || _source[nameEnd] == '_') ) { nameEnd++; }
			return _source.Substring( nameBegin, nameEnd - nameBegin );
		}

		HeaderFooterType PickHeaderFooterType( int endOfTagIndex ) {

			if( endOfTagIndex - _curIndex > 1 && _source[_curIndex + 1] == '/' )
				return HeaderFooterType.Tail;

			if( endOfTagIndex - _curIndex >= 2 && _source[endOfTagIndex - 2] == '/' )
				return HeaderFooterType.Single;

			return HeaderFooterType.Head;
		}

		/// <summary>
		/// Consumes text at curIndex, appending it as a TextNode to the curNode
		/// </summary>
		void ConsumeText() {

			// parse text node
			TextNode textNode = new TextNode( _source, _curIndex );

			// add to current node
			if( HasValidText( textNode ) )
				_curNode.AddChild( textNode );

			// move to end
			_curIndex = textNode.End;
		}

		bool HasValidText( TextNode textNode ) => AllowsWhitespaceNodes || !textNode.IsWhiteSpace;

		bool AllowsWhitespaceNodes =>(_options & HtmlParseOptions.KeepEmptyText) != 0;


		/// <summary>
		/// Consumes comment at index, appending it as CommentNode to curNode
		/// </summary>
		void ConsumeComment() {
			CommentNode commentNode = new CommentNode( _source, _curIndex );
			_curNode.AddChild( commentNode );
			_curIndex = commentNode.End;
		}

		bool CurrentIndexPointsAt( string needle ) {

			// new way... checks only at current positoin
			if( _curIndex + needle.Length > _source.Length )
				return false;

			for( int i = 0; i < needle.Length; ++i )
				if( _source[_curIndex + i] != needle[i] )
					return false;

			return true;
		}

		/// <summary>
		/// Consumes the tag head, tail, or single at curIndex
		/// </summary>
		void ConsumeTag() {

			HeaderFooter headerFooter = ParseHeaderFooter();

			switch( headerFooter.Type ) {
				case HeaderFooterType.Single:
				case HeaderFooterType.Head:
					DoStarterTag( headerFooter );
					break;
				case HeaderFooterType.Tail:
					DoTailTag( headerFooter );
					break;
				default:
					throw new FormatException( "screwy tag type" );
			}

			_curIndex = headerFooter.End;

		}

		// returns the new current node
		void DoStarterTag( HeaderFooter tag ) {

			// Head & Single
			// new node
			ContainerNode n = new ContainerNode( _source, tag );

			// create parent-child relationship
			_curNode.AddChild( n );

			// force some node types to be single
			bool isSingle = tag.Type == HeaderFooterType.Single
				|| _emptyTags.Contains( tag.Name );
			// if forced-empty node has a tailtag, it will be ignored

			// !! Not doing anything special about illegally embeded nodes such as 
			//		<p>first paragraph <p> new paragraph, close previous <p> another paragraph

			if( !isSingle )
				_curNode = n;

		}

		static string[] _emptyTags = new string[]{
			 "br"
			,"hr"
			,"img"
			,"input"
		};

		// returns the new current node
		void DoTailTag( HeaderFooter tag ) {

			// find self or parent with matching header tag
			ContainerNode searchNode = _curNode;
			while( searchNode != null && searchNode.Header.NameLowerCased != tag.NameLowerCased )
				searchNode = searchNode.ParentNode;

			// if no matching head found
			if( searchNode == null )
				return;

			// add to current node
			searchNode.Footer = tag;

			if( _noInnerTagNodes.Contains( tag.NameLowerCased ) ) {

				// convert whatever children happened to be parsed, into a single text node
				// used primarily to fix <script> nodes that have < and > in them
				searchNode.ChildNodes.Clear();

				// We used to have option to parse JavaScriptDocument but now we will do that manually because it was too confusing.
				searchNode.AddChild( new TextNode( searchNode.Source, searchNode.Header.End, searchNode.Footer.Begin ) );
			}

			// return current node to parent
			_curNode = searchNode.ParentNode;

		}

		static string[] _noInnerTagNodes = new string[] { "script" };

	}

}
