using System.CodeDom.Compiler;

namespace NSMk_xaml {
    public interface ICodeDomGenerationUtil {
        CodeDomProvider provider { get; }
        CodeGeneratorOptions options { get; }
        void createProvider();
    }
}