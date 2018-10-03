using System;
using System.Collections.Generic;
using System.Linq;
using Outfish.Strings;

namespace Outfish {

	/// <summary>
	/// Base abstract class for Document Nodes.
	/// </summary>
	abstract public class HtmlNode{

		#region constructor

		internal HtmlNode(string source,int begin, string name ){
			if( source == null ) throw new ArgumentNullException( nameof(source) ); 
			if( source.Length < begin )
				throw new ArgumentException("Begin is out of bounds."); 
		
			// I think source.Length == begin is ok if we are creating a 0-length text node 

			this.Source = source;
			this.Begin = begin;
			this.Name = name;
		}

		#endregion

		/// <summary>
		/// Gets the name of the node.  Null for nameless nodes like text nodes.
		/// </summary>
		public string Name{ get; internal set; }

		/// <summary> Shortcut for .InnerHtml.DecodeHtml(). </summary>
		public string InnerText => InnerHtml.DecodeHtml();

		/// <summary> Gets the raw source that constructed this node excluding the header and footer. </summary>
		virtual public string InnerHtml => this.OuterHtml; // this is correct for most node types

		/// <summary> Gets the raw souce that constructed this node including any header or footer. </summary>
		public string OuterHtml => this.Source.Substring( Begin, End-Begin );

		#region public family relationships

		/// <summary>Gets the parent node.  Null for the Document/Root node.</summary>
		public ContainerNode ParentNode{ get; internal set; }

		/// <summary>Gets the child nodes contained inside this container node.</summary>
		public List<HtmlNode> ChildNodes => _childNodes;

		/// <summary>Gets all descendant nodes in document order.</summary>
		public IEnumerable<HtmlNode> Descendants() => DescendantSearcherHelper.Descendants( this );

		/// <summary>Gets self and all descendant nodes in document order.</summary>
		public IEnumerable<HtmlNode> DescendantsAndSelf(){
			Stack<HtmlNode> nodes = new Stack<HtmlNode>();
			nodes.Push( this );
			while( nodes.Count > 0 ){
				HtmlNode node = nodes.Pop();
				// yield this node
				yield return node;
				// add children to stack (in reverse 
				for(int i = node.ChildNodes.Count; i > 0;){
					nodes.Push( node.ChildNodes[--i] );
				}
			}
		}

		/// <summary>Starting at the root, steps down thru the tree to the current node's parent.</summary>
		public IEnumerable<HtmlNode> Ancestors() => Parents().Reverse();

		/// <summary>Starting at the root, steps down thru the tree to the current node.</summary>
		public IEnumerable<HtmlNode> AncestorsAndSelf() => ParentsAndSelf().Reverse();

		/// <summary>Starting at the curent node, steps up to the root node.</summary>
		public IEnumerable<HtmlNode> ParentsAndSelf(){
			for(var cur=this; cur!=null;cur=cur.ParentNode){ yield return cur; }
		}

		/// <summary>Steps up the tree from the closest parent to the root node</summary>
		public IEnumerable<HtmlNode> Parents(){
			// gather parents in a list
			List<HtmlNode> parents = new List<HtmlNode>();
			HtmlNode cur = this.ParentNode;
			while( cur != null ){
				parents.Add( cur );
				cur = cur.ParentNode;
			}
			return parents;
		}

		/// <summary>Gets the nodes previous sibling, or null if none exists.</summary>
		public HtmlNode PreviousSibling{
			get{
				int index = this.SiblingIndex;
				return ( this.ParentNode != null && index > 0 )
					? this.ParentNode.ChildNodes[index-1]
					: null;
			}
		}

		/// <summary>Gets the node's next sibling, or null if none exists.</summary>
		public HtmlNode NextSibling{
			get{
				int index = this.SiblingIndex;
				var siblings = this.ParentNode.ChildNodes;
				return ( this.ParentNode != null && index < siblings.Count-1 )
					? siblings[ index+1 ]
					: null;
			}
		}

