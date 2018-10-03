using System;
using System.Net;

namespace Outfish {

	/// <summary>
	/// Throw when a WebException occurrs during the transport process.
	/// </summary>
	public class ScrapeException : Exception {

		/// <summary>
		/// Wraps a WebException attaching the IScrapeRequest that generated the error.
		/// </summary>
		public ScrapeException( WebException inner, ScrapeRequest request, string responseBody ) 
			: base("", inner ) {

			this.Request = request;
			this.ResponseBody = responseBody;

		}

		/// <summary> The request that generated the exception. </summary>
		public ScrapeRequest Request { get; private set; }

		/// <summary> The response body content (if available).  May contain error information. </summary>
		public string ResponseBody { get; private set; }

	}

}
