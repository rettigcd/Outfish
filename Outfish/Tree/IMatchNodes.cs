using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Outfish.Xml
{
	interface IMatchNodes{
		bool Match( HtmlNode node );
	}
}
