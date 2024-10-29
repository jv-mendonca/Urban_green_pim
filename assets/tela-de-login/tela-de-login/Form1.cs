using Microsoft.Data.SqlClient;
using Redefinir_Senha;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using tela_de_logins; // Importando o namespace onde está a classe BancoDeDados
using TelasDeCadastro;

namespace tela_de_login
{
    public partial class Form1 : Form
    {
        private string nomeBanco = "fazenda_urbana_Urban_Green_pim4"; // Nome do banco de dados

        public Form1()
        {
            InitializeComponent();
            ConfigurarFormulario();
            CriarBancoDeDados(); // Criar banco de dados e tabelas ao inicializar o formulário
        }

        private void CriarBancoDeDados()
        {
            // Instanciando a classe BancoDeDados
            BancoDeDados bancoDeDados = new BancoDeDados(nomeBanco);

            // Verifica se o banco de dados já existe
            if (!bancoDeDados.VerificaBancoDeDadosExistente())
            {
                try
                {
                    bancoDeDados.CriarBancoDeDadosSeNaoExistir(); // Criar o banco de dados se não existir
                    bancoDeDados.CriarTabelasSeNaoExistirem(); // Criar tabelas se não existirem
                    MessageBox.Show("Banco de dados e tabelas criados com sucesso!"); // Mensagem de sucesso
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao criar banco de dados ou tabelas: " + ex.Message);
                }
            }
            else
            {
                // Oculta a tela de criação do banco e mostra a tela de login
                this.Hide(); // Oculta a tela atual (Form1)
                             // Assumindo que você está no mesmo Form1 que é a tela de login
                this.Show(); // Mostra novamente a tela de login (ou apenas a mantenha visível)
            }
        }



        private void ConfigurarFormulario()
        {
            input_senha.UseSystemPasswordChar = true; // Esconder caracteres da senha
            this.FormBorderStyle = FormBorderStyle.None; // Remover bordas
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20)); // Bordas arredondadas
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse
        );

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            panel2.BackColor = Color.FromArgb(30, 0, 0, 0); // Fundo translúcido
            int radius = 20; // Raio das bordas arredondadas

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(panel2.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(panel2.Width - radius, panel2.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, panel2.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                panel2.Region = new Region(path);
            }
        }

        private void button_login_Click(object sender, EventArgs e)
        {
            string connectionString = $"Server=mendonça\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";
            string loginQuery = @"
            SELECT u.cod_usuario, c.nome_cargo
            FROM usuario u
            INNER JOIN funcionario f ON u.cod_funcionario = f.cod_funcionario
            INNER JOIN cargo c ON f.cod_cargo = c.cod_cargo
            WHERE u.email COLLATE SQL_Latin1_General_CP1_CS_AS = @Email 
            AND u.senha COLLATE SQL_Latin1_General_CP1_CS_AS = @Senha";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(loginQuery, con);
                cmd.Parameters.AddWithValue("@Email", input_user.Text.Trim());
                cmd.Parameters.AddWithValue("@Senha", input_senha.Text.Trim());

                try
                {
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read()) // Login bem-sucedido
                        {
                            string nomeCargo = dr["nome_cargo"].ToString();

                            if (nomeCargo == "Administrador") // Verifica se o nome do cargo é "Administrador"
                            {
                                MessageBox.Show("Acesso como Administrador aprovado.");
                                this.Hide(); // Ocultar a tela de login
                                Page2 paginaAdministrador = new Page2(); // Abrir a tela de administrador
                                paginaAdministrador.Show();
                            }
                            else
                            {
                                MessageBox.Show($"Bem-vindo, {nomeCargo}.");
                                this.Hide();
                                tela_inicial_do_programa.Tela_Inicial_Dashboard telaInicial = new tela_inicial_do_programa.Tela_Inicial_Dashboard(); // Abrir a tela inicial
                                telaInicial.Show();
                            }
                        }
                        else // Falha no login
                        {
                            MessageBox.Show("Usuário ou senha inválidos", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            input_user.Clear();
                            input_senha.Clear();
                            input_user.Focus();
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show("Erro ao acessar o banco de dados: " + sqlEx.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: " + ex.Message);
                }
            }
        }


        private void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            input_senha.UseSystemPasswordChar = !checkbox.Checked;
        }

        private void link_senha_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Ocultar o Form1
            this.Hide();

            // Criar uma nova instância do formulário de redefinição de senha
            alterarSenha redefinirSenhaForm = new alterarSenha();
            redefinirSenhaForm.FormClosed += (s, args) => this.Show(); // Mostrar o Form1 novamente ao fechar o redefinirSenhaForm
            redefinirSenhaForm.Show(); // Exibir o formulário
        }

        private void AbrirLinkNoNavegador(string url)
        {
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
                MessageBox.Show($"Erro ao abrir o link: {ex.Message}");
            }
        }

        private void linkLabel18_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AbrirLinkNoNavegador("https://dancing-kitten-ab5a83.netlify.app/");
        }

        private void button_cadastro_Click(object sender, EventArgs e)
        {
            this.Hide(); // Oculta a tela de login
            Page1 paginaCadastro = new Page1(); // Cria uma nova instância da tela de cadastro
            paginaCadastro.FormClosed += (s, args) => this.Show(); // Mostra a tela de login novamente quando a tela de cadastro for fechada
            paginaCadastro.Show(); // Exibe a tela de cadastro
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }
    }
}