		/// <summary>Gets node's first child or null if it has no children.</summary>
		public HtmlNode FirstChild => ChildNodes.Count > 0 ? this.ChildNodes[0] : null;

		/// <summary>0-based sibling index. / Number of previous siblings.</summary>
		public int SiblingIndex => ( ParentNode != null ) ? ParentNode.ChildNodes.IndexOf( this ) : 0; // no parent, must be root node

		/// <summary>Total # of siblings (including this node).</summary>
		public int SiblingCount => this.ParentNode != null ? this.ParentNode.ChildNodes.Count : 1;

		/// <summary>0-based depth of node in the tree. Root node is 0.</summary>
		public int Depth{
			get{
				int i=0;
				for( var cur = this; cur!=null; cur=cur.ParentNode){ ++i; }
				return i;
			}
		}

		/// <summary>true if node has no children. (includes text nodes)</summary>
		public bool IsEmpty => this.ChildNodes == null || this.ChildNodes.Count == 0; 

		/// <summary>true if node has at least 1 child.</summary>
		public bool IsParent => ChildNodes != null && this.ChildNodes.Count > 0;

		/// <summary>node has no previous siblings</summary>
		public bool IsFirst => this.SiblingIndex == 0;
		
		/// <summary>node has no next siblings</summary>
		public bool IsLast => this.SiblingIndex == this.SiblingCount-1;

		#endregion
		
		#region public CSS selectors for my xml
		
		/// <summary> Finds Descendant nodes that match the CSS selector. </summary>
		/// <param name="cssSelector"> 1 or more css paths, sepearted by commas(,) </param>
		public IEnumerable<HtmlNode> Find( string cssSelector ) => new CssExpression( cssSelector ).Find( this );

		/// <summary>
		/// Finds the first node matching the selector.  Throws ItemNotFoundException with including CSS-Selctor and OuterHtml if no node is found.
		/// </summary>
		///<remarks>Find first is used so many times that it would be super handy 
		/// to make it available and to report the CssSelecotr and OuterHtml when it fails.
		/// </remarks>
		public HtmlNode FindFirst( string cssSelector ){
			HtmlNode node = this.Find( cssSelector ).FirstOrDefault();
			if( node != null ) return node;

			throw new ItemNotFoundException( cssSelector, this.OuterHtml );
		}

		/// <summary>
		/// Search immediate child nodes for a match.
		/// </summary>
		/// <remarks>
		/// Assumes cssSelector is is single-step, single path
		///		the single-step assumption is good because children are a single step
		///		the single-path assumption is bad because we should be able to separate with a comma
		/// </remarks>
		public IEnumerable<HtmlNode> Children( string cssSelector ) =>
			this.ChildNodes
				.Where( new CssExpression(cssSelector).IsMatch );
		
		/// <summary>
		/// Steps up the tree from the closest parent to the root node returning each node that matches
		/// </summary>
		public IEnumerable<HtmlNode> Parents(string cssSelector) =>
			this.Parents().Where( new CssExpression(cssSelector).IsMatch );

		/// <summary>
		/// Searches up ancestor tree, starting with self, for first node that matches
		/// </summary>
		public HtmlNode Closest( string cssSelector ){
			var cssExpression = new CssExpression(cssSelector);
			HtmlNode cur = this;
			while( cur != null ){
				if( cssExpression.IsMatch( cur ) ){ return cur; }
				cur = cur.ParentNode;
			}
			return null;
		}

