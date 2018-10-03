using System;
using System.Collections.Generic;
using System.Linq;

namespace Outfish {

	/// <summary>
	/// represents and HtmlDocument
	/// </summary>
	public class HtmlDocument : WebDocument {

		/// <summary>
		/// Constructs an HtmlDocument from a source string.
		/// Throws ArgumentNullException if htmlSource is null.
		/// Throws a ParseExcpetion if it can't parse the string.
		/// </summary>
		public HtmlDocument( string htmlSource, Uri location = null ):this( htmlSource, HtmlParseOptions.Clean, location ){}

		/// <summary>
		/// Constructs an HtmlDocument from a source string plus options.
		/// Throws ArgumentNullException if htmlSource is null.
		/// Throws a ParseExcpetion if it can't parse the string.
		/// </summary>
		public HtmlDocument( string htmlSource, HtmlParseOptions options, Uri location = null )
			:base( htmlSource, ParseSource(htmlSource,options), location ) 
		{}

		static HtmlNode ParseSource( string htmlSource, HtmlParseOptions options ) {
			try {
				return new HtmlDocumentParser( htmlSource, options ).Parse();
			}
			catch(ParseException) {
				return new TextNode( htmlSource,0,htmlSource.Length );
			}
		}

		/// <summary>
		/// Finds the first node matching the selector then constructs an HtmlForm from it.
		/// Throws ItemNotFoundException including CSS-Selctor and OuterHtml if no node is found.
		/// Throws ArgumentException if found node is not a form node.
		/// </summary>
		/// <param name="cssSelector">defaults to "form"</param>
		public HtmlForm FindForm( string cssSelector="form" ) {
			if( _foundForms == null )
				_foundForms = new Dictionary<string, HtmlForm>();
			if( !_foundForms.ContainsKey( cssSelector ) )
				_foundForms.Add( cssSelector, DoFormFind( cssSelector ) );
			return _foundForms[cssSelector];
		}

		HtmlForm DoFormFind( string cssSelector ) {
			HtmlNode node = this.Find( cssSelector ).FirstOrDefault();
			if( node == null )
				throw new ItemNotFoundException( cssSelector, this.Source );
			if( string.Compare( node.Name, "form", true ) != 0 )
				throw new ArgumentException( $"css selector [{cssSelector}] did not match a <form> node." );
			return new HtmlForm( node, this.Location );
		}
		Dictionary<string,HtmlForm> _foundForms;



	}

}