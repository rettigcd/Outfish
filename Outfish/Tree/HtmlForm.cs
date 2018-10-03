using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Outfish {

	public class HtmlForm {

		/// <summary>
		/// Constructs a blank form that defaults to GET / url-encoded
		/// </summary>
		public HtmlForm() {
			_actionResolver = NullUriResolver.Singleton;
			this.Encoding = FormEncodingType.ApplicationXFormUrlEncoded;
			this.Method = "GET";
		}

		/// <summary>
		/// Constructs a Form from an HtmlNode tree.
		/// </summary>
		/// <param name="node">required</param>
		/// <param name="absoluteDocumentLocation">optional, required if accessing form with Relative Action</param>
		public HtmlForm( HtmlNode node, Uri absoluteDocumentLocation = null ) {
			if( node == null ) throw new ArgumentNullException( nameof( node ) );

			this.Node = node;
			_actionResolver = new LazyUriResolver( absoluteDocumentLocation, node["action"] );
			this.Encoding = FormEncodingTypeStrings.ParseFormEcoding( node["enctype"] );
			this.Method = (node["method"]!=null && node["method"].ToLower()=="post") ? "POST" : "GET";

			Fields.AddRange(AddInputFieldsAndButtons(node));
			Fields.AddRange(AddSelects( node ));
			Fields.AddRange(AddTextAreas(node));

		}

		public HtmlNode Node { get; private set; }

		public string Method { get; set; }

		/// <summary> Absolute url of action.</summary>
		public Uri Action {
			get { return _userSetAction ?? (_userSetAction=_actionResolver.Result); }
			set { _userSetAction = value; }
		}
		Uri _userSetAction;

		/// <summary> Encoding used to submit form fields.  Defaults to ApplicationXFormUrlEncoded </summary>
		public FormEncodingType Encoding { get; set; }

		public List<HtmlFormField> Fields { get; private set; } = new List<HtmlFormField>();

		/// <summary>
		/// Adds an enabled FormField to the list of fields.
		/// </summary>
		public void AddField(string name, string value) {
			Fields.Add(new HtmlFormField( name, value ) );
		}

		/// <summary>
		/// Finds the single field in the feild list matching the name.
		/// If there are no fields, returns ItemNotFoundException
		/// If multiple fields, returns InvalidOperationException
		/// </summary>
		/// <remarks>
		/// This does not throw the standard ItemNotFound exception because if there were multiple fields, that would be a confusing exception
		/// AND it is better that this method throw only 1 type of exception instead of ItemNotFound for 0 fields and InvalidOperation for multiple fields.
		/// </remarks>
		public HtmlFormField this[string name] {
			get {
				var fields = this.Fields.Where( x => x.Name == name ).ToArray();
				if( fields.Length == 1 )
					return fields[0];
				throw new InvalidOperationException( $"Form contained {fields.Length} fields named {name}. => {_formString}" );
			}
		}


		/// <summary>
		/// Returns name/value pairs of the ENABLED form fields.
		/// </summary>
		public NameValueCollection GetFormData() {
			NameValueCollection formValues = new NameValueCollection();
			foreach( var field in Fields )
				if( !field.Disabled )
					formValues.Add( field.Name, field.Value );

			return formValues;
		}

		public ScrapeRequest GenerateSubmitRequest() {

			// Get
			var formValues = GetFormData();
			if( this.Method == "GET" ) {
				var action = this.Action;
				if(formValues.Count>0)
					action = new Uri( action.ToString() + "?" + UrlEncodedFormData.GetQueryString(formValues) );
				return new ScrapeRequest( action );
			}

			// POST
			if( this.Method == "POST")
				return new ScrapeRequest( Action, GetPostData(formValues) );

			throw new ArgumentException("Unexpected method:"+this.Method);
		}

		#region private

		IPostData GetPostData(NameValueCollection formValues) {
			switch( this.Encoding ) {
				case FormEncodingType.ApplicationXFormUrlEncoded:
					return new UrlEncodedFormData( formValues );
				case FormEncodingType.MultipartFormData:
					return new MultiPartFormData( formValues );
				case FormEncodingType.TextPlain:
					return new TextPlainFormData( formValues );
				default:
					throw new ArgumentException( "Unknown encoding:" + this.Encoding );
			}
		}

		static IEnumerable<HtmlFormField> AddTextAreas(HtmlNode formNode) =>formNode
			.Find( "textarea[name]" )
			.Select( n => new HtmlFormField (
				name: n["name"],
				value: n.InnerText,
				disabled: IsDisabled( n )
			) );

		static IEnumerable<HtmlFormField> AddSelects(HtmlNode formNode) => formNode
			.Find( "select[name]" )
			.Select( n => new HtmlFormField (
				name: n["name"],
				value: PickOptionValue( n ),
				disabled: IsDisabled( n )
			) );

		static string PickOptionValue( HtmlNode selectNode ) {
			var x = selectNode.Find( "option[selected]" ).Select( DetermineOptionValue ).FirstOrDefault();
			var y = selectNode.Find( "option" ).Select( DetermineOptionValue ).FirstOrDefault();
			return x ?? y;
		}

		static IEnumerable<HtmlFormField> AddInputFieldsAndButtons(HtmlNode formNode) => formNode
			.Find( "input[name],button[name]" )
			.Select( n => new HtmlFormField (
				name: n["name"],
				value: n["value"],
				disabled: IsDisabled( n )
			) );

		static bool IsDisabled( HtmlNode n ) =>!string.IsNullOrEmpty( n["disabled"] );

		static string DetermineOptionValue(HtmlNode opt) => opt["value"] ?? opt.InnerHtml;

		UriResolver _actionResolver;

		string _formString => Node != null ? Node.OuterHtml : "[unknown form string]";

		#endregion

	}

	// allows us store document uri and Action string and delay determining if they are valid.  
	// Don't want 1 invalid URI to throw exception, preventing us from working with a different form.
	interface UriResolver { Uri Result { get; } }
	class LazyUriResolver : UriResolver {
		public LazyUriResolver( Uri baseUrl, string relative ) { _baseUrl = baseUrl; _uriString = relative; }
		public Uri Result => _result ?? (_result = Resolve());
		#region private
		Uri _baseUrl;
		string _uriString;
		Uri _result;
		Uri Resolve() {
			if( Uri.IsWellFormedUriString( _uriString, UriKind.Absolute ) )
				return new Uri( _uriString, UriKind.Absolute );

			if( Uri.IsWellFormedUriString( _uriString, UriKind.Relative ) ) {
				if( _baseUrl == null ) throw new ArgumentException( "Unable to resolve relative form Action.  Null reference document location." );
				if( !_baseUrl.IsAbsoluteUri ) throw new ArgumentException( "Unable to resolve relative form Action.  Reference document location is not an absolute Uri.: " + _baseUrl );
				return new Uri( _baseUrl, _uriString );
			}

			throw new FormatException( $"Invalid form action [{_uriString}]" );
		}
		#endregion
	}

	class NullUriResolver : UriResolver {
		static public NullUriResolver Singleton = new NullUriResolver();
		NullUriResolver() { } // hide
		public Uri Result => null;
	}

}
