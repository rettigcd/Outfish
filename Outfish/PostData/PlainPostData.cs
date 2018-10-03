using System;

namespace Outfish {
	
	/// <summary>
	/// Uses UTF-8 to encode plane text as post data.
	/// </summary>
	public class PlainPostData : IPostData {
		
		/// <param name="contentType">Typically plain/text or plain/xml or text/plain.</param>
		/// <param name="postDataString">Data to include in the post body</param>
		public PlainPostData( string contentType, string postDataString ) {
			if( contentType == null ){ throw new ArgumentNullException("contentType"); }
			this.ContentType = contentType;
			
			this.PostBytes = string.IsNullOrEmpty( postDataString )
				? new byte[0]
				: System.Text.Encoding.UTF8.GetBytes( postDataString );
		}
		
		/// <summary>Gets the bytes to embed in the POST REQUEST body.</summary>
		public byte[] PostBytes { get; private set; }
		
		/// <summary>Gets the ContentType to embed in the POST REQUEST header.</summary>
		public string ContentType { get; private set; }
		
	}
	
}
