using System.Collections.Generic;
using NUnit.Framework;
using Outfish.JavaScript;

namespace Outfish_Test {

	[TestFixture]
	public class JsonSerializer_Tests {

		JsonSerializer _sut = new JsonSerializer();

		[TestCase("null", null)]
		[TestCase("\"bob\"", "bob")]
		[TestCase("'bob'", "bob")]
		[TestCase("'\\n'","\n")]
		[TestCase("'\\''","'")]
		[TestCase("\"\\\"\"","\"")]
		[TestCase("\"'\"","'")]
		[TestCase("'\"'","\"")]
		public void DeserializeStringTo( string json, string expectedResult ) {
			string actual = _sut.DeserializeString( json );
			Assert.That( actual, Is.EqualTo( expectedResult ) );
		}

		[TestCase("true",true)]
		[TestCase("true ",true)]
		[TestCase("false",false)]
		[TestCase(" false",false)]
		public void DeserializeBoolean( string json, bool expectedResult ) {
			bool actual = _sut.DeserializeBoolean( json );
			Assert.That( actual, Is.EqualTo( expectedResult ) );
		}

		// numbers
		[TestCase("3",3)]
		[TestCase("  5",5)]
		[TestCase("0",0)]
		[TestCase("3.14",3.14)]
		[TestCase("3.14   ",3.14)]
		[TestCase("1.23E5",1.23E5)]
		[TestCase("1.23E-5",1.23E-5)]
		[TestCase("-1.23E-5",-1.23E-5)]
		[TestCase("   -.23E-5",-.23E-5)]
		[TestCase("-.23E-5",-.23E-5)]
		[TestCase("-0.23E5",-0.23E5)]
		public void DeserializeNumbers( string json, double expectedResult ) {
			double actual = _sut.DeserializeDouble( json );
			Assert.That( actual, Is.EqualTo( expectedResult ) );
		}

		[Test]
		public void Array() {
			List<dynamic> arr = _sut.DeserializeArray("[1,'ted',null,true,[]]");
			Assert.That( arr.Count, Is.EqualTo(5));
			Assert.That( arr[0], Is.EqualTo(1) );
			Assert.That( arr[1], Is.EqualTo("ted"));
			Assert.That( arr[2], Is.Null );
			Assert.That( arr[3], Is.True );
			Assert.That( arr[4], Is.Not.Null );

		}

		[Test]
		public void dyna() {
			dynamic d = "bob";

			string s = d;

		}

		// dictionary

		// line breaks

		// binary

		// invalid stuff
		// [1
		// [1,
		// [1,1]
		// misquoted strings

	}

}
