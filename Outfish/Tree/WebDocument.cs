using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Outfish.Strings;

namespace Outfish {

	/// <summary>
	/// Base class for various Document Object Model classes.
	/// </summary>
	public class WebDocument {
	
		internal WebDocument( string source, HtmlNode documentNode, Uri location ){
			if( source == null ) throw new ArgumentNullException( nameof(source) );
			if( documentNode == null ) throw new ArgumentNullException( nameof( documentNode ) );
			Source = source;
			DocumentNode = documentNode;
			Location = location;
		}
	
		/// <summary> The final request URL that returned the document. </summary>
		/// <remarks>Used for resolving relative Urls in documents.</remarks>
		public Uri Location { get; private set; }

		/// <summary>Root node of the document</summary>
		public HtmlNode DocumentNode { get; private set; }

		/// <summary>
		/// The original string that generated the document.
		/// </summary>
		/// <remarks>
		/// Stored in here because nodes need (1) the string so they can parse, print, etc, (2) the Document
		/// </remarks>
		public string Source{ get; private set; }
	
		/// <summary>
		/// Finds the leafiest node that contains the given html index
		/// </summary>
		public HtmlNode FindNodeAt( int contentIndex ){
		
			HtmlNode cur = this.DocumentNode;
			if( !cur.Contains( contentIndex ) ){ return null; }
			
			while( true ){
				HtmlNode childNodeContainingIndex = cur.ChildNodes.Where( c => c.Contains( contentIndex ) ).FirstOrDefault();
				if( childNodeContainingIndex != null )
					cur = childNodeContainingIndex;
				else
					return cur;
			}
		
		}

		#region FindXXX(css) shortcuts

		/// <summary> Shortcut to .DocumentNode.Find(cssSelector) </summary>
		public IEnumerable<HtmlNode> Find( string cssSelector ) => DocumentNode.Find( cssSelector );

		/// <summary> Throws ItemNotFoundException if node is not found. </summary>
		public HtmlNode FindFirst( string cssSelector ) => DocumentNode.FindFirst( cssSelector );

		#endregion

		#region Match-string shortcuts

		/// <summary> Matches the first occurance. </summary>
		/// <returns>If no match, throws ItemNotFoundException.</returns>
		public Match MatchFirst( Regex regex ) => Source.MatchFirst( regex );

		/// <summary> Matches the first occurance. Throws exception if no match is found or not successful.</summary>
		/// <returns>If no match, throws ItemNotFoundException.</returns>
		public Match MatchFirst( string pattern ) => Source.MatchFirst( pattern );

		/// <summary> Fluent method for finding RegEx Matches </summary>
		public IEnumerable<Match> Matches( Regex regex ) => Source.Matches(regex);

		/// <summary> Fluent method for finding RegEx Matches </summary>
		public IEnumerable<Match> Matches( string regexPattern ) => Source.Matches( regexPattern );

		#endregion


	
	}
}
