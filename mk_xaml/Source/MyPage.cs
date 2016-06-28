using System;

namespace NSMk_xaml {
    class MyPage : BaseXamlFileGeneration {
        protected override string localElementName { get { return "Page"; } }
        protected override string localFileName { get { throw new NotImplementedException(); } }
        protected override GenFileType localGenerationType { get { return GenFileType.View; } }
    }
}