using System;
using System.Collections.Generic;
using System.IO;

using Outfish;
using Outfish.Usps;
using NUnit.Framework;

namespace Outfish_Unit_Tests {

	[TestFixture]
	public class UspsAddressPage_Test {

		
		
		[Test]
		public void Parse_Matching_Address(){
			string content = File.ReadAllText("valid_deliverable.html");
			UspsAddressPage page = new UspsAddressPage( content );
			
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
		
		[Test]
		public void Parse_Valid_Non_Deliverable(){
			string content = File.ReadAllText("valid_non_deliverable.html");
			UspsAddressPage page = new UspsAddressPage( content );
			
			Assert.That( page.IsMatch, Is.True );
			Assert.That( page.ReturnedAddresses.Count, Is.EqualTo(1) );
			UspsAddress result = page.ReturnedAddresses[0];
			Assert.That( result.Validity, Is.EqualTo(UspsAddressValidity.Nondeliverable ));
			
			Assert.That( result.Street, Is.EqualTo("515 S 7TH ST") );
			Assert.That( result.Street2, Is.Null );
			Assert.That( result.City, Is.EqualTo("TERRE HAUTE") );
			Assert.That( result.State, Is.EqualTo("IN") );
			Assert.That( result.County, Is.EqualTo("VIGO") );
			Assert.That( result.Zip, Is.EqualTo("47807") );
			
		}

		[Test]
		public void Parse_Invalid_Alternate(){
			string content = File.ReadAllText("invalid_alternate.html");
			UspsAddressPage page = new UspsAddressPage( content );
			
			Assert.That( page.IsMatch, Is.False );
			Assert.That( page.ReturnedAddresses.Count, Is.EqualTo(3) );
			// 0
			UspsAddress result = page.ReturnedAddresses[0];
			Assert.That( result.Validity, Is.EqualTo(UspsAddressValidity.Alternate));
			Assert.That( result.Street, Is.EqualTo("517 S 7TH ST") );
			Assert.That( result.Street2, Is.Null );
			Assert.That( result.City, Is.EqualTo("TERRE HAUTE") );
			Assert.That( result.State, Is.EqualTo("IN") );
			Assert.That( result.County, Is.EqualTo("VIGO") );
			Assert.That( result.Zip, Is.EqualTo("47807-4447") );
			// 1
			result = page.ReturnedAddresses[1];
			Assert.That( result.Validity, Is.EqualTo(UspsAddressValidity.Alternate));
			Assert.That( result.Street, Is.EqualTo("517 S 7TH ST APT (Range 1 - 8)") );
			Assert.That( result.Street2, Is.Null );
			Assert.That( result.City, Is.EqualTo("TERRE HAUTE") );
			Assert.That( result.State, Is.EqualTo("IN") );
			Assert.That( result.County, Is.EqualTo("VIGO") );
			Assert.That( result.Zip, Is.EqualTo("47807-4447") );
			// 2
			result = page.ReturnedAddresses[2];
			Assert.That( result.Validity, Is.EqualTo(UspsAddressValidity.Alternate));
			Assert.That( result.Street, Is.EqualTo("(ODD Range 501 - 599) S 7TH ST") );
			Assert.That( result.Street2, Is.Null );
			Assert.That( result.City, Is.EqualTo("TERRE HAUTE") );
			Assert.That( result.State, Is.EqualTo("IN") );
			Assert.That( result.County, Is.EqualTo("VIGO") );
			Assert.That( result.Zip, Is.EqualTo("47807-4407") );
		}
		
		
	}
	
	
}