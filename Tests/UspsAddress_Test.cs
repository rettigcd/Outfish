//using System.Reflection;
//using NUnit.Framework;
//using Outfish.Usps;

//namespace Outfish_Test {

//	[TestFixture]
//	public class UspsAddress_Test 	{
	
//		[Test]
//		public void Parse_Valid_Address_Response_Test(){

//			// Given: we have a valid-address response
//			string html = ReadResourceFile( "ValidAddress" );

//			// When: we parse it
//			UspsAddressPage page = new UspsAddressPage();
//			page.LoadContent( html );

//			// Then: results indicate a valid address
//			Assert.True( page.IsValid );
//			var validAddress = page.ValidAddress;
//			Assert.IsNotNull( validAddress );
//			Assert.That( validAddress.Street,     Is.EqualTo("831 42ND ST") );
//			Assert.That( validAddress.City,       Is.EqualTo("DES MOINES") );
//			Assert.That( validAddress.StateAbrev, Is.EqualTo("IA") );
//			Assert.That( validAddress.Zip,        Is.EqualTo("50312-2613") );
//			Assert.That( validAddress.County,     Is.EqualTo("POLK") );

//		}

//		[Test]
//		public void Parse_Alternate_Address_Response_Test(){
		
//			// Given: we have an alternate-address response
//			string html = ReadResourceFile("AlternateAddresses");
			
//			// When we parse it
//			UspsAddressPage page = new UspsAddressPage();
//			page.LoadContent( html );
			
//			// Then: results are alterate, not valid
//			Assert.False( page.IsValid );
//			Assert.That( page.ReturnedAddresses.Count, Is.EqualTo( 26 ) );
//			Assert.That( page.Validity, Is.EqualTo(UspsAddressValidity.Alternate) );
			
//		}

//		[Test]
//		public void Parse_Invalid_Address_Response_Test(){
//			// Given: we have an alternate-address response
//			string html = ReadResourceFile("InvalidAddress");
			
//			// When we parse it
//			UspsAddressPage page = new UspsAddressPage();
//			page.LoadContent( html );
			
//			// Then: results are alterate, not valid
//			Assert.False( page.IsValid );
//			Assert.That( page.ReturnedAddresses.Count, Is.EqualTo( 0 ) );
//		}

//		[Test]
//		public void Parse_Undeliverable_Address_Response_Test(){
//			// Given: we have an alternate-address response
//			string html = ReadResourceFile("AddressUndeliverable");
			
//			// When we parse it
//			UspsAddressPage page = new UspsAddressPage();
//			page.LoadContent( html );
			
//			// Then: results are alterate, not valid
//			Assert.True( page.IsValid );
//			Assert.That( page.ReturnedAddresses.Count, Is.EqualTo( 1 ) );
//			Assert.That( page.Validity, Is.EqualTo( UspsAddressValidity.Nondeliverable ) );
//		}

////		[Test,Explicit]
////		public void Call_Usps_Integration_Test(){
////			
////			var address = new UspsAddress{
////				Street = "831 42nd street",
////				City = "DES MOINES",
////				StateAbrev = "IA",
////				Zip = "50312"
////			};
////			
////			var request = UspsAddressPage.BuildRequest( address );
////			var page = new WebScraper().GetPage<UspsAddressPage>( request );
////			
////			Assert.True( page.IsValid );
////			Assert.That( page.MatchedAddress.Validity, Is.EqualTo(UspsAddressValidity.Deliverable) );
////			
////		}
		
		
//		string ReadResourceFile( string resourceName ){
//			using( var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream( resourceName ) ){
//				using( var reader = new System.IO.StreamReader( stream ) ){
//					return reader.ReadToEnd();
//				}
//			}
//		}

//	}
//}