		/// <summary>
		/// Returns a verbose selector that will select this node.
		/// </summary>
		public INodeMatcher GenerateCssSelector( bool includeNthChild ){
		
			AndSelector selector = new AndSelector();
			
			// name
			if( this.Name != null )
				selector.Selectors.Add( new NameSelector( this.Name ) );
			
			// attributes
			foreach(string s in this.AttributeNames){
				string[] parts = this[s].Split(' ');
				if( parts.Length == 1 ){ 
					// 'class' is the only attribute I know of that allows mix and match, everything else should be exact match
					var op = (s!="class") 
							? AttributeSelector.EqualToOp 		// exact match
							: AttributeSelector.ContainsWordOp; // mix and match - also allows string to have .class for single classed elements
					selector.Selectors.Add( new AttributeSelector( s, op, parts[0] ) );
				} 
				else
					foreach( string part in parts )
						selector.Selectors.Add( new AttributeSelector(s, AttributeSelector.ContainsWordOp,part));
			}
			
			if( this.SiblingCount > 1 && includeNthChild )
				selector.Selectors.Add( new SiblingSelector(0,this.SiblingIndex+1) );

			
			// : selectors
			var candidates = new INodeMatcher[]{
//				 new ColonSelector(":first-child")
//				,new ColonSelector(":last-child")
//				,new ColonSelector(":empty")
//				,new ColonSelector(":parent")
			};
			foreach(var c in candidates){
				if( c.IsMatch( this ) ){ 
					selector.Selectors.Add( c );
				}
			}

			return selector;

		}

		#endregion

		#region Attribute methods

		/// <summary>
		/// Gets the names/keys of all attributes in the node.
		/// </summary>
		virtual public IEnumerable<string> AttributeNames{ 
			get{ return Enumerable.Empty<string>(); }
		}

		/// <summary>
		/// Gets an attribute of the node using a case-insensitive key.
		/// </summary>
		/// <returns>null if attribute not found.</returns>
		public string this[string attributeName]{
			get{
				if( attributeName == null ) throw new ArgumentNullException("attributeName");
				return this.GetAttr( attributeName.ToLower() );
			}
		}

		/// <summary>
		/// allows internal things like selectors that have already done a .ToLower 
		/// to bypass the .ToLower in this[name]. For case insensitive, use node[attrName]
		/// </summary>
		/// <param name="attributeName">lower case attribute name</param>
		/// <returns>null if attribute not found</returns>
		virtual internal string GetAttr( string attributeName ){ return null; }

		#endregion

		#region public string properties and methods
		
		/// <summary>The string the WebDocument was generated from.</summary>
		public string Source{ get; private set; }
		
		/// <summary>The index in the Source string where this node begins.</summary>
		/// <remarks>Sorting by Begin will put the elements in Document-order but should rarely be needed as enumerators return them in doc-order.</remarks>
		public int Begin{ get; private set; }

		/// <summary>The index in the Source string where this node ends. Points to start of next node.</summary>
		abstract public int End{ get; }

		/// <summary>Determines if node lies on given index into the Source string.</summary>
		public bool Contains(int index){ return this.Begin<=index && index < this.End; }

		/// <summary>Determines if Html source contains given text.</summary>
		public bool Contains(string needle){ return this.Source.IndexOf(needle,this.Begin) < this.End-needle.Length; }

		#endregion

		#region internal methods
		
		virtual internal void AddFormattedText(System.Text.StringBuilder builder, int tabCount){
		
			// if short - do on 1 line
			if( tabCount > 5 && this.End - this.Begin < 80 ){
				builder.Append('\t',tabCount);
				builder.AppendLine( this.OuterHtml );
				return;
			}

			// this is the implementation for Text and Comment nodes
			foreach( var line in LineManager.SplitToLines( this.OuterHtml.Trim() ) ){
				builder.Append('\t', tabCount);
				builder.AppendLine( line.Trim() );
			}
		}
		
		#endregion

		List<HtmlNode> _childNodes = new List<HtmlNode>();

		/// <summary>
		/// Creates a string version of the node indenting child nodes.
		/// </summary>
		public string ToFormattedString(){
			var builder = new System.Text.StringBuilder();
			this.AddFormattedText(builder, 0);
			return builder.ToString();
		}

	}

}
