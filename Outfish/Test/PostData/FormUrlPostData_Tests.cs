using System.Collections.Specialized;
using NUnit.Framework;
using Outfish;

namespace Outfish_Test.PostData {

	[TestFixture]
	public class FormUrlPostData_Tests {

		NameValueCollection _data;

		[SetUp]
		public void SetUp() {
			_data = new NameValueCollection();
		}

		[Test]
		public void FormDataWithNulls() {

			// Given: form data with mixed strings values and null values
			_data.Add("key1","value1");
			_data.Add("key2",null);
			_data.Add("key3","value3");
			_data.Add("key3",null);

			string query = When_ConvertToQuery();

			// Then: 
			Assert.That(query, Is.EqualTo("key1=value1&key3=value3"));

		}

		[Test]
		public void ValuesWith_UrlCharacter() {
			_data.Add("key","< >&");
			string query = When_ConvertToQuery();
			Assert.That( query, Is.EqualTo( "key=%3c+%3e%26" ) );
		}

		[Test]
		public void KeysWith_UrlCharacter() {
			_data.Add( "a key", "ted" );
			string query = When_ConvertToQuery();
			Assert.That( query, Is.EqualTo( "a+key=ted" ) );
		}

		[Test]
		public void ValueIsEmpty() {
			_data.Add( "key", string.Empty );
			string query = When_ConvertToQuery();
			Assert.That( query, Is.EqualTo( "key=" ) );
		}


		string When_ConvertToQuery() {
			return UrlEncodedFormData.GetQueryString( _data );
		}

	}
}
