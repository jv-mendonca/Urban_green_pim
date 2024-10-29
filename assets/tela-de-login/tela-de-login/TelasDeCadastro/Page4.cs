using System;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using tela_de_logins;

namespace TelasDeCadastro
{
    public partial class Page4 : Form
    {
        private BancoDeDados bancoDeDados; // Instância da classe BancoDeDados
        private int codFuncionario;
        private bool fecharProgramaticamente = false; // Controle do fechamento programático

        public Page4(int codFuncionario)
        {
            InitializeComponent();
            this.codFuncionario = codFuncionario;

            // Inicializa a classe BancoDeDados com o nome do banco
            bancoDeDados = new BancoDeDados("fazenda_urbana_Urban_Green_pim4");

            // Armazena a referência da Page1
            this.FormClosing += Page4_FormClosing; // Adiciona evento de fechamento do formulário
        }

        private bool VerificaEmail(string email)
        {
            // Regex para verificar se o e-mail é de um provedor permitido (Gmail, Hotmail, Yahoo)
            string pattern = @"^[^@\s]+@(gmail\.com|hotmail\.com|yahoo\.com)$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        private bool VerificaSenha(string senha)
        {
            // Verifica se a senha tem pelo menos 8 caracteres, uma letra maiúscula, uma letra minúscula e um caractere especial
            return Regex.IsMatch(senha, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{8,}$");
        }

        private void InserirUsuario(string email, string senha, int codFuncionario, int codUsuario)
        {
            string query = "INSERT INTO usuario (email, senha, cod_funcionario, cod_usuario, status_conta) VALUES (@Email, @Senha, @CodFuncionario, @CodUsuario, @StatusConta)";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Senha", senha);
                    cmd.Parameters.AddWithValue("@CodFuncionario", codFuncionario);
                    cmd.Parameters.AddWithValue("@CodUsuario", codUsuario);
                    cmd.Parameters.AddWithValue("@StatusConta", "ativo"); // Sempre define status_conta como 'ativo'
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int GerarCodUsuario()
        {
            Random random = new Random();
            int codUsuario;

            do
            {
                // Gera um número aleatório entre 1000 e 9999 (ou qualquer intervalo que você desejar)
                codUsuario = random.Next(1000, 10000);
            } while (CodUsuarioJaCadastrado(codUsuario));

            return codUsuario;
        }

        private bool CodUsuarioJaCadastrado(int codUsuario)
        {
            string query = "SELECT COUNT(*) FROM usuario WHERE cod_usuario = @CodUsuario";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CodUsuario", codUsuario);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0; // Retorna true se o código já estiver cadastrado
                }
            }
        }

        private void Page4_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!fecharProgramaticamente)
            {
                // Exibe a mensagem de confirmação somente se for manual
                var result = MessageBox.Show("Você realmente deseja sair? Os dados do funcionário não serão salvos e serão excluídos se houver.",
                                              "Confirmação",
                                              MessageBoxButtons.YesNo,
                                              MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true; // Cancela o fechamento do formulário se o usuário não confirmar
                }
            }
        }

        private void btn_confirmar_Click_1(object sender, EventArgs e)
        {
            string email = input_email.Text.Trim();
            string senha = input_senha.Text.Trim();
            string confirmarSenha = input_confirmarSenha.Text.Trim();

            // Verifica se o e-mail é válido
            if (!VerificaEmail(email))
            {
                MessageBox.Show("Por favor, insira um e-mail válido (Gmail, Hotmail, Yahoo).", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Verifica se as senhas coincidem
            if (senha != confirmarSenha)
            {
                MessageBox.Show("As senhas não coincidem.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Verifica se a senha é válida
            if (!VerificaSenha(senha))
            {
                MessageBox.Show("A senha deve ter pelo menos 8 caracteres, incluir uma letra maiúscula, uma letra minúscula e um caractere especial.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Gera um novo cod_usuario
            int codUsuario = GerarCodUsuario();

            // Tenta cadastrar o usuário
            try
            {
                InserirUsuario(email, senha, codFuncionario, codUsuario); // Usa o CodFuncionario existente
                MessageBox.Show("Usuário cadastrado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao cadastrar usuário: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Abrir Page5 após sucesso
            Page5 page5 = new Page5();
            page5.Show();  // Abre a Page5
            fecharProgramaticamente = true; // Evita a confirmação ao fechar
            this.Close();  // Fecha a Page4
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Chama o método para apagar os dados de telefone
            ApagarDadosTelefone();

            // Define fechar programaticamente para evitar a mensagem
            fecharProgramaticamente = true;

            // Abre a Page3
            Page3 page3 = new Page3(codFuncionario);
            page3.Show();

            // Fecha a Page4 sem confirmação
            this.Close();
        }

        private void ApagarDadosTelefone()
        {
            string querySelecionarCodTelefones = "SELECT cod_telefone FROM funcionario_telefone WHERE cod_funcionario = @CodFuncionario";
            string queryExcluirFuncionarioTelefone = "DELETE FROM funcionario_telefone WHERE cod_funcionario = @CodFuncionario";
            string queryExcluirTelefone = "DELETE FROM telefone WHERE cod_telefone = @CodTelefone";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                // Lista para armazenar os cod_telefone
                List<int> codTelefones = new List<int>();

                // Primeiro, selecionamos os códigos de telefone associados ao funcionário
                using (SqlCommand cmd = new SqlCommand(querySelecionarCodTelefones, con))
                {
                    cmd.Parameters.AddWithValue("@CodFuncionario", codFuncionario);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Adiciona todos os cod_telefone à lista
                        while (reader.Read())
                        {
                            codTelefones.Add((int)reader["cod_telefone"]);
                        }
                    }
                }

                // Primeiro, excluímos os registros da tabela funcionario_telefone
                using (SqlCommand cmdExcluirFuncionarioTelefone = new SqlCommand(queryExcluirFuncionarioTelefone, con))
                {
                    cmdExcluirFuncionarioTelefone.Parameters.AddWithValue("@CodFuncionario", codFuncionario);
                    cmdExcluirFuncionarioTelefone.ExecuteNonQuery();
                }

                // Depois, para cada cod_telefone, excluímos o registro na tabela telefone
                foreach (int codTelefone in codTelefones)
                {
                    using (SqlCommand cmdExcluirTelefone = new SqlCommand(queryExcluirTelefone, con))
                    {
                        cmdExcluirTelefone.Parameters.AddWithValue("@CodTelefone", codTelefone);
                        cmdExcluirTelefone.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
