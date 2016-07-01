using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using Microsoft.VisualC;
using Microsoft.JScript;

namespace NSMk_xaml {
    public class MKXOptions : ICodeDomGenerationUtil {

        public enum LangaugeType {
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

        #region methods
        internal void setLanguageByName(string aName) {
            switch (aName.ToLower()) {
                case "vb": this.generationType = LangaugeType.VB; break;
                case "cpp": case "c++": this.generationType = LangaugeType.CPP; break;
                case "js": this.generationType = LangaugeType.JavaScript; break;
                case "c#": case "csharp": this.generationType = LangaugeType.CSharp; break;
            }
        }

        internal void setGeneratedLanguage(LangaugeType lt) {
            this.generationType = lt;
        }

        #endregion

        #region properties
        LangaugeType generationType { get; set; }
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
                case LangaugeType.VB: this.provider = new VBCodeProvider(); break;
                case LangaugeType.CPP: this.provider = new CppCodeProvider10(); break;
                case LangaugeType.JavaScript: this.provider = new JScriptCodeProvider(); break;
                case LangaugeType.CSharp: this.provider = new CSharpCodeProvider(); break;
            }
        }
        #endregion ICodeDomGenerationUtil implementation
    }
}