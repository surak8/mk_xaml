namespace NSTmp {
    
    public partial class MainWindow : Window {
        MainWindowViewModel _vm;
        public MainWindow() {
            this._vm = new MainWindowViewModel();
            this.InitializeComponent();
        }
    }
}
