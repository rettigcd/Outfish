using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using Outfish;

namespace Outfish.Usps {

	/// <summary>
	/// Represents and address that the US Postal Service can validate using their public web page.
	/// </summary>
	public class UspsAddress {

		public string Street{ get; set; }
		
		/// <summary>Apartment, suite, pobox</summary>
		public string Street2{ get; set; }
		
		public string City { get; set; }
		
		/// <summary>Capitalized 2-leter abreviation</summary>
		public string StateAbrev { get; set; }
		
		public string Zip { get; set; }
		
		public string County { get; set; }
		
	}

	/// <summary>Indicates if the USPS thinks an address is valid, not, or undeliverable.</summary>
	public enum UspsAddressValidity { None = 0,
		/// <summary>The address is not recognized.</summary>
		Invalid,
		/// <summary>The address represents a possible alternate to a proposed invalid address.</summary>
		Alternate, 
		/// <summary>The address is known to be valid but the USPS does not deliver to it.</summary>
		Nondeliverable,
		/// <summary>The address is known to be valid and deliverable.</summary>
		Deliverable,
	};
	
	/// <summary>
	/// Parses HTML response from an address verification request
	/// </summary>
	public class UspsAddressPage  {

		HtmlDocument _doc;
		HtmlNode Root => _doc.DocumentNode;

		public void LoadContent(string source) {
			_doc = new HtmlDocument(source);

			// validate page
			if( this.Root.FindFirst("title").InnerHtml != "USPS.com&reg; - ZIP Code&#153; Lookup" )
				throw new Exception("Page did not have correct title");

			// determine address validity
			if( this.Root.Find("div.noresults-container li.error").FirstOrDefault() != null )
				this.Validity = UspsAddressValidity.Invalid;
			else if( this.Root.Find("p.multi").FirstOrDefault() != null )
				this.Validity = UspsAddressValidity.Alternate;
			else if( this.Root.Find("p#nonDeliveryMsg").FirstOrDefault() != null )
				this.Validity = UspsAddressValidity.Nondeliverable;
			else 
				this.Validity = UspsAddressValidity.Deliverable;

			this.ReturnedAddresses = this.Root.Find("div#results-content p.std-address")
				.Select( node => new UspsAddress {
					Street     = node.FindFirst("span.address1").InnerText.Trim(),
					City       = node.FindFirst("span.city").InnerText.Trim(),
					StateAbrev = node.FindFirst("span.state").InnerText,
					Zip        = node.FindFirst("span.zip").InnerText + "-" + node.FindFirst("span.zip4").InnerText,
					County     = node.Find("dt").Where( n=>n.InnerHtml=="County").First().NextSibling.InnerText.Trim(),
 				})
				.ToList();

		}

		/// <summary>
		/// Address returned from the search.
		/// </summary>
		public List<UspsAddress> ReturnedAddresses { get; private set; }

		/// <summary>
		/// Invalid, Alternate, Nondeliverable, Deliverable
		/// </summary>
		public UspsAddressValidity Validity{ get; private set; }

		/// <summary>
		/// Returns true if submitted address matches exactly 1 known (deliverable or non-deliverable) address.
		/// </summary>
		public bool IsValid {
			get {
				if( this.ReturnedAddresses.Count != 1 ){ return false; }
				return this.Validity == UspsAddressValidity.Nondeliverable 
					|| this.Validity == UspsAddressValidity.Deliverable;
			}
		}

		/// <summary>
		/// Gets single matched address.
		/// </summary>
		/// <returns>null if there is not a single matched address</returns>
		public UspsAddress ValidAddress {
			get{ return this.IsValid ? this.ReturnedAddresses[0] : null; }
		}

		/// <summary>
		/// Builds the request.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		static public ScrapeRequest BuildRequest( UspsAddress address ){
			if( address == null ){ throw new ArgumentNullException("address"); }

			// validate and clean up
			address.Street = ValidateAndCleanup(address.Street, "address.Street");
			address.City = ValidateAndCleanup(address.City, "address.City");
			address.StateAbrev = ValidateAndCleanup(address.StateAbrev, "address.State");

			/*
			https://tools.usps.com/go/ZipLookupResultsAction!input.action?resultMode=0&companyName=&address1=16000+cr+85&address2=&city=findlay&state=OH&urbanCode=&postalCode=&zip=45840
			*/

			NameValueCollection queryStringData = new NameValueCollection{
				{ "address1",   address.Street                  }, // REQUIRED
				{ "address2",   address.Street2 ?? string.Empty },
				{ "city",       address.City                    }, // REQUIRED
				{ "state",      address.StateAbrev              },
				{ "zip",        address.Zip ?? string.Empty     },
				{ "resultMode", "0" },
				{ "companyName", string.Empty },
				{ "urbanCode",   string.Empty },
				{ "postalCode",  string.Empty },
			};
			
			return new ScrapeRequest("https://tools.usps.com/go/ZipLookupResultsAction!input.action?"
				+ScrapeRequest.ToQuery( queryStringData )
			);

		}

		#region private methods

		/// <summary>
		/// helper - makes sure string is not null nor empty whitespace, trims
		/// </summary>
		static string ValidateAndCleanup(string s, string varName){
			if( s == null ){ throw new ArgumentNullException( varName ); }
			s = s.Trim();
			if( s.Length == 0 ){ throw new ArgumentException("string is nothing but whitespace",varName); }
			return s;
		}

		#endregion
		
		
	}
}
