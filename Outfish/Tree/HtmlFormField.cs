using System;

namespace Outfish {

	public class HtmlFormField {

		public HtmlFormField(string name, string value=null, bool disabled=false) {
			Name = name;
			Value = value;
			Disabled = disabled;
		}

		public string Name { get; private set; }
		public string Value { get; set; }
		public bool Disabled { get; set; }
	}

}
