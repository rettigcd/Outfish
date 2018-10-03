using System;

namespace Outfish {

	internal enum HeaderFooterType{ 
		Head, 
		Tail, 
		Single // head and tail
	}

	/// <summary>
	/// Marks the beginning and end of a Container Node.
	/// </summary>
	internal class HeaderFooter{
	
		public HeaderFooter( string src, int begin, int end, string name, HeaderFooterType type ){
			this.Source = src;
			this.Begin = begin;
			this.End = end;
			this.Name = name;
			this.NameLowerCased = name.ToLower();
			this.Type = type;
		}

		internal string Name{ get; private set; }
		internal HeaderFooterType Type{ get; private set; }
		internal int Begin{ get; private set; }
		internal int End{ get; private set; }
		internal string Source{ get; private set; }
		
		internal string NameLowerCased{ get; private set; } // to simplify comparing with casing mismatch
		
		/// <summary>
		/// Returns the sub-string that makes up the HeaderFooter
		/// </summary>
		public override string ToString() {
			return this.Source.Substring( this.Begin, this.End-this.Begin );
		}

		public string ToVerboseString(){
			return string.Format("{0}:[{1}..{2}]{3}"
				,this.Type
				,this.Begin
				,this.End
				,this.ToString()
			);
		}

	}

	
}
