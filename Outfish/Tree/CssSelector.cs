using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Outfish.Xml {



	/// <summary>
	/// non-heirarchy (single-step) HtmlNode Predicate
	/// </summary>
	/// <remarks>a 'step' is a single element with matching attributes or pseudo classes</remarks>
	public class CssSelectorStep : IMatchNodes {

		#region constructor

		public CssSelectorStep( string step ) {
		
			foreach(Match m in Regex.Matches(step,@"(^|#|\.|\[|:)[^#.\[:]+") ){
				string sub = m.Value;
				switch( sub[0] ){
					case '#':	_preds.Add( new AttributeMatcher("id","eq",sub) ); break;
					case '.':	_preds.Add( new AttributeMatcher("class","~=",sub) ); break;
					case '[':	_preds.Add( new AttributeMatcher(sub) ); break;
					case ':':	_preds.Add( new ColonNodeMatcher( sub.Substring(1) ) ); break;
					default:	this.NodeName = sub; break;
				}
			}
		
		}

		#endregion

		public string NodeName{ get; private set; }
		
		public bool Match( Outfish.Xml.HtmlNode node ){
			if(this.NodeName != null && NodeName != this.NodeName ){ return false; }
			foreach(IMatchNodes matcher in this._preds){
				if( !matcher.Match(node) ){ return false; }
			}
			return true;
		}

		List<IMatchNodes> _preds = new List<IMatchNodes>();


	}








}
