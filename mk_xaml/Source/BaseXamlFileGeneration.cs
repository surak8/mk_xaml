using System.CodeDom;
using System.Xml;

namespace NSMk_xaml {
    abstract class BaseXamlFileGeneration : IXamlFileGenerationData {

        #region protected properties
        protected string localXamlName { get; set; }
        protected string localCodeBehind { get; set; }
        protected string localViewModelName { get; set; }
        #endregion

        #region abstract properties
        protected abstract string localElementName { get; }
        public abstract string localFileName { get; }
        protected abstract GenFileType localGenerationType { get; }
        #endregion

        #region virtual properties
        protected virtual bool shouldGenerateViewmodel { get { return false; } }
        public virtual string localNamespace { get { return string.Empty; } }
        #endregion virtual properties

        #region virtual methods
        protected virtual void addLocalImports(CodeNamespace ns) { }
        protected virtual void localGenerateCode(CodeNamespace ns, CodeTypeDeclaration ctd, CodeConstructor cc) { }
        protected virtual void localGenerateModelCode(CodeNamespace ns, CodeTypeDeclaration ctd) { }
        protected virtual void writeElementAttributes(XmlWriter xw) { }
        protected virtual void writeElement(XmlWriter xw) { }
        #endregion virtual methods

        #region IXamlFileGenerationData implementation
        #region IXamlFileGenerationData properties

        string IXamlFileGenerationData.elementName { get { return localElementName; } }
        string IXamlFileGenerationData.fileName { get { return localFileName; } }
        bool IXamlFileGenerationData.generateViewModel { get { return shouldGenerateViewmodel; } }
        GenFileType IXamlFileGenerationData.generationType { get { return localGenerationType; } }
        string IXamlFileGenerationData.nameSpace { get { return localNamespace; } }

        // read/write properties
        string IXamlFileGenerationData.codeBehindName { get { return localCodeBehind; } set { localCodeBehind = value; } }
        string IXamlFileGenerationData.viewModelName { get { return localViewModelName; } set { localViewModelName = value; } }
        string IXamlFileGenerationData.xamlName { get { return localXamlName; } set { localXamlName = value; } }

        #endregion IXamlFileGenerationData properties

        #region IXamlFileGenerationData methods
        void IXamlFileGenerationData.addImports(CodeNamespace ns) { addLocalImports(ns); }
        void IXamlFileGenerationData.generateCode(CodeNamespace ns, CodeTypeDeclaration ctd, CodeConstructor cc) { localGenerateCode(ns, ctd, cc); }
        void IXamlFileGenerationData.generateModelCode(CodeNamespace ns, CodeTypeDeclaration ctd) { localGenerateModelCode(ns, ctd); }
        void IXamlFileGenerationData.populateElement(XmlWriter xw) { writeElement(xw); }
        void IXamlFileGenerationData.populateElementAttributes(XmlWriter xw) { writeElementAttributes(xw); }
        #endregion IXamlFileGenerationData methods
        #endregion IXamlFileGenerationData implementation

    }
}