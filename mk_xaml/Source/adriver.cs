using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace NSMk_xaml {
    class Program {

        [STAThread()]
        public static void Main(string[] args) {
            int exitCode = 0;
            MKXOptions opts = new MKXOptions();

            opts.useCompileUnit = true;
            opts.createProvider();
            //            XamlFileGenerator.showFileContent = true;
            try {
                // XamlFileGenerator.generateFile(MyNewObj.shared.createType(GenFileType.Application), opts);
                // XamlFileGenerator.generateFile(MyNewObj.shared.createType(GenFileType.Model), opts);
                XamlFileGenerator.generateFile(MyNewObj.shared.createType(GenFileType.NavigationWindow), opts);
                // XamlFileGenerator.generateFile(MyNewObj.shared.createType(GenFileType.View), opts);
            } catch (Exception ex) {
                Console.Error.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                exitCode = 1;
            }
            Environment.Exit(exitCode);
        }
    }

    class MyNewObj {
        internal static readonly MyNewObj shared = new MyNewObj();

        public MyNewObj(string clsName) {
            className = clsName;
        }

        MyNewObj() { }

        internal IXamlFileGenerationData createType(GenFileType gft) {
            return new Tester(gft);
        }

        public string className { get; private set; }
    }
}