using System;

namespace Outfish {
	
	/// <summary>
	/// Implement this interface to simplify loading in ScreenScraper
	/// </summary>
	public interface IWebPage {
		
		void Load( string htmlContent );
		
	}
	
}
