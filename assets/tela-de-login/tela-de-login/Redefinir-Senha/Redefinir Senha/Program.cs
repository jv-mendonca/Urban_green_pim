using System;
using System.Windows.Forms;
using tela_de_login;

namespace Redefinir_Senha
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Substitua "Form1" pelo seu formulário principal
        }
    }
}
