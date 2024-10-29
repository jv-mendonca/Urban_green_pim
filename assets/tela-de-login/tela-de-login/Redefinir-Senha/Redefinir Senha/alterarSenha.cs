using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using tela_de_logins;


namespace Redefinir_Senha
{
    public partial class alterarSenha : Form
    {
        private BancoDeDados bancoDeDados;
       

        

        public alterarSenha()
        {
            InitializeComponent();
            this.FormClosed += AlterarSenha_FormClosed;
            bancoDeDados = new BancoDeDados("fazenda_urbana_Urban_Green_pim4");
            bancoDeDados.CriarBancoDeDadosSeNaoExistir();
            bancoDeDados.CriarTabelasSeNaoExistirem();
        }

        private void AlterarSenha_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private string GerarCodigoAleatorio()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Gera um código de 6 dígitos
        }

        private void EnviarCodigoPorWhatsApp(string email, string ddi, string ddd, string numeroTelefone, string codigoRecuperacao)
        {
            // Verifica se o e-mail existe
            string usuarioQuery = "SELECT COUNT(*) FROM usuario WHERE email = @Email";

            try
            {
                using (SqlConnection con = bancoDeDados.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand(usuarioQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        int count = (int)cmd.ExecuteScalar();

                        if (count > 0)
                        {
                            string numeroCompleto = $"{ddi}{ddd}{numeroTelefone}"; // Concatena DDI, DDD e número
                            EnviarWhatsApp(numeroCompleto, codigoRecuperacao);
                            MessageBox.Show("Código de redefinição enviado para o seu WhatsApp.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            AbrirTelaRecuperacao(codigoRecuperacao, email, ddi, ddd, numeroTelefone);
                        }
                        else
                        {
                            MessageBox.Show("E-mail não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Erro ao acessar o banco de dados: " + sqlEx.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnviarWhatsApp(string numeroTelefone, string codigoRecuperacao)
        {
            string mensagem = $"Seu código de recuperação é: {codigoRecuperacao}";
            string url = $"https://api.whatsapp.com/send?phone={numeroTelefone}&text={Uri.EscapeDataString(mensagem)}";

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir o WhatsApp: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirTelaRecuperacao(string codigoRecuperacao, string email, string ddi, string ddd, string numeroTelefone)
        {
            Criar_Nova_Senha telaAlterarSenha = new Criar_Nova_Senha(codigoRecuperacao, email, ddi, ddd, numeroTelefone);
            this.Hide();
            telaAlterarSenha.Show();
        }


        private void button_Gcod_Click_1(object sender, EventArgs e)
        {
            string email = input_email.Text.Trim();
            string ddi = inputDdi.Text.Trim(); // Campo para DDI
            string ddd = input_ddd.Text.Trim(); // Campo para DDD
            string numeroTelefone = input_whastapp.Text.Trim(); // Campo para número
            string codigoRecuperacao = GerarCodigoAleatorio();

            // Envie o código via WhatsApp
            EnviarCodigoPorWhatsApp(email, ddi, ddd, numeroTelefone, codigoRecuperacao);
        }
    }
}
