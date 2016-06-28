using System.CodeDom;
using System.Reflection;
using System.Xml;

namespace NSMk_xaml {
    class Tester : BaseXamlFileGeneration {
        GenFileType _gentype;

        public Tester(GenFileType gft) {
            _gentype = gft;
        }

        #region abstract implementation
        protected override string localElementName {
            get {
                switch (localGenerationType) {
                    case GenFileType.NONE: return "NoneElementName";
                    case GenFileType.Application: return "Application";
                    case GenFileType.View: return "Page";
                    case GenFileType.Model: return "ModelElementName";
                    default: return "DefaultElementName";
                }
            }
        }

        protected override string localFileName {
            get {
                switch (localGenerationType) {
                    case GenFileType.NONE: return "NoneFileName";
                    case GenFileType.Application: return "App";
                    case GenFileType.View: return "ViewFileName";
                    case GenFileType.Model: return "ModelFileName";
                    default: return "DefaultFileName";
                }
            }
        }

        protected override GenFileType localGenerationType { get { return _gentype; } }
        #endregion abstract implementation

        #region virtual implementation

        protected override string localNamespace { get { return "NSDummy"; } }

        protected override void addLocalImports(CodeNamespace ns) {
            Logger.log(MethodBase.GetCurrentMethod());
            base.addLocalImports(ns);
            ns.Imports.Add(new CodeNamespaceImport("System"));
        }

        protected override void localGenerateCode(CodeNamespace ns, CodeTypeDeclaration ctd, CodeConstructor cc) {
            Logger.log(MethodBase.GetCurrentMethod());
            base.localGenerateCode(ns, ctd, cc);
            if (cc == null) {
                ctd.Members.Add(cc = new CodeConstructor());
                cc.Attributes = 0;
            }
            cc.Statements.Add(new CodeCommentStatement("statements for " + this.localGenerationType));
        }

        protected override void localGenerateModelCode(CodeNamespace ns, CodeTypeDeclaration ctd) {
            Logger.log(MethodBase.GetCurrentMethod());
            base.localGenerateModelCode(ns, ctd);
        }

        protected override bool shouldGenerateViewmodel {
            get {
                if (localGenerationType == GenFileType.Application) return false;
                Logger.log(MethodBase.GetCurrentMethod(), " for type:" + this.localGenerationType);
                return base.shouldGenerateViewmodel;
            }
        }

        #endregion abstract implementation

        protected override void writeElement(XmlWriter xw) {
            base.writeElement(xw);
            if (this.localGenerationType == GenFileType.Application) {
                xw.WriteStartElement("Application.Resources");
                xw.WriteWhitespace("\r\n\t");
                xw.WriteFullEndElement();
            } else {
                Logger.log(MethodBase.GetCurrentMethod());
                xw.WriteComment(" in writeElement ");
            }
        }

        protected override void writeElementAttributes(XmlWriter xw) {
            base.writeElementAttributes(xw);
            if (localGenerationType == GenFileType.Application)
                xw.WriteAttributeString("generationType", "app");
            else
                Logger.log(MethodBase.GetCurrentMethod());
        }
    }
}