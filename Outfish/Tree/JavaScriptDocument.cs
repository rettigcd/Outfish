namespace Outfish {

	/// <summary>
	/// A Document Object Model of JavaScript literals.
	/// </summary>
	public class JavaScriptDocument : WebDocument {
		
		/// <summary>
		/// Creates a JavaScriptDocument inserting a phantom root-script tag, using scriptContent as the JavaScript content
		/// Throws ParseException if can't parse content.
		/// </summary>
		public JavaScriptDocument( string content )
			:base(content, new JavaScriptDocumentParser( content ).BuildStructureTree(), null ) {}

	}



}
