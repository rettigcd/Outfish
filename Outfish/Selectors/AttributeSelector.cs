using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace Outfish {

	/// <summary>
	/// Selects HtmlNodes by Examining its attributes
	/// </summary>
	internal class AttributeSelector : INodeMatcher{

		// !!! this is the only public node selector
		// fix tests so we can make this internal

		#region public Attibute Operators

		public const string ExistsOp         = "";
		public const string ContainsOp       = "*=";
		public const string ContainsPrefixOp = "|=";
		public const string StartWithOp      = "^=";
		public const string ContainsWordOp   = "~=";
		public const string EndsWithOp       = "$=";
		public const string EqualToOp        = "=";
		public const string NotEqualToOp     = "!=";

		#endregion

		#region constructor / factory method

		public AttributeSelector( string name, string op, string value ){
			this.Name = name;
			this.Operator = op;
			if( value != null ) { this.Value = value; }
		}

		static public AttributeSelector ContainsClass( string className ){
			return new AttributeSelector( 
				"class", 
				AttributeSelector.ContainsWordOp,
				className
			);
		}

		static public AttributeSelector IdIs( string idName ){
			return new AttributeSelector( 
				"id", 
				AttributeSelector.EqualToOp,
				idName
			);
		}

		#endregion

		public string Name{ 
			get{ return _lowerCaseName; }
			private set{ this._lowerCaseName = value.ToLower(); } // to make comparing easier
		}

		public string Operator{ 
			get{ return this._operator; }
			private set{ 
				this._operator = value;
				this._pred = FindPred();
			} 
		}
		
		public string Value{ 
			get{ return _lowerCaseValue; }
			private set{ this._lowerCaseValue = value.ToLower(); } // to make comparing easier
		}
		
		public bool IsMatch(HtmlNode n){
			string v = n.GetAttr(this._lowerCaseName);
			return ( v != null ) && this._pred( v.ToLower(), this._lowerCaseValue );
		}

		public bool IsMatch( XmlNode node ) {
			XmlAttribute v = node.Attributes[_lowerCaseName]; // !!! should we use lower case???
			return (v != null) && this._pred( v.Value.ToLower(), this._lowerCaseValue );
		}

		public override string ToString() {
			if( this._lowerCaseName == "class" && this._operator == ContainsWordOp ){ return "." + this._lowerCaseValue; }
			if( this._lowerCaseName == "id" && this._operator == EqualToOp ){ return "#" + this._lowerCaseValue; }
			string inner = this._lowerCaseName;
			if( this._operator != ExistsOp ){ inner += this._operator + '\'' + this._lowerCaseValue + '\''; }
			return "[" + inner + "]";
		}

		#region private
		
		string _lowerCaseName;
		string _operator;
		string _lowerCaseValue;

		Func<string,string,bool> _pred;

		Func<string,string,bool> FindPred(){
			switch( this._operator ){
				case ContainsOp:       return (s,v) => s.Contains(v);
				case ContainsPrefixOp: return (s,v) => Regex.IsMatch(s,"\\b"+v);
				case StartWithOp:      return (s,v) => s.StartsWith(v);
				case ContainsWordOp:   return (s,v) => Regex.IsMatch(s,"\\b"+v+"\\b");
				case EndsWithOp:       return (s,v) => s.EndsWith(v);
				case EqualToOp:        return (s,v) => s == v;
				case NotEqualToOp:     return (s,v) => s != v;
				case ExistsOp:         return (s,v) => true;
			}
			throw new InvalidOperationException();
		}

		#endregion

	}
}
