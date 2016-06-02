namespace NSTmp {
    using System.ComponentModel;
    
    public partial class MainWindowViewModel : INotifyPropertyChanged {
        public MainWindowViewModel() {
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void firePropertyChanged(string v) {
            if ((this.PropertyChanged != )) {
                this.PropertyChanged(this, v);
            }
        }
        public void firePropertyChanged(MethodBase mb) {
            int n;

            if (((n = mb.Name.Length) 
                        > 4)) {
                // here
            }
        }
    }
}
