/*
 * 
 * msMessenger DID selection winform GUI and methods
 * Copyright (C) 2016  Clinton Jarvis
 * WWW: jarvis.im   Email: clinton@jarvis.im
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace msMessenger
{
    public partial class DIDSelector : MaterialForm
    {
        // define MaterialSkin object
        private readonly MaterialSkinManager materialSkinManager;

        public DIDSelector()
        {
            InitializeComponent();

            // Initialize MaterialSkinManager
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Red800, Primary.Red900, Primary.Red500, Accent.Red200, TextShade.WHITE);
        }

        private void DIDSelector_Load(object sender, EventArgs e)
        {
            //TestItems
            for (int i = 1; i <= 5; i++)
            {

                MaterialRadioButton radio = new MaterialRadioButton();
                radio.Text = "40" + i + "7445447";
                radio.Name = "did_" + i;
                radio.AutoSize = false;
                radio.Height = 30;
                radio.Width = flowLayoutPanel1.Width - 10;
                
                flowLayoutPanel1.Controls.Add(radio);


            }
            this.Height = 165 + (5 * 30);
        }

        private void materialFlatButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            var checkedButton = flowLayoutPanel1.Controls.OfType<MaterialRadioButton>().FirstOrDefault(r => r.Checked);
            MessageBox.Show("Selected " + checkedButton.Text + "!","Selected!",MessageBoxButtons.OK);
            this.Close();
        }
    }
}
