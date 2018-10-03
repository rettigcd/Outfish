using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Outfish {


	/// <summary>
	/// Node that can contain other nodes and attributes.
	/// </summary>
	public class ContainerNode : HtmlNode {
	
		#region constructor

		/// <summary>
		/// Constructs a Container that uses the attributes embedded in the tag header.
		/// </summary>
		internal ContainerNode( string source, HeaderFooter header )
			:base( source, header.Begin, header.Name )
		{
			this.Header = header;
		}

		/// <summary>
		/// Constructs a Container that uses attributes provided by the input dictionary.
		/// </summary>
		internal ContainerNode( string source, HeaderFooter header, Dictionary<string,string> attr )
			:base( source, header.Begin, header.Name )
		{
			this.Header = header;
			this._attributes = attr;
		}

		#endregion

		/// <summary>Gets the index of the 1st position beyond the tag.</summary>
		override public int End{
			get{
				if( this.Footer != null )
					return this.Footer.End;

				if( this.ChildNodes.Count > 0 )
					return this.ChildNodes[ this.ChildNodes.Count-1 ].End;

				return this.Header.End;
			}
		}

		#region internal

		internal void AddChild( HtmlNode node ){
			this.ChildNodes.Add( node );
			node.ParentNode = this;
		}

		internal HeaderFooter Header{get; private set; }
		
		internal HeaderFooter Footer{ set; get; }
	
		internal override void AddFormattedText(System.Text.StringBuilder builder, int tabCount) {
			// if short - do on 1 line
			if( tabCount > 5 && this.End - this.Begin < 80 ){
				builder.Append('\t',tabCount);
				builder.AppendLine( this.OuterHtml );
				return;
			}

			// head
			builder.Append('\t',tabCount);
			builder.AppendLine( this.Header.ToString() );
			
			// children
			foreach(var child in this.ChildNodes){
				child.AddFormattedText(builder, tabCount+1);
			}

			// tail
			if( this.Footer != null ){
				builder.Append('\t',tabCount);
				builder.AppendLine( this.Footer.ToString() );
			}

		}
	
		#endregion
		
		/// <summary>Gets the HTML string that appears between the HTML tags</summary>
		public override string InnerHtml{
			get{
				if( this.ChildNodes.Count == 0 ){ return string.Empty; }
				int beg = this.ChildNodes.First().Begin;
				int end = this.ChildNodes.Last().End;
				return this.Source.Substring( beg, end-beg );
			}
		}

		#region public/internal attributes

		/// <summary>
		/// allows internal things like selectors that have already done a .ToLower 
		/// to bypass the .ToLower in this[name]. For case insensitive, use node[attrName]
		/// </summary>
		/// <param name="attributeName">lower case attribute name</param>
		/// <param name="val">new value for the attribute</param>
		/// <returns>null if attribute not found</returns>
		internal void SetAttr( string attributeName, string val ){
			if( attributeName == null ) throw new ArgumentNullException(nameof(attributeName));
			if( val != null )
				this.Attributes[attributeName] = val;
			else if( this.Attributes.ContainsKey( attributeName ) )
				this.Attributes.Remove( attributeName );
		}

		/// <summary>
		/// allows internal things like selectors that have already done a .ToLower 
		/// to bypass the .ToLower in this[name]. For case insensitive, use node[attrName]
		/// </summary>
		/// <param name="attributeName">lower case attribute name</param>
		/// <returns>null if attribute not found</returns>
		override internal string GetAttr( string attributeName ){
			if( attributeName == null ){ throw new ArgumentNullException("attributeName"); }
			return this.Attributes.ContainsKey(attributeName)
				? this.Attributes[attributeName]
				: null;
		}

		/// <summary>
		/// Gets the names of all attributes in the node.
		/// </summary>
		override public IEnumerable<string> AttributeNames => this.Attributes.Keys;

		Dictionary<string, string> _attributes; // lazy init if needed

		Dictionary<string, string> Attributes => _attributes ?? (_attributes = ParseAttributesFromHeader() );

		Dictionary<string,string> ParseAttributesFromHeader(){
		
			int begin = this.Header.Begin;
			int end = this.Header.End;
		
			// get attributes
			var attr= new Dictionary<string, string>();
			string sub = this.Source.Substring(begin, end-begin);
			foreach( Match m in Regex.Matches(sub,@"([_:A-Za-z][_\-:\.A-Za-z0-9]*)\s*(=\s*(\w+|'[^']*'|""[^""]*""))?") ){
				string key = m.Groups[1].Value.ToLower(); // use lower case (internally) to access attribute name
				if( attr.ContainsKey( key ) ){ continue; } // don't crash on duplicate attribute names

				var g1 = m.Groups[1];
				var g2 = m.Groups[2];
				var g3 = m.Groups[3];

				string v = key; // default value to name
				// if there was an equals sign, then use stuff on right as the value
				if( m.Groups[2].Success ) {
					v = m.Groups[3].Value;
					if( v.Length > 0 && (v[0]=='\'' || v[0]=='"'))
						v = v.Substring(1,v.Length-2);
				}

				attr.Add( key,v );
			}
			return attr;
			
		}

		#endregion

	}



	
}
