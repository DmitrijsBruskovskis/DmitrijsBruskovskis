using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Midis.EyeOfHorus.ClientLibrary;

namespace Midis.EyeOfHorus.Client2
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        public bool login = true;

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (login)
            {
                this.Hide();
                Settings fm = new Settings();
                fm.Show();
            }
        }
    }
}
