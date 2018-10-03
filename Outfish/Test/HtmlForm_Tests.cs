using System;
using NUnit.Framework;
using Outfish;

namespace Outfish_Test {

	[TestFixture]
	public class HtmlForm_Tests {

		[Test]
		public void MissingField_ThrowsExceptionWithFieldName() {
			var form = new HtmlForm();
			string fieldName = "nonExistingField";
			var ex = Assert.Throws<InvalidOperationException>( () => { var x = form[ fieldName ]; } );
			Assert.That(ex.Message,Contains.Substring( "nonExistingField" ) );
		}

		[Test]
		public void MultipleFields_ThrowsExceptionWithFieldName() {
			var form = new HtmlForm();
			form.AddField( "bob","value1" );
			form.AddField( "bob", "value2" );
			Assert.Throws<InvalidOperationException>( () => { var x = form["bob"]; } );
		}

		[Test]
		public void RecordDocumentUriInHtmlDocument() {
			var uri = new Uri( "http://www.google.com" );
			var doc = new HtmlDocument( "<root>", uri );
			Assert.That( uri, Is.EqualTo( uri ) );
		}

		[Test]
		public void ScrapingHtmlDocument_ReturnsDocWithTargetUrl() {
			var uri = new Uri( "http://www.google.com" );
			HtmlDocument doc = new WebScraper().GetDocument( new ScrapeRequest( uri ) );
			Assert.That( doc.Location, Is.EqualTo( uri ) );
		}

		[Test]
		public void GetFormWithNoAttributesFromPage() {
			var doc = new HtmlDocument( "<html><body><form></form></body></html>" );
			var form = doc.FindForm( "form" );
		}

		[Test]
		public void AbsoluteAction() {
			var doc = new HtmlDocument( "<html><body><form aCtIoN='http://www.google.com'></form></body></html>" );
			var form = doc.FindForm( "form" );
			Assert.True( form.Action.IsAbsoluteUri );
		}

		[Test]
		public void RelativeActionWithoutReferenceDocument_ThrowsException() {
			var doc = new HtmlDocument( "<html><body><form aCtIoN='../bob.html'></form></body></html>" );
			// Cause the lazy evaluator to evaluate
			Assert.Throws<ArgumentException>( () => { var x = doc.FindForm().Action; } );
		}

		[Test]
		public void RelativeActionWithReferenceDocument_CalculatesValidAction() {
			var doc = new HtmlDocument( "<html><body><form aCtIoN='../bob.html'></form></body></html>", new Uri( "https://www.google.com/bill/ted/home.html" ) );
			var uri = doc.FindForm( "form" ).Action;
			Assert.That( uri.ToString(), Is.EqualTo( "https://www.google.com/bill/bob.html" ) );
		}

		[Test]
		public void Inputs() {
			var doc = new HtmlDocument( "<html><body><form><input name='bob' value='x1'><input name='ted' value='y1' disabled='disabled'></form></body></html>" );
			var form = doc.FindForm( "form" );
			FieldIs( form.Fields[0], "bob", "x1", false );
			FieldIs( form.Fields[1], "ted", "y1", true );
		}

		[Test]
		public void TextAreas() {
			var doc = new HtmlDocument( "<html><body><form><textarea name='bob'>oh happy day for joyous glee</textarea></form></body></html>" );
			var form = doc.FindForm();
			FieldIs( form.Fields[0], "bob", "oh happy day for joyous glee", false );
		}

		[Test]
		public void SelectsWithNoOption_Null() {
			var doc = new HtmlDocument( "<html><body><form><select name='bob'></select></form></body></html>" );
			var form = doc.FindForm();
			FieldIs( form.Fields[0], "bob", null, false );
		}

		[Test]
		public void SelectsWithNoSelectedOption_UsesFirstOption() {
			var doc = new HtmlDocument( "<html><body><form><select name='bob'><option value='xx'>yy</option></select></form></body></html>" );
			var form = doc.FindForm();
			FieldIs( form.Fields[0], "bob", "xx", false );
		}

		[Test]
		public void SelectsWithSelectedOption_UsesSelectedOption() {
			var doc = new HtmlDocument( @"<html><body><form>
				<select name='bob'>
					<option value='value1'>text1</option> 
					<option value='value2' selected>text2</option>
				</select></form></body></html>" );
			var form = doc.FindForm();
			FieldIs( form.Fields[0], "bob", "value2", false );
		}

		[Test]
		public void MissingOptionValue_UsesText() {
			var doc = new HtmlDocument( @"<html><body><form><select name='bob'><option>text1</option></select></form></body></html>" );
			var form = doc.FindForm();
			FieldIs( form.Fields[0], "bob", "text1", false );
		}

		[Test]
		public void FormWithNoMethd_DefaultsToGet() {
			var doc = new HtmlDocument( @"<html><body><form></form></body></html>" );
			var form = doc.FindForm();
			Assert.That( form.Method, Is.EqualTo( "GET" ) );
		}

		[Test]
		public void FormWithPostMethd_IsPost() {
			var doc = new HtmlDocument( @"<html><body><form mEtHoD='PoSt'></form></body></html>" );
			var form = doc.FindForm();
			Assert.That( form.Method, Is.EqualTo( "POST" ) );
		}

		[Test]
		public void FieldsWithNoNames_AreIgnored() {
			var doc = new HtmlDocument( @"<html><body><form><input value='xx'><button value='ted'><textarea>aa</textarea><select></select></form></body></html>" );
			var form = doc.FindForm();
			Assert.That( form.Fields.Count, Is.EqualTo( 0 ) );
		}

		[TestCase("bob","ted",false)]
		[TestCase( "bob", null, true )]
		[TestCase( "bob", "", false )]
		public void FieldProperties_AreInitialized(string name, string value, bool disabled) {
			FieldIs( new HtmlFormField( name, value, disabled ), name, value, disabled);
		}

		void FieldIs( HtmlFormField field, string name, string value, bool disabled ) {
			Assert.That( field.Name, Is.EqualTo( name ) );
			Assert.That( field.Value, Is.EqualTo( value ) );
			Assert.That( field.Disabled, Is.EqualTo( disabled ) );
		}

	}

}
