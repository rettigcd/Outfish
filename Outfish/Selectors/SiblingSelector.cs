using System;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Finds children with matching index or index / divisor == some remainder.  Handles :odd and :even.
	/// </summary>
	internal class SiblingSelector : INodeMatcher {

		#region constructor
	
		static readonly public SiblingSelector Even = new SiblingSelector( 2, 0 );
		
		static readonly public SiblingSelector Odd = new SiblingSelector( 2, 1 );
	
		/// <param name="divisor">set to 0 or 1 to disable</param>
		/// <param name="remainder">base-1 index. 1 returns the first item</param>
		public SiblingSelector(int divisor,int remainder){
			this._divisor = divisor;
			this._remainder = remainder;
		}
	
		#endregion
	
		public bool IsMatch(HtmlNode node){
			int base1Index = node.SiblingIndex+1;
			if( this._divisor > 1 ){ base1Index %= this._divisor; }
			return base1Index == this._remainder;
		}

		public bool IsMatch( XmlNode node ) {
			int base1Index = GetPreviousSiblingCount( node ) + 1;
			if( this._divisor > 1 ) { base1Index %= this._divisor; }
			return base1Index == this._remainder;
		}

		public override string ToString() {
			switch( this._divisor ){
				case 0:
				case 1:		return ":nth-child(" + this._remainder + ")";
				case 2:		return this._remainder == 0 ? ":even" : ":odd";
				default:	return string.Format( this._remainder > 0 
						? ":nth-child({0}n+{1})" 
						: ":nth-child({0}n)"
					,this._divisor,this._remainder
					);
			}
		}

		static int GetPreviousSiblingCount( XmlNode node ) {
			if( node.ParentNode == null || node.ParentNode.ChildNodes == null ) return 0;
			var siblings = node.ParentNode.ChildNodes;
			for(int i=0;i<siblings.Count;++i)
				if( siblings[i] == node ) return i;
			throw new Exception("Unable to find current node in parents children.");
		}

		#region private fields

		int _divisor;
		int _remainder;
		
		#endregion
		
	}
}
