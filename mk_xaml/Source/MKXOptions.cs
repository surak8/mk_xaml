using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Microsoft.VisualC;
using Microsoft.JScript;

namespace NSMk_xaml {
	public class MKXOptions : ICodeDomGenerationUtil {

		enum LanguangeType {
			VB,
			CPP,

			JavaScript,
			CSharp
		}

		#region ctor
		public MKXOptions() : this("c#") { }
		public MKXOptions(string whichType) {
			setLanguageByName(whichType);
			useCompileUnit = false;
		}
		#endregion

		internal void setLanguageByName(string aName) {
			switch (aName.ToLower()) {
				case "vb": provider = new VBCodeProvider(); break;
				case "cpp": case "c++": provider = new CppCodeProvider10(); break;
				case "js": provider = new JScriptCodeProvider(); break;
				case "c#":
				case "csharp":
				default:
					provider = new CSharpCodeProvider();
					break;
			}
		}

		#region properties
		LanguangeType generationType { get; set; }
		internal bool useCompileUnit { get; set; }
		#endregion

		#region ICodeDomGenerationUtil implementation
		public CodeGeneratorOptions options { get; private set; }
		public CodeDomProvider provider { get; private set; }

		public void createProvider() {
			if (options == null) {
				options = new CodeGeneratorOptions();
				options.BlankLinesBetweenMembers = false;
				options.ElseOnClosing = true;
				options.IndentString = "\t";
			}
			switch (generationType) {
				case LanguangeType.VB: this.provider = new VBCodeProvider(); break;
				case LanguangeType.CPP: this.provider = new CppCodeProvider10(); break;
				case LanguangeType.JavaScript: this.provider = new JScriptCodeProvider(); break;
				case LanguangeType.CSharp: this.provider = new CSharpCodeProvider(); break;
			}
		}
		#endregion ICodeDomGenerationUtil implementation
	}
}