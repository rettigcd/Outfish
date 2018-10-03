using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Outfish {

	/// <summary> Simplifies pulling items from the web. </summary>
	public interface IWebScraper : IFileScraper, IDocScraper {}

	}
