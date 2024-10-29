using System;
using System.Windows.Forms;

namespace tela_de_login
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Substitua 'Form1' pelo nome do seu formulário principal
        }
    }
}
