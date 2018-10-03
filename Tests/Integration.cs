using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Outfish;
using Outfish.Usps;

namespace Outfish_Integration_Tests {
	
	[TestFixture]
	public class Usps 	{

		[Test,Explicit]
		public void Integration_Matching_Address(){
			
			// This is an integratino test - actually hits the website
			
			UspsAddress address = new UspsAddress{
				Street = "7200 Royalgreen Drive",
				City = "Cincinnati",
				State = "OH",
			};
			
			UspsAddressPage page = new ScreenScraper(20000).GetPage<UspsAddressPage>(
				UspsAddressPage.BuildRequest( address )
			);

			Assert.That( page.IsMatch, Is.True );
			Assert.That( page.ReturnedAddresses.Count, Is.EqualTo(1) );
			UspsAddress result = page.ReturnedAddresses[0];
			Assert.That( result.Validity, Is.EqualTo(UspsAddressValidity.Valid ));
			
			Assert.That( result.Street, Is.EqualTo("7200 ROYALGREEN DR") );
			Assert.That( result.Street2, Is.Null );
			Assert.That( result.City, Is.EqualTo("CINCINNATI") );
			Assert.That( result.State, Is.EqualTo("OH") );
			Assert.That( result.Zip, Is.EqualTo("45244-3625") );
			Assert.That( result.County, Is.EqualTo("HAMILTON") );
		}
		
	}
}
