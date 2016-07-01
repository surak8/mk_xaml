using System.CodeDom.Compiler;

namespace NSMk_xaml {
    public interface ICodeDomGenerationUtil {
        #region properties
        /// <summary>provides access to the code-generator.</summary>
        CodeDomProvider provider { get; }
        /// <summary>Access to the code-generation options.</summary>
        CodeGeneratorOptions options { get; }
        #endregion
        #region methods
        /// <summary>Create the provider, based upon internal flags.</summary>
        void createProvider();
        #endregion
    }
}