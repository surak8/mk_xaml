using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.CSharp;

// -f WpfApplication1 -tx -xn -g -n WpfApplication1 -xf Page0 -xf Page1 -xf Page2
// -f SnertPopulator -n NSSnertPop -g -tx -xn -xf AddRange -xf DeleteRange -xf ModifyRange -xf Allocate -xf CreateJob -xf MaintainData 

namespace NSMk_xaml {
    /// <summary></summary>
    public static class XamlFileGenerator {
        #region constants
        /// <summary></summary>
        public const string NS_X = "http://schemas.microsoft.com/winfx/2006/xaml";
        /// <summary></summary>
        public const string NS_DEFAULT = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        /// <summary></summary>
        public const string NS_BLEND = "http://schemas.microsoft.com/expression/blend/2008";
        #endregion

        #region fields
        static XmlWriterSettings _xws;
        static readonly CodeExpression ceThis = new CodeThisReferenceExpression();
        static readonly CodeExpression ceNull = new CodePrimitiveExpression();
        static readonly CodeStatement csBlank = new CodeSnippetStatement();
        static readonly CodeExpression ceZero = new CodePrimitiveExpression(0);
        static readonly CodeExpression ceThree = new CodePrimitiveExpression(3);
        static readonly CodeExpression ceTrue = new CodePrimitiveExpression(true);
        /// <summary>description of showFileContent.</summary>
        public static bool showFileContent = false;
        #endregion

        #region properties
        static XmlWriterSettings settings {
            get {
                if (_xws == null) {
                    _xws = new XmlWriterSettings();
                    _xws.Indent = true;
                    _xws.IndentChars = "\t";
                    _xws.OmitXmlDeclaration = true;
                    _xws.NewLineOnAttributes = true;
                    _xws.NewLineHandling = NewLineHandling.None;
                }
                return _xws;
            }
        }
        #endregion

