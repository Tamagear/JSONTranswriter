using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChromaJSONEditor.Dialogs
{
    public partial class NewCardDialog : Window
    {
        public NewCardResponse response;

        public NewCardDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            response = new NewCardResponse();
            response.color = Color_Dropdown.SelectedValue.ToString().ToLower().Split(' ')[1];
            if (int.TryParse(Amount_Field.Text, out int res))
            {
                response.amount = res;
                response.ok = true;
                Close();
            }
            else
                MessageBox.Show("Amount was not a valid a number!", "Warning! ParseError!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public class NewCardResponse
    {
        public string color;
        public int amount = 0;
        public bool ok = false;
    }
}
