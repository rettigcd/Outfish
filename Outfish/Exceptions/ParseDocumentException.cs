using System;

namespace Outfish {

	/// <summary> Thrown when the Document can't be parsed into a node-tree. </summary>
	public class ParseException : Exception {

		public ParseException( string msg, string source )
			: base( msg ) {
			this.Source = source;
		}

		public ParseException( string msg, string source, Exception inner )
			:base(msg,inner)
		{
			this.Source = source;
		}

		/// <summary> The source string that as not parsable. </summary>
		public string Source { get; private set; }
	}
}
