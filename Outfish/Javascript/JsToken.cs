using System;

namespace Outfish.JavaScript {

	/// <summary>Stores text, location, length, and type of a JavaScript token</summary>
	/// <remarks>public for unit testing</remarks>
	public class JsToken {

		/// <summary>
		/// Constructs a JavaScript token from the host string
		/// </summary>
		/// <param name="host"></param>
		/// <param name="begin"></param>
		/// <param name="end"></param>
		/// <param name="type"></param>
		public JsToken( string host, int begin, int end, JsTokenType type ){
			this.Text = host.Substring(begin, end-begin);
			this.Begin = begin;
			this.Type = type;
		}
		
		/// <summary>Text starting at .Begin and ending before .End.</summary>
		public string Text { get; set; }
		
		/// <summary>The location of the text/token in the host string.</summary>
		public int Begin { get; set; }
		
		/// <summary>Index just after the last character. (beginning of next item)</summary>
		public int End {get { return this.Begin + this.Text.Length; }}
		
		/// <summary>Groups tokens in to different Category to facilitate evaluation.</summary>
		public JsTokenType Type { get; set; }
	}
	
}
