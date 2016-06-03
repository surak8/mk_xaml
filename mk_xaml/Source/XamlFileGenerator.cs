using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.CSharp;

namespace NSMk_xaml {
    #region delegates
    delegate void XamlGenerationHandler(object sender, out string elementName, out string fileName, out string nameSpace);
    delegate void XamlElementAttributeHandler(object sender, XmlWriter xw);
    delegate void XamlContentHandler(object sender, XmlWriter xw);
    #endregion

    static class XamlFileGenerator {
        #region constants
        public const string NS_X = "http://schemas.microsoft.com/winfx/2006/xaml";
        public const string NS_DEFAULT = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        public const string NS_BLEND = "http://schemas.microsoft.com/expression/blend/2008";
        #endregion

        #region fields
        static XmlWriterSettings _xws;
        static readonly CodeExpression ceThis = new CodeThisReferenceExpression();
        static readonly CodeExpression ceNull = new CodeSnippetExpression();
        #endregion

        static readonly CodeStatement csBlank = new CodeSnippetStatement();

        #region properties
        static XmlWriterSettings settings {
            get {
                if (_xws == null) {
                    _xws = new XmlWriterSettings();
                    _xws.Indent = true;
                    _xws.IndentChars = "\t";
                    _xws.OmitXmlDeclaration = true;
                    _xws.NewLineOnAttributes = true;
                }
                return _xws;
            }
        }
        #endregion

        internal static void generateFile(XamlGenerationHandler xgh, XamlElementAttributeHandler xeah, XamlContentHandler xch, out string outXamlName, out string outCSName, out string outModelName) {
            StringBuilder sb;
            string fname, ename, ns, modelName;
            CodeDomProvider cdp = new CSharpCodeProvider();

            outXamlName = outCSName = outModelName = null;

            if (xgh != null) {
                xgh(null, out ename, out fname, out ns);
                if (string.IsNullOrEmpty(ename))
                    throw new ArgumentNullException("ename", "element-name is null!");
                if (string.IsNullOrEmpty(fname))
                    throw new ArgumentNullException("fname", "file-name is null!");
                outXamlName = fname + ".xaml";
                outCSName = fname + ".xaml." + cdp.FileExtension;
                modelName = fname + "ViewModel";
                outModelName = modelName + "." + cdp.FileExtension;
                sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb)) {
                    using (XmlWriter xw = XmlWriter.Create(sw, settings)) {
                        //	xw.WriteStartElement(ename);
                        /*
						 * <Window x:Name="window1" x:Class="WpfApplication1.MainWindow"
			xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
			xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			xmlns:d="http://schemas.microsoft.com/expression/blend/2008"										
			xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
			xmlns:local="clr-namespace:WpfApplication1"
			mc:Ignorable="d"
			Title="MainWindow" Height="350" Width="525">
						 * */
                        xw.WriteStartElement(ename, NS_DEFAULT);
                        xw.WriteAttributeString("xmlns", "x", null, NS_X);
                        //						xw.WriteAttributeString("Name", NS_X, "zzz");
                        xw.WriteAttributeString("xmlns", "d", null, NS_BLEND);
                        if (!string.IsNullOrEmpty(ns))
                            xw.WriteAttributeString("xmlns", "local", null, "clr-namespace:" + ns);
                        if (xeah != null)
                            xeah(null, xw);
                        if (xch != null)
                            xch(null, xw);
                        xw.WriteEndDocument();
                    }
                }
                Debug.Print(sb.ToString());
                File.WriteAllText(outXamlName, sb.ToString());

                CodeGeneratorOptions opts;

                opts = new CodeGeneratorOptions();
                opts.BlankLinesBetweenMembers = false;
                opts.ElseOnClosing = true;

