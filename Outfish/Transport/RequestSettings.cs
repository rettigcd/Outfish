using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Outfish {

	public class RequestSettings {

		/// <summary>The specific timeout in milliseconds to use for this request.  Overrides timeout in scraper.</summary>
		public int Timeout { get; set; }

		public bool KeepAlive { get; set; }

		public Version ProtocolVersion { get; set; }

		/// <summary> Get or Sets the Referrer that is required to make this request. </summary>
		public string Referrer { get; set; }

		/// <summary>Set to something like System.Net.CredentialCache.DefaultCredentials</summary>
		public ICredentials Credentials { get; set; }

		public string Accept { get; set; }

		public string UserAgent { get; set; }

		public Dictionary<string,string> Headers {
			get { return _headers; }
			set { if(value==null) throw new ArgumentNullException(); _headers = value; }
		}
		Dictionary<string, string> _headers = new Dictionary<string, string>();

		internal RequestSettings Clone() {
			var clone = new RequestSettings();
			clone.Timeout = this.Timeout;
			clone.KeepAlive = this.KeepAlive;
			clone.ProtocolVersion = this.ProtocolVersion;
			clone.Credentials = this.Credentials;
			clone.Accept = this.Accept;
			clone.UserAgent = this.UserAgent;
			clone.Headers = this.Headers.ToDictionary(pair=>pair.Key,pair=>pair.Value); 
			return clone;
		}

	}

}
