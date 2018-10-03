using System;

namespace Outfish {

	/// <summary>
	/// Thrown when searching an HtmlDocument for a required substring, regex, or HtmlNode (using css)
	/// </summary>
	public class ItemNotFoundException : Exception {

		/// <summary>
		/// thrown when a search(css,regex,pattern,etc) does not find a matching element/node/substring
		/// </summary>
		public ItemNotFoundException(string needle, string haystack):base( $"Unable to find [{needle}] in [{haystack}]" ){
			this.Needle = needle;
			this.Haystack = haystack;
		}

		public string Needle { get; private set; }
		public string Haystack { get; private set; }

	}

}
