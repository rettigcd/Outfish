using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;

namespace Outfish_Test.PostData {

	/// <summary>
	/// Helps to explicitly show how NameValueCollection handles duplicate keys and null values.
	/// </summary>
	[TestFixture]
	public class NameValueCollection_Tests {

		NameValueCollection _data;

		[SetUp]
		public void SetUp() {
			_data = new NameValueCollection();
		}

		[Test( Description = ".Count is the number of unique keys. Each key can have multiple values." )]
		public void NameValueCollection_Count_Is_Unique_Keys() {

			// Just to make sure we understand how NameValueCollection handles duplicate keys
			_data.Add( "key", "value1" );
			_data.Add( "key", "value2" );

			Assert.That( _data.Count, Is.EqualTo( 1 ) );
			var values = _data.GetValues( 0 );
			Assert.That( values.Length, Is.EqualTo( 2 ) );

		}

		[Test( Description = "GetValues on a key with only null values returns null array" )]
		public void NullValue_Gives_NullValuesArray() {
			// Just to make sure we understand how NameValueCollection handles duplicate keys
			_data.Add( "key", null );
			_data.Add( "key", null );
			_data.Add( "key", null );
			_data.Add( "key", null );

			Assert.That( _data.Count, Is.EqualTo( 1 ) );
			var values = _data.GetValues( 0 );
			Assert.That( values, Is.Null ); // this is a pain in the ass.

		}

		[Test( Description = "GetValues on a key with a null value returns array without the null values" )]
		public void RepeatKeyWith1NullValue_DiscardsNullValue() {
			// Just to make sure we understand how NameValueCollection handles duplicate keys
			_data.Add( "key", null );
			_data.Add( "key", "bob" );

			Assert.That( _data.Count, Is.EqualTo( 1 ) );
			var values = _data.GetValues( 0 );
			Assert.That( values.Length, Is.EqualTo( 1 ) );
			Assert.That( values[0], Is.EqualTo( "bob" ) );
		}

	}
}
