using System;
using Outfish.Strings;

// Outfish.Common namespace contains short-hand access to common html attributes 
namespace Outfish.HtmlNodeShortcuts {

	/// <summary>
	/// Exposes common html attributes as properties on the node.
	/// </summary>
	static public class HtmlExtensions{

		/// <summary>Returns raw source inside 'class' attribute.</summary>
		static public string Class(this HtmlNode node){ return node["class"]; }

		/// <summary>Returns space delimited words inside 'class' attribute.</summary>
		static public string[] Classes(this HtmlNode node){ return (node["class"] ?? string.Empty).Split(' '); }

		/// <summary>Returns raw source inside 'id' attribute.</summary>
		static public string Id(this HtmlNode node){ return node["id"]; }

		/// <summary>Returns raw source inside 'name' attribute.</summary>
		static public string Name(this HtmlNode node){ return node["name"]; }

		/// <summary>Returns raw source inside 'type' attribute.</summary>
		static public string Type(this HtmlNode node){ return node["type"]; }
	
		/// <summary>Returns raw source inside 'value' attribute.</summary>
		static public string Value(this HtmlNode node){ return node["value"]; }
		
		/// <summary>Returns raw source inside 'src' attribute.</summary>
		static public string Src(this HtmlNode node){ return node["src"]; }

		/// <summary>Determines if the elementes 'checked' attribute is set to 'checked'</summary>
		static public bool Checked(this HtmlNode node){ 
			string s = node["checked"];
			if( string.IsNullOrEmpty( s ) ) return false;
			if( s.ToLower() == "checked" ) return true;
			throw new Exception("Unexpected checked value '"+s+"'");
		}

		/// <summary>Determines if the elemente's (ie. options) 'selected' attribute is set to 'selected'</summary>
		static public bool Selected(this HtmlNode node){
			string s = node["selected"];
			if( string.IsNullOrEmpty( s ) ){ return false; }
			if( s.ToLower() == "selected" ){ return true; }
			throw new Exception("Unexpected selected value '"+s+"'");
		}

		/// <summary>Gets the url of the background image.</summary>
		static public string BackgroundImage(this HtmlNode node){
			string style = node["style"];
			if( style == null ){ return null; }
			// assumes if there is a style, has background image
			return style.Clip( Clip.After("url("), Clip.At(");") );
		}

	}
}
