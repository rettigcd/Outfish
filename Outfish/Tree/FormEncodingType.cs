using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Outfish {

	/// <summary> The 3 encodings that can be used when submitting an HTML form </summary>
	public enum FormEncodingType {

		// Default. All characters are encoded before sent (spaces are converted to "+" symbols, and special characters are converted to ASCII HEX values)
		ApplicationXFormUrlEncoded, // "application/x-www-form-urlencoded"   

		// "multipart/form-data" No characters are encoded. This value is required when you are using forms that have a file upload control
		MultipartFormData,

		// "text/plain" Spaces are converted to "+" symbols, but no special characters are encoded
		TextPlain, 

	}

	/// <summary> Parses html form encoding types. </summary>
	public class FormEncodingTypeStrings {

		public static FormEncodingType ParseFormEcoding( string enctype ) {
			enctype = (enctype ?? string.Empty).ToLower();
			switch( enctype ) {
				case MultiPartFormData.ContentTypeStr:
					return FormEncodingType.MultipartFormData;

				case TextPlainFormData.ContentTypeStr:
					return FormEncodingType.TextPlain;

				case UrlEncodedFormData.ContentTypeStr:
				default:
					return FormEncodingType.ApplicationXFormUrlEncoded;
			}
		}

	}

}
