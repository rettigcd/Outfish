using System.IO;
using System.Threading.Tasks;

namespace Outfish {

	/// <summary>
	/// Pulls items from the web and writes them to streams and saves them to files.
	/// </summary>
	public interface IFileScraper {

		/// <summary>
		/// Saves the document retrieved from the web response to a flie, using the default timeout
		/// </summary>
		/// <param name="request">Contains specific URI, post-body, etc specifics of the web request.</param>
		/// <param name="fileName">Filename to save file as.</param>
		/// <returns>HTML formatted Content type (image/jpeg, application/xml, etc)</returns>
		string WriteResponseToFile( ScrapeRequest request, string fileName );
		Task<string> WriteResponseToFileAsync( ScrapeRequest request, string fileName );

		/// <summary>Writes the response to a Stream</summary>
		/// <returns>Response content type of successful attempts, or response body of unsuccessful attempts..</returns>
		string WriteResponseToStream( ScrapeRequest request, Stream oStream );

		/// <summary>Writes the response to a Stream</summary>
		/// <returns>Response content type of successful attempts, or response body of unsuccessful attempts..</returns>
		Task<string> WriteResponseToStreamAsync( ScrapeRequest request, Stream oStream );

	}

	}
