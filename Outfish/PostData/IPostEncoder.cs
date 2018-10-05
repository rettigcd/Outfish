
namespace Outfish {
	
	/// <summary>
	/// Represents the different ways that POST data can be encoded
	/// </summary>
	/// <remarks>
	/// To the enctype attribute determines which form type to encode.
	/// </remarks>
	public interface IPostData {
	
		/// <summary>
		/// Gets the encoded bytes of the POST data ready to be used as the request body
		/// </summary>
		byte[] PostBytes{ get; }
		
		/// <summary>
		/// Gets the method used to encode the bytes.  For use in the HTTP content-type header.
		/// </summary>
		string ContentType{ get; }
		
	}
}
