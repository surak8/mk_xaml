using System.CodeDom;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace NSMk_xaml {
    class Tester : BaseXamlFileGeneration {
        #region constants
        public const string DEFAULT_NAMESPACE = "NSDummy";
        #endregion constants

        #region fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        GenFileType _gentype;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string _namespace;
        #endregion fields

        public Tester(GenFileType gft) : this(gft, DEFAULT_NAMESPACE) { }

        public Tester(GenFileType gft, string ns) {
            _gentype = gft;
            _namespace = ns;
        }

        #region abstract implementation
        protected override string localElementName {
            get {
                switch (localGenerationType) {
                    case GenFileType.NONE: return "NoneElementName";
                    case GenFileType.Application: return "Application";
                    case GenFileType.View: return "Page";
                    case GenFileType.NavigationWindow: return "NavigationWindow";
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
                    case GenFileType.NavigationWindow: return "MainWindow";
                    default: return "DefaultFileName";
                }
            }
        }

        protected override GenFileType localGenerationType { get { return _gentype; } }
        #endregion abstract implementation

        #region virtual implementation

        protected override string localNamespace { get { return _namespace; } }

        protected override void addLocalImports(CodeNamespace ns) {
            base.addLocalImports(ns);
            ns.Imports.Add(new CodeNamespaceImport("System"));
            if (localGenerationType == GenFileType.Application) {
                ns.Imports.Add(new CodeNamespaceImport("System.Windows"));
                ns.Imports.Add(new CodeNamespaceImport("System.Windows.Controls"));
            } else if (localGenerationType == GenFileType.View) {
                ns.Imports.Add(new CodeNamespaceImport("System.Windows.Controls"));
            } else if (localGenerationType == GenFileType.NavigationWindow) {
                ns.Imports.Add(new CodeNamespaceImport("System.Windows"));
                ns.Imports.Add(new CodeNamespaceImport("System.Windows.Navigation"));
            } else
                Logger.log(MethodBase.GetCurrentMethod(), "Type=" + this.localGenerationType);
        }

        protected override void localGenerateCode(CodeNamespace ns, CodeTypeDeclaration ctd, CodeConstructor cc) {
            base.localGenerateCode(ns, ctd, cc);
            if (localGenerationType == GenFileType.Application) {
                if (cc == null) {
                    ctd.Members.Add(cc = new CodeConstructor());
                    cc.Attributes = 0;
                }
                cc.Statements.Add(new CodeCommentStatement("statements for " + this.localGenerationType));
            } else if (localGenerationType == GenFileType.View) {
                ctd.Members.Add(new CodeSnippetTypeMember("#warning in " + Logger.makeSig(MethodBase.GetCurrentMethod()) + " here."));
            } else if (localGenerationType == GenFileType.NavigationWindow) {
                ctd.Members.Add(new CodeSnippetTypeMember("#warning in " + Logger.makeSig(MethodBase.GetCurrentMethod()) + " here."));
            } else
                Logger.log(MethodBase.GetCurrentMethod(), "Type=" + this.localGenerationType);
        }

        protected override void localGenerateModelCode(CodeNamespace ns, CodeTypeDeclaration ctd) {
            base.localGenerateModelCode(ns, ctd);
            if (localGenerationType == GenFileType.View) {
                ns.Imports.Add(new CodeNamespaceImport("System.Reflection"));
                ns.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
            } else if (localGenerationType == GenFileType.NavigationWindow) {
                ns.Imports.Add(new CodeNamespaceImport("System.Reflection"));
                ns.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
                ctd.Members.Add(new CodeSnippetTypeMember("#warning in " + Logger.makeSig(MethodBase.GetCurrentMethod()) + " here."));
            } else
                Logger.log(MethodBase.GetCurrentMethod(), "Type=" + this.localGenerationType);
        }

        protected override bool shouldGenerateViewmodel {
            get {
                if (localGenerationType == GenFileType.Application) return false;
                if (localGenerationType == GenFileType.View) return true;
                if (localGenerationType == GenFileType.NavigationWindow) return true;
                Logger.log(MethodBase.GetCurrentMethod(), " for type:" + this.localGenerationType);
                return base.shouldGenerateViewmodel;
            }
        }

        protected override void writeElementAttributes(XmlWriter xw) {
            base.writeElementAttributes(xw);
            if (localGenerationType == GenFileType.Application)
                xw.WriteAttributeString("generationType", "app");
            else if (localGenerationType == GenFileType.View) {
                xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, blah(localFileName, 1));
                xw.WriteAttributeString("Class", XamlFileGenerator.NS_X,
                    (string.IsNullOrEmpty(this.localNamespace) ?
                        this.localFileName :
                        (this.localNamespace + "." + this.localFileName)));
                xw.WriteAttributeString("Width", "300");
                xw.WriteAttributeString("Heighth", "300");
            } else if (localGenerationType == GenFileType.NavigationWindow) {
                xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, blah(this.localFileName, 1));
                xw.WriteAttributeString("Class", XamlFileGenerator.NS_X,
                    (string.IsNullOrEmpty(this.localNamespace) ?
                        this.localFileName :
                        (this.localNamespace + "." + this.localFileName)));
            } else
                Logger.log(MethodBase.GetCurrentMethod(), "Type=" + this.localGenerationType);
        }

        protected override void writeElement(XmlWriter xw) {
            base.writeElement(xw);
            if (this.localGenerationType == GenFileType.Application) {
                xw.WriteStartElement("Application.Resources");
                xw.WriteWhitespace("\r\n\t");
                xw.WriteFullEndElement();
            } else if (this.localGenerationType == GenFileType.View) {
                xw.WriteStartElement("Page.Resources", XamlFileGenerator.NS_DEFAULT);
                xw.WriteFullEndElement();
            } else if (this.localGenerationType == GenFileType.NavigationWindow) {
                xw.WriteComment(" " + Logger.makeSig(MethodBase.GetCurrentMethod()) + " ");
            } else {
                Logger.log(MethodBase.GetCurrentMethod(), "Type=" + this.localGenerationType);
            }
        }

        #endregion abstract implementation

        static string blah(string file, int v) {
            return file.Substring(0, 1).ToLower() +
                file.Substring(1) + v;
        }
    }
}