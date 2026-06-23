using System.Windows;
using ShoeManager.Core;

namespace ShoeManager.Windows
{
    public partial class ReportesWindow : Window
    {
        private readonly BaseMaestra _baseMaestra;

        public ReportesWindow(BaseMaestra baseMaestra)
        {
            InitializeComponent();
            _baseMaestra = baseMaestra;
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}