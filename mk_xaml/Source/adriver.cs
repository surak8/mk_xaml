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

            //            opts.useCompileUnit = true;
            //            XamlFileGenerator.showFileContent = true;
            opts.createProvider();
            try {
                // XamlFileGenerator.generateFile(MyNewObj.shared.createType(GenFileType.Application), opts);
                // XamlFileGenerator.generateFile(MyNewObj.shared.createType(GenFileType.Model), opts);
                XamlFileGenerator.generateFile(new Tester(GenFileType.NavigationWindow, "NSTester"), opts);
                // XamlFileGenerator.generateFile(MyNewObj.shared.createType(GenFileType.View), opts);
            } catch (Exception ex) {
                Console.Error.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                exitCode = 1;
            }
            Environment.Exit(exitCode);
        }
    }
}