                createMainFile(outCSName, ns, fname, ename, cdp, modelName, opts);
                createModelfile(outModelName, ns, modelName, cdp, opts);
            }
        }

        static void createModelfile(string outModelName, string nameSpace, string modelName, CodeDomProvider cdp, CodeGeneratorOptions opts) {
            CodeCompileUnit ccu = null;
            CodeNamespace ns = new CodeNamespace(nameSpace);
            CodeTypeDeclaration ctd;
            CodeConstructor cc;
            CodeMemberEvent cme;
            CodeMemberMethod cmm, cmm2;
            CodeEventReferenceExpression cere;

            ns.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
            ns.Types.Add(ctd = new CodeTypeDeclaration(modelName));
            if (cdp.Supports(GeneratorSupport.PartialTypes))
                ctd.IsPartial = true;

            ctd.BaseTypes.Add("INotifyPropertyChanged");
            cere = new CodeEventReferenceExpression(ceThis, "PropertyChanged");
            ctd.Members.AddRange(new CodeTypeMember[] {
                cme=new CodeMemberEvent(),
                cc=new CodeConstructor(),
                cmm=new CodeMemberMethod(),
                cmm2=new CodeMemberMethod(),
            }); ;
            cme.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            cme.Type = new CodeTypeReference("PropertyChangedEventHandler");
            cme.Name = cere.EventName;
            cmm2.Attributes = cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            cmm2.Name = cmm.Name = "firePropertyChanged";
            cc.Attributes = MemberAttributes.Public;

            CodeArgumentReferenceExpression ar = new CodeArgumentReferenceExpression("v");
            cmm.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), ar.ParameterName));
            cmm.Statements.Add(new CodeConditionStatement(
                new CodeBinaryOperatorExpression(cere, CodeBinaryOperatorType.IdentityInequality, ceNull),
                new CodeExpressionStatement(new CodeDelegateInvokeExpression(cere, ceThis, ar))));

            CodeArgumentReferenceExpression ar2 = new CodeArgumentReferenceExpression("mb");
            CodeVariableReferenceExpression vr = new CodeVariableReferenceExpression("n");

            cmm2.Parameters.Add(new CodeParameterDeclarationExpression("MethodBase", ar2.ParameterName));

            CodeExpression ceMBName = new CodePropertyReferenceExpression(ar2, "Name");
            CodeMethodReferenceExpression mrsub = new CodeMethodReferenceExpression(ceMBName, "Substring");
            CodeMethodInvokeExpression cmieSub = new CodeMethodInvokeExpression(
                mrsub, new CodePrimitiveExpression(0), new CodePrimitiveExpression(4));
            cmm2.Statements.AddRange(new CodeStatement[] {
                new CodeVariableDeclarationStatement (typeof(int),vr.VariableName),
                csBlank                    ,
                new CodeConditionStatement (
                    new CodeBinaryOperatorExpression (
                        new CodeBinaryOperatorExpression(vr,  CodeBinaryOperatorType.Assign ,
                        new CodePropertyReferenceExpression (
                        new CodePropertyReferenceExpression(ar2,"Name"),"Length")),
                        CodeBinaryOperatorType.GreaterThan ,
                        new CodePrimitiveExpression(4)),
                    new CodeCommentStatement("here"))
					
					
//					)
//										  )))
			});
            outputFile(ccu, ns, cdp, outModelName, opts);
        }

        static void createMainFile(string outCSName, string nameSpace, string fname, string ename, CodeDomProvider cdp, string modelName, CodeGeneratorOptions opts) {
            CodeCompileUnit ccu = null;
            CodeNamespace ns = new CodeNamespace(nameSpace);
            CodeTypeDeclaration ctd;
            CodeConstructor cc;
            CodeMemberField f;

            ns.Types.Add(ctd = new CodeTypeDeclaration(fname));
            if (cdp.Supports(GeneratorSupport.PartialTypes))
                ctd.IsPartial = true;
            ctd.BaseTypes.Add(ename);

            CodeFieldReferenceExpression fr = new CodeFieldReferenceExpression(ceThis, "_vm");
            ctd.Members.AddRange(
                new CodeTypeMember[] {
                    f=new CodeMemberField(modelName,fr.FieldName),
                    cc=new CodeConstructor(),
                });
            f.Attributes = 0;
            cc.Attributes = MemberAttributes.Public;
            cc.Statements.AddRange(
                new CodeStatement[] {
                    new CodeAssignStatement(fr, new CodeObjectCreateExpression(modelName)),
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression (ceThis,"InitializeComponent",new CodeExpression[0]))
                });
            outputFile(ccu, ns, cdp, outCSName, opts);
        }

        static void outputFile(CodeCompileUnit ccu, CodeNamespace ns, CodeDomProvider cdp, string outModelName, CodeGeneratorOptions opts) {
            StringBuilder sb;

            using (TextWriter sw = new StringWriter(sb = new StringBuilder())) {
                if (ccu != null)
                    cdp.GenerateCodeFromCompileUnit(ccu, sw, opts);
                else
                    cdp.GenerateCodeFromNamespace(ns, sw, opts);
            }
            File.WriteAllText(outModelName, sb.ToString());
            Debug.Print(sb.ToString());
            sb.Clear();
            sb = null;
        }
    }
}