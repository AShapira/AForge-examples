using System.Windows;
using System.Windows.Controls;

namespace AForge.Wpf
{
    /// <summary>
    /// Interaction logic for Presenter.xaml
    /// </summary>
    public partial class Presenter : Window
    {
        public Presenter()
        {
            InitializeComponent();
        }

        private void TextInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //TextInput.ScrollToEnd();
        }
    }
}
