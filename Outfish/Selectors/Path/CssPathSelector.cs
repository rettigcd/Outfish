using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Outfish {



	/// <summary>Heirachy (multi-step) html node predicate</summary>
	/// <remarks>a css path is several steps separated by a space (' ')</remarks>
	internal class CssPathSelector : IDescendantFinder {

		#region constructor
		
		public CssPathSelector( IEnumerable<StepSelector> stepSelectors ){
					
			this._steps = stepSelectors.ToArray(); 
		}
		
		#endregion
	
		public IEnumerable<HtmlNode> FindDescendantNodes( HtmlNode node ){

			IEnumerable<HtmlNode> nodes = new HtmlNode[]{ node };

			foreach(var step in this._steps){
				//DateTime start = DateTime.Now;
				// nodes = nodes.SelectMany<HtmlNode,HtmlNode>( step.Search );
				
				nodes = step.Search( nodes );
				
				//TimeSpan dif = DateTime.Now - start;
				//System.Diagnostics.Debug.WriteLine( dif );
			}

			return nodes;

		}

		public IEnumerable<XmlNode> FindDescendantNodes( XmlNode node ) {
			IEnumerable<XmlNode> nodes = new XmlNode[] { node };

			foreach( var step in this._steps ) {
				//DateTime start = DateTime.Now;
				// nodes = nodes.SelectMany<HtmlNode,HtmlNode>( step.Search );

				nodes = step.Search( nodes );

				//TimeSpan dif = DateTime.Now - start;
				//System.Diagnostics.Debug.WriteLine( dif );
			}

			return nodes;
		}


		public override string ToString() {
			return _steps
				// filter out nodes that have child selector
				// because those will be drawn by their children
				// AND we don't want an extra ' ' delimter in the result (which would happen if it returned an empty string.
				.Where( step => !step.HasChildSelector )
			.Join(" ");
		}

		#region private fields

		StepSelector[] _steps;
	
		#endregion
	
	}
	





}