        /// <summary>do it.</summary>
        /// <param name="ixfgd"></param>
        /// <param name="opts"></param>
        internal static void generateFile(IXamlFileGenerationData ixfgd, ICodeDomGenerationUtil opts) {
            StringBuilder sb;
            string fname, ename, ns, modelName, ext;

            if (ixfgd == null)
                throw new ArgumentNullException("ixfgd", "generation-object is null!");
            if (string.IsNullOrEmpty(ename = ixfgd.elementName))
                throw new ArgumentNullException("ename", "element-name is null!");
            if (string.IsNullOrEmpty(fname = ixfgd.fileName))
                if (string.IsNullOrEmpty(fname))
                    throw new ArgumentNullException("fname", "file-name is null!");
            if (ixfgd.generationType == GenFileType.View||ixfgd.generationType== GenFileType.NavigationWindow)
                ixfgd.xamlName = Path.Combine("Source\\Views\\", fname + ".xaml");
            else
                ixfgd.xamlName = fname + ".xaml";
            ns = ixfgd.nameSpace;
            sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb)) {
                using (XmlWriter xw = XmlWriter.Create(sw, settings)) {
                    xw.WriteStartDocument();
                    generationInfo(xw);
                    xw.WriteStartElement(ename, NS_DEFAULT);
                    xw.WriteAttributeString("xmlns", "x", null, NS_X);
                    xw.WriteAttributeString("xmlns", "d", null, NS_BLEND);
                    if (!string.IsNullOrEmpty(ns))
                        xw.WriteAttributeString("xmlns", "local", null, "clr-namespace:" + ns);
                    ixfgd.populateElementAttributes(xw);
                    ixfgd.populateElement(xw);
                    xw.WriteEndDocument();
                }
            }
            logFileGeneratedFile(ixfgd.xamlName, sb);
            ext = opts.provider.FileExtension;
            modelName = fname + "ViewModel";
            if (ixfgd.generationType == GenFileType.View||ixfgd.generationType==  GenFileType.NavigationWindow) {
                ixfgd.codeBehindName = Path.Combine("Source\\Views", fname + ".xaml." + ext);
                ixfgd.viewModelName = Path.Combine("Source\\Models\\", modelName + "." + ext);
            } else {
                ixfgd.codeBehindName = fname + ".xaml." + ext;
                ixfgd.viewModelName = modelName + "." + ext;
            }
            createMainFile(ixfgd.codeBehindName, ns, fname, ename, modelName, ixfgd.generateViewModel, ixfgd, opts);
            if (ixfgd.generateViewModel)
                createModelfile(ixfgd.viewModelName, ns, modelName, ixfgd, opts);
        }

        static void generationInfo(XmlWriter xw) {
            xw.WriteComment(" "+infoString() + " ");
        }

        static string infoString() {
            return makeInfoString(Assembly.GetEntryAssembly(), DateTime.Now, Environment.UserName, Environment.MachineName);
        }

        static string makeInfoString(Assembly a, DateTime dtNow, string user, string machine) {
            return "generated by " + a.GetName().Name + " on " + dtNow.ToString("dd-MMM-yy hh:mm:ss") + " by " + user + " on machine " + machine;
        }

        static void createDirIfNeeded(string fname) {
            string tmp;

            if (!string.IsNullOrEmpty(tmp = Path.GetDirectoryName(fname)) &&
                !Directory.Exists(tmp))
                Directory.CreateDirectory(tmp);
        }

        static void createModelfile(string outModelName, string nameSpace, string modelName, IXamlFileGenerationData ixfgd, ICodeDomGenerationUtil opts) {
            CodeCompileUnit ccu = null;
            CodeNamespace ns0, ns;
            CodeTypeDeclaration ctd;
            CodeConstructor cc;
            CodeMemberEvent cme0;
            CodeMemberMethod cmm0, cmm3;
            CodeEventReferenceExpression cere;
            bool useCCU = true;

            if (opts is MKXOptions)
                useCCU = ((MKXOptions)opts).useCompileUnit;
            if (useCCU) {
                ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(ns = ns0 = new CodeNamespace());
                if (!string.IsNullOrEmpty(nameSpace))
                    ccu.Namespaces.Add(ns = new CodeNamespace(nameSpace));
                ns0.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
                ns0.Imports.Add(new CodeNamespaceImport("System.Reflection"));
            } else
                ns = new CodeNamespace(nameSpace);
            ns.Comments.Add(new CodeCommentStatement(infoString()));
            ns.Types.Add(ctd = new CodeTypeDeclaration(modelName));

            if (opts.provider.Supports(GeneratorSupport.PartialTypes))
                ctd.IsPartial = true;

            const string EVENT_NAME = "PropertyChanged";
            const string METHOD_NAME = "firePropertyChanged";
            CodeArgumentReferenceExpression care0, care1;
            ctd.BaseTypes.Add("INotifyPropertyChanged");
            cere = new CodeEventReferenceExpression(ceThis, EVENT_NAME);
            ctd.Members.AddRange(new CodeTypeMember[] {
                cme0=createEvent(cere),
                cc=new CodeConstructor(),
                cmm0=createMethod1(METHOD_NAME,cere,care0=new CodeArgumentReferenceExpression ("propertyName")),
                cmm3=createMethod2(METHOD_NAME,cere,care1=new CodeArgumentReferenceExpression ("mb"))
            });
            cc.Comments.Add(new CodeCommentStatement("<summary>ctor.</summary>", true));
            cmm0.Comments.AddRange(new CodeCommentStatement[] {
                new CodeCommentStatement("<summary>Raise notification.</summary>", true),
                new CodeCommentStatement("<param name=\""+care0.ParameterName+"\"/>", true),
                new CodeCommentStatement("<seealso cref=\""+EVENT_NAME+"\"/>", true),
            });
            cmm3.Comments.AddRange(new CodeCommentStatement[] {
                new CodeCommentStatement("<summary>Raise notification.</summary>", true),
                new CodeCommentStatement("<param name=\""+care1.ParameterName+"\"/>", true),
                new CodeCommentStatement("<seealso cref=\""+EVENT_NAME+"\"/>", true),
            });
            cc.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "ctor"));
            cc.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "ctor"));
            cme0.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "events"));
            cme0.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "events"));

            cmm0.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "methods"));
            cmm3.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "methods"));

            cc.Attributes = MemberAttributes.Public;
            ixfgd.generateModelCode(ns, ctd);
            outputFile(ccu, ns, outModelName, opts);
        }

        static void createMainFile(string outCSName, string nameSpace, string fname, string ename, string modelName, bool generateViewModel, IXamlFileGenerationData ixfgd, ICodeDomGenerationUtil opts) {
            CodeCompileUnit ccu = null;
            CodeNamespace ns0, ns;
            CodeTypeDeclaration ctd;
            CodeConstructor cc = null;
            CodeMemberField f;
            CodeFieldReferenceExpression fr = null;
            bool useCCU = true;


            if (opts is MKXOptions)
                useCCU = ((MKXOptions)opts).useCompileUnit;

            if (useCCU) {
                ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(ns = ns0 = new CodeNamespace());
                if (!string.IsNullOrEmpty(nameSpace))
                    ccu.Namespaces.Add(ns = new CodeNamespace(nameSpace));
            } else {
                ns0 = ns = new CodeNamespace(string.IsNullOrEmpty(nameSpace) ? string.Empty : nameSpace);
                //                ns.Imports .Add()
            }
            ns.Comments.Add(new CodeCommentStatement(infoString()));
            ixfgd.addImports(ns0);
            ns.Types.Add(ctd = new CodeTypeDeclaration(fname));
            if (opts.provider.Supports(GeneratorSupport.PartialTypes))
                ctd.IsPartial = true;
            ctd.BaseTypes.Add(ename);

            if (generateViewModel) {
                fr = new CodeFieldReferenceExpression(null, "_vm");
                ctd.Members.Add(f = new CodeMemberField(modelName, fr.FieldName));
                f.Attributes = 0;
                ctd.Members.Add(cc = new CodeConstructor());
                cc.Attributes = MemberAttributes.Public;
                cc.Statements.Add(
                    new CodeAssignStatement(
                        new CodePropertyReferenceExpression(ceThis, "DataContext"),
                        new CodeBinaryOperatorExpression(fr, CodeBinaryOperatorType.Assign, new CodeObjectCreateExpression(modelName))));
                cc.Statements.Add(
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(null, "InitializeComponent", new CodeExpression[0])));
            }
            ixfgd.generateCode(ns, ctd, cc);
            outputFile(ccu, ns, outCSName, opts);
        }

        static void outputFile(CodeCompileUnit ccu, CodeNamespace ns, string outModelName, ICodeDomGenerationUtil opts) {
            StringBuilder sb;

            using (TextWriter sw = new StringWriter(sb = new StringBuilder())) {
                if (ccu != null)
                    opts.provider.GenerateCodeFromCompileUnit(ccu, sw, opts.options);
                else
                    opts.provider.GenerateCodeFromNamespace(ns, sw, opts.options);
            }
            logFileGeneratedFile(outModelName, sb);
            sb.Clear();
            sb = null;
        }

        static void logFileGeneratedFile(string fileName, StringBuilder sb) {
            createDirIfNeeded(fileName);
            File.WriteAllText(fileName, sb.ToString());
            if (showFileContent)
                Debug.Print(Environment.NewLine + fileName + Environment.NewLine + sb.ToString());
            Console.Out.WriteLine("wrote: " + fileName);
            Logger.log("wrote: " + fileName);
        }

        #region code-generation methods
        static CodeMemberEvent createEvent(CodeEventReferenceExpression cere) {
            CodeMemberEvent cme = new CodeMemberEvent();

            cme.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            cme.Type = new CodeTypeReference("PropertyChangedEventHandler");
            cme.Name = cere.EventName;
            return cme;
        }

        static CodeMemberMethod createMethod1(string methodName, CodeEventReferenceExpression cere, CodeArgumentReferenceExpression ar) {
            CodeMemberMethod ret = new CodeMemberMethod();
            /*
            * void firePropertyChanged(string v) {
            *      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
            * }
            * */

            ret.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            ret.Name = methodName;
            ret.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), ar.ParameterName));
            ret.Statements.Add(new CodeConditionStatement(
                new CodeBinaryOperatorExpression(cere, CodeBinaryOperatorType.IdentityInequality, ceNull),
                new CodeExpressionStatement(
                    new CodeDelegateInvokeExpression(cere, ceThis,
                        new CodeObjectCreateExpression("PropertyChangedEventArgs", ar)))));
            return ret;
        }

        static CodeMemberMethod createMethod2(string methodName, CodeEventReferenceExpression cere, CodeArgumentReferenceExpression ar2) {
            CodeMemberMethod ret = new CodeMemberMethod();
            CodeExpression ce4 = new CodePrimitiveExpression(4);
            CodePropertyReferenceExpression mbName = new CodePropertyReferenceExpression(ar2, "Name");
            CodeExpression mie0 = makeSubstr(mbName, ceZero, ceThree);
            CodeMethodReferenceExpression mr1 = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(string)), "Compare");
            CodeExpression ceStrComp2 = new CodeMethodInvokeExpression(mr1, mie0, new CodePrimitiveExpression("set"), ceTrue);
            CodeExpression ceStrComp1 = new CodeMethodInvokeExpression(mr1, mie0, new CodePrimitiveExpression("get"), ceTrue);
            CodePropertyReferenceExpression mbNameLen = new CodePropertyReferenceExpression(mbName, "Length");
            CodeVariableReferenceExpression vr = new CodeVariableReferenceExpression("n");

            /*
             * void firePropertyChanged(MethodBase mb) {
             *      int n;
             *      if ((n = mb.Name.Length) > 4) {
             *          if (string.Compare(mb.Name.Substring(0, 3), "set", true) == 0 ||
             *              string.Compare(mb.Name.Substring(0, 3), "get", true) == 0)
             *              firePropertyChanged(mb.Name.Substring(4));
             *      }
             * }
             * */
            ret.Parameters.Add(new CodeParameterDeclarationExpression("MethodBase", ar2.ParameterName));
            ret.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            ret.Name = methodName;
            ret.Statements.AddRange(new CodeStatement[] {
                new CodeVariableDeclarationStatement (typeof(int),vr.VariableName),
                csBlank,
                new CodeConditionStatement (
                    new CodeBinaryOperatorExpression (
                        new CodeBinaryOperatorExpression(vr,CodeBinaryOperatorType.Assign ,mbNameLen),CodeBinaryOperatorType.GreaterThan ,ce4),
                            new CodeConditionStatement(new CodeBinaryOperatorExpression(eq(ceStrComp1, ceZero) ,  CodeBinaryOperatorType.BooleanOr ,eq(ceStrComp2,ceZero)),
                                new CodeExpressionStatement(new CodeMethodInvokeExpression (null,ret.Name,makeSubstr (mbName,ce4))))),
            });
            return ret;
        }

        static CodeExpression eq(CodeExpression ceLeft, CodeExpression ceRight) {
            return new CodeBinaryOperatorExpression(ceLeft, CodeBinaryOperatorType.IdentityEquality, ceRight);
        }

        static CodeExpression ne(CodeExpression ceLeft, CodeExpression ceRight) {
            return new CodeBinaryOperatorExpression(ceLeft, CodeBinaryOperatorType.IdentityInequality, ceRight);
        }

        static CodeExpression binOp(CodeExpression ceLeft, CodeBinaryOperatorType type, CodeExpression ceRight) {
            return new CodeBinaryOperatorExpression(ceLeft, type, ceRight);
        }

        static CodeExpression makeSubstr(CodeExpression ceTarget, CodeExpression ceLen) {
            return new CodeMethodInvokeExpression(ceTarget, "Substring", ceLen);
        }

        static CodeExpression makeSubstr(CodeExpression ceTarget, CodeExpression ceStart, CodeExpression ceLen) {
            return new CodeMethodInvokeExpression(ceTarget, "Substring", ceStart, ceLen);
        }

#endregion

    }
}