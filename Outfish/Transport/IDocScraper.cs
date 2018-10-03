using System.Net;
using System.Threading.Tasks;

namespace Outfish {

	/// <summary>
	/// Interface for engine that can scrape web pages.
	/// Wraps WebExceptions in detailed ScrapeException.
	/// </summary>
	public interface IDocScraper {

		/// <summary>
		/// Accesses the scraper's Cookies
		/// </summary>
		/// <remarks>Cookies may be cleared by replacing the CookieJar with a new one.</remarks>
		CookieContainer CookieJar { get; set; }

		/// <summary> 
		/// Gets a generic Xml-like Document
		/// Wraps ScrapeException around any WebException
		/// Throws ParseException if document can't be parsed.
		/// </summary>
		HtmlDocument GetDocument( ScrapeRequest request );

		/// <summary> 
		/// Gets a generic Xml-like Document 
		/// Wraps ScrapeException around any WebException
		/// Throws ParseException if document can't be parsed.
		/// </summary>
		Task<HtmlDocument> GetDocumentAsync( ScrapeRequest request );

		/// <summary> 
		/// Get the response body of a web request. 
		/// Wraps ScrapeException around any WebException
		/// </summary>
		/// <returns>the body as a string, never null</returns>
		string GetResponseBody( ScrapeRequest request );

		/// <summary> 
		/// Get the response body of a web request.
		/// Wraps ScrapeException around any WebException
		/// </summary>
		/// <returns>the body as a string, never null</returns>
		Task<string> GetResponseBodyAsync( ScrapeRequest request );

	}

}
