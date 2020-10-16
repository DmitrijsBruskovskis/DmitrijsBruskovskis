using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Midis.EyeOfHorus.Client
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        public bool login = true;
        private void button1_Click(object sender, EventArgs e)
        {
            if (login)
            {
                this.Hide();
                new Settings().Show();
            }
        }
    }
}
