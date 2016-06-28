using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace NSMk_xaml {
    public class MKXOptions : ICodeDomGenerationUtil {
        #region ctor
        public MKXOptions() : this("c#") { }
        public MKXOptions(string whichType) {
            switch (whichType.ToLower()) {
                case "vb": provider = new Microsoft.VisualBasic.VBCodeProvider(); break;
                case "cpp": case "c++": provider = new Microsoft.VisualC.CppCodeProvider10(); break;
                case "js": provider = new Microsoft.JScript.JScriptCodeProvider(); break;
                case "c#":
                case "csharp":
                default:
                    provider = new CSharpCodeProvider();
                    break;
            }
            useCompileUnit = false;
        }
        #endregion

        #region properties
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
        }
        #endregion ICodeDomGenerationUtil implementation
    }
}