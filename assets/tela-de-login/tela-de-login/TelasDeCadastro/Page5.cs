using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tela_de_login;

namespace TelasDeCadastro
{
    public partial class Page5 : Form
    {
        public Page5()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {


            // Cria uma nova instância do formulário de login
            Form1 loginForm = new Form1();

            // Exibe o formulário de login
            loginForm.Show();

            // Oculta o formulário atual
            this.Hide();
        }

        
    }
}
