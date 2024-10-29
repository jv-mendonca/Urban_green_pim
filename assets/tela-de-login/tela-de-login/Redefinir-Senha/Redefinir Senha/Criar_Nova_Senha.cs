using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using tela_de_login;
using tela_de_logins;

namespace Redefinir_Senha
{
    public partial class Criar_Nova_Senha : Form
    {
        private string codigoVerificacao; // Código de verificação
        private string email; // E-mail do usuário
        private string ddi; // DDI do telefone
        private string ddd; // DDD do telefone
        private string numeroTelefone; // Número de telefone
        private DateTime tempoGeracaoCodigo; // Tempo da geração do código
        private BancoDeDados bancoDeDados; // Referência ao banco de dados

        public Criar_Nova_Senha(string codigoRecuperacao, string email, string ddi, string ddd, string numeroTelefone)
{
        InitializeComponent();
        codigoVerificacao = codigoRecuperacao; // Inicializa o código de verificação
        this.email = email; // Armazena o e-mail
        this.ddi = ddi; // Atribui DDI recebido
        this.ddd = ddd; // Atribui DDD recebido
        this.AutoScaleMode = AutoScaleMode.Dpi; // Ajusta para o DPI do sistema
        this.numeroTelefone = numeroTelefone; // Atribui número de telefone recebido
        tempoGeracaoCodigo = DateTime.Now; // Inicializa o tempo de geração
        bancoDeDados = new BancoDeDados("fazenda_urbana_Urban_Green_pim4"); // Inicializa o banco de dados
        this.AutoScaleMode = AutoScaleMode.Dpi; // Ajusta para o DPI do sistema
        }


        private bool CodigoValido(string codigoDigitado)
        {
            // Verifica se o código digitado é válido e se não expirou
            return codigoDigitado == codigoVerificacao && (DateTime.Now - tempoGeracaoCodigo).TotalSeconds < 60; // 1 minuto
        }

        private bool ValidarSenha(string senha)
        {
            // Valida se a senha atende aos critérios
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
            return Regex.IsMatch(senha, pattern);
        }

        private bool SenhaDiferente(string novaSenha)
        {
            // Verifica se a nova senha é diferente da senha atual no banco de dados
            string senhaAtual = "";
            string obterSenhaQuery = "SELECT senha FROM usuario WHERE email = @Email";

            try
            {
                using (SqlConnection con = bancoDeDados.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand(obterSenhaQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        var result = cmd.ExecuteScalar();
                        senhaAtual = result != null ? (string)result : null; // Atribui null se não houver resultado
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Erro ao acessar o banco de dados: " + sqlEx.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Verifica se a nova senha é diferente da senha atual
            return novaSenha != senhaAtual;
        }

        private bool AtualizarSenha(string novaSenha)
        {
            // Atualiza a senha no banco de dados
            string atualizarSenhaQuery = "UPDATE usuario SET senha = @NovaSenha WHERE email = @Email";

            try
            {
                using (SqlConnection con = bancoDeDados.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand(atualizarSenhaQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@NovaSenha", novaSenha);
                        cmd.Parameters.AddWithValue("@Email", email);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0; // Retorna verdadeiro se a senha foi atualizada
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Erro ao acessar o banco de dados: " + sqlEx.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void EnviarWhatsApp(string numeroTelefone, string mensagem)
        {
            // Cria o link para enviar a mensagem via WhatsApp
            string url = $"https://api.whatsapp.com/send?phone={numeroTelefone}&text={Uri.EscapeDataString(mensagem)}";

            try
            {
                // Usa ProcessStartInfo para garantir que o link será aberto no navegador padrão
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Garante que o link será aberto no navegador padrão
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir o WhatsApp: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GerarNovoCodigo()
        {
            // Gera um novo código de verificação
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Gera um código de 6 dígitos
        }

        private void LimparCamposSenha()
        {
            // Limpa os campos de entrada de senha
            input_codacess.Clear();
            input_novaSenha.Clear();
            input_senhaConfirm.Clear();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Gerar novo código de verificação
            codigoVerificacao = GerarNovoCodigo(); // Gera um novo código
            tempoGeracaoCodigo = DateTime.Now; // Atualiza o tempo de geração do novo código

            // Enviar o novo código pelo WhatsApp
            EnviarWhatsApp($"{ddi}{ddd}{numeroTelefone}", "Seu novo código de verificação é: " + codigoVerificacao);
            MessageBox.Show("Um novo código de verificação foi enviado para o seu WhatsApp.", "Novo Código", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button_enviar_Click_1(object sender, EventArgs e)
        {
            string codigoDigitado = input_codacess.Text.Trim();
            string novaSenha = input_novaSenha.Text.Trim();
            string confirmarSenha = input_senhaConfirm.Text.Trim();

            if (!CodigoValido(codigoDigitado))
            {
                MessageBox.Show("Código de verificação inválido ou expirado. Gere um novo código.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LimparCamposSenha();
                return;
            }

            if (novaSenha != confirmarSenha)
            {
                MessageBox.Show("As senhas não coincidem. Tente novamente.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidarSenha(novaSenha))
            {
                MessageBox.Show("A senha deve ter pelo menos 8 caracteres, incluindo uma letra maiúscula, uma letra minúscula, um número e um caractere especial.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LimparCamposSenha();
                return;
            }

            if (!SenhaDiferente(novaSenha))
            {
                MessageBox.Show("A nova senha não pode ser a mesma que a senha atual.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (AtualizarSenha(novaSenha))
            {
                MessageBox.Show("Senha atualizada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide(); // Oculta a tela atual
                Form1 telaLogin = new Form1(); // Instancia a tela de login
                telaLogin.Show(); // Mostra a tela de login
            }
        }
    }
}
