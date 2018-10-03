using System;
using System.Text.RegularExpressions;

namespace Outfish {

	/// <summary>
	/// A node that contains text with no header or footer.
	/// </summary>
	public class TextNode : HtmlNode {
	
		#region constructor

		internal TextNode( string source, int begin, int end )
			:base( source, begin, null )
		{
			this._end = end;
		}
		
		/// <summary>
		/// Constructs a text node, scanning for '&lt;' for an end
		/// </summary>
		/// <param name="source"></param>
		/// <param name="begin"></param>
		internal TextNode( string source, int begin ) 
			:base( source, begin, null )
		{
			if( this.Source[begin] == '<' ){ throw new ArgumentException("first char cannot be a '<'"); }
	
			int end = begin;
			
			while( end < this.Source.Length	// runs out of string
				&& this.Source[end] != '<'  // finds new tag
			){
				++end;
			}
			
			this._end = end;
			
		}

		/// <summary>
		/// Determines if the Next node contains nothing but white space.
		/// </summary>
		public bool IsWhiteSpace{ 
			get{ return Regex.IsMatch( this.OuterHtml, @"^\s*$" );	}
		}

		#endregion

		/// <summary>
		/// Gets the begining index of the next element.
		/// </summary>
		override public int End{ get{ return this._end; } }
	
		#region private fields
		
		int _end;
		
		#endregion
	}
}
