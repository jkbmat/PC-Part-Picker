using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Projekt.Validation
{
    public static class TextBoxValidation
    {
        /// <summary>
        /// Povoli zadavat do TextBoxu iba cisla
        /// </summary>
        public static void NumericalTextBoxCheck(object sender, TextCompositionEventArgs e)
        {
            double n;
            string text = ((TextBox)sender).Text + e.Text;

            if (text != "" && !Double.TryParse(text, out n))
            {
                e.Handled = true;
                System.Media.SystemSounds.Asterisk.Play();
            }
        }
    }
}
