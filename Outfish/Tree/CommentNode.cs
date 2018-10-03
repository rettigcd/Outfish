using System;

namespace Outfish {

	/// <summary>
	/// Contains an HTML comment.
	/// </summary>
	public class CommentNode : HtmlNode {
	
		/// <summary>
		/// Constructs a Comment node from the source starting at begin and going until --> is found.
		/// </summary>
		public CommentNode(string source, int begin)
			:base(source,begin,null)
		{
			int index = source.IndexOf("-->",begin);
			this._end = index == -1
				? source.Length
				: index + 3;
		}

		/// <summary>Returns the text between the comment tags.</summary>
		public override string InnerHtml {
			get {// strip off <!-- -->
				return this.Source.Substring( this.Begin+4, this.End - this.Begin - 7 );
			}
		}

		/// <summary>Gets the index of the 1st position beyond the tag.</summary>
		public override int End { get{ return this._end; } }

		#region internal / private
		
		int _end;
		
		#endregion
	}
}
