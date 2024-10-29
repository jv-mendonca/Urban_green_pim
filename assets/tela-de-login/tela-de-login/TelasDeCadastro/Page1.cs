using Guna.UI2.WinForms;
using System;
using Microsoft.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using tela_de_logins;
using tela_de_login;

namespace TelasDeCadastro
{
    public partial class Page1 : Form
    {
        private int codFuncionario; // Variável para armazenar o código do funcionário
        private BancoDeDados bancoDeDados; // Instância da classe BancoDeDados
        private bool deveSalvarDados; // Controle para saber se os dados devem ser salvos

        public Page1()
        {
            InitializeComponent();
            CentralizarPanel(); // Centraliza o painel ao inicializar
            this.Resize += Page1_Resize; // Adiciona o evento de redimensionamento
            this.FormClosing += Page1_FormClosing; // Adiciona o evento de fechamento do formulário
            this.AutoScaleMode = AutoScaleMode.Dpi; // Ajusta para o DPI do sistema
            // Inicializa a classe BancoDeDados com o nome do banco
            bancoDeDados = new BancoDeDados("fazenda_urbana_Urban_Green_pim4");

            // Configura o MaskedTextBox para a data de nascimento
            dataNascimentoUsuario.Mask = "00/00/0000"; // Máscara para data
            dataNascimentoUsuario.ValidatingType = typeof(DateTime); // Tipo de validação
        }

        public void LimparDados()
        {
            nomeCompleto.Text = string.Empty;
            cpfUsuario.Text = string.Empty;
            digitoCpf.Text = string.Empty;
            rgUsuario.Text = string.Empty;
            rgDigito.Text = string.Empty;
            dataNascimentoUsuario.Text = string.Empty;
        }

        private void CentralizarPanel()
        {
            // Centraliza o painel na janela
            panel1.Location = new System.Drawing.Point(
                (this.ClientSize.Width - panel1.Width) / 2,
                (this.ClientSize.Height - panel1.Height) / 2
            );
        }

        private void Page1_Resize(object sender, EventArgs e)
        {
            CentralizarPanel(); // Re-centraliza o painel ao redimensionar a janela
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            // Obter os valores dos campos de entrada
            string nomeCompleto = this.nomeCompleto.Text.Trim();
            // Dividir o nome completo em partes
            var partesNome = nomeCompleto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Obter o primeiro e o último nome
            string primeiroNome = partesNome.Length > 0 ? partesNome[0] : string.Empty; // Primeiro nome
            string ultimoNome = partesNome.Length > 1 ? partesNome[^1] : string.Empty; // Último nome

            string cpf = $"{cpfUsuario.Text.Trim()}{digitoCpf.Text.Trim()}"; // Concatenar CPF e dígito
            string rg = $"{rgUsuario.Text.Trim()}{rgDigito.Text.Trim()}"; // Concatenar RG e dígito

            // Validação do comprimento do RG
            if (rg.Length > 9) // Ajuste o limite conforme o tamanho da coluna no banco de dados
            {
                MessageBox.Show("O RG deve ter no máximo 9 caracteres.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validação do CPF
            if (!ValidarCPF(cpf))
            {
                MessageBox.Show("CPF inválido! Por favor, insira um CPF válido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validação do RG
            if (!ValidarRG(rg))
            {
                MessageBox.Show("RG inválido! Por favor, insira um RG válido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Coleta a data de nascimento do TextBox
            if (!DateTime.TryParse(dataNascimentoUsuario.Text.Trim(), out DateTime dataNascimento))
            {
                MessageBox.Show("Data de nascimento inválida! Por favor, insira uma data no formato correto (dd/MM/yyyy).", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validação da data
            if (!ValidarDataNascimento(dataNascimento))
            {
                MessageBox.Show("Data de nascimento inválida! Por favor, insira uma data válida.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Gerar o código do funcionário
            codFuncionario = GerarCodigoFuncionario();

            // Inserir dados no banco de dados
            try
            {
                InserirFuncionario(codFuncionario, rg, cpf, dataNascimento, primeiroNome, ultimoNome);
                deveSalvarDados = true; // Define que os dados foram salvos
                //MessageBox.Show("Funcionário cadastrado com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao cadastrar funcionário: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Criar uma instância da Page3, passando os dados necessários e a referência da Page1
            Page3 page3 = new Page3(codFuncionario);
            
            page3.Show(); // Mostra a Page3
            this.Hide(); // Oculta a Page1
        }

        private bool ValidarCPF(string cpf)
        {
            // Remover caracteres não numéricos
            cpf = cpf.Replace(".", "").Replace("-", "").Trim();

            // Verifica se o CPF possui 11 dígitos e se é numérico
            if (cpf.Length != 11 || !long.TryParse(cpf, out _))
                return false;

            // Cálculo do primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (int)(cpf[i] - '0') * (10 - i);

            int digito1 = 11 - (soma % 11);
            if (digito1 >= 10) digito1 = 0;

            // Cálculo do segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (int)(cpf[i] - '0') * (11 - i);

            int digito2 = 11 - (soma % 11);
            if (digito2 >= 10) digito2 = 0;

            // Verifica se os dígitos verificadores estão corretos
            return cpf[9] == (char)(digito1 + '0') && cpf[10] == (char)(digito2 + '0');
        }

        private bool ValidarRG(string rg)
        {
            // Remover caracteres não numéricos
            rg = rg.Replace(".", "").Replace("-", "").Replace("/", "").Trim();

            // Verifica se o RG é numérico e possui 9 dígitos
            if (rg.Length != 9 || !long.TryParse(rg, out _))
                return false;

            string rgBase = rg.Substring(0, 8); // Os 8 primeiros dígitos
            char digitoVerificador = rg[8]; // O 9º dígito (dígito verificador)

            // Cálculo do dígito verificador
            int soma = 0;
            int peso = 2;

            for (int i = rgBase.Length - 1; i >= 0; i--)
            {
                soma += (rgBase[i] - '0') * peso; // Converte o caractere em número
                peso++;
                if (peso > 9) peso = 2; // Reinicia o peso após 9
            }

            int resultado = soma % 11;
            char digitoCalculado = (resultado < 10) ? (char)(resultado + '0') : 'X'; // 'X' se o resultado for 10

            // Verifica se o dígito verificador calculado é igual ao fornecido
            return digitoCalculado == digitoVerificador;
        }

        private bool ValidarDataNascimento(DateTime dataNascimento)
        {
            // Define uma data mínima (por exemplo, 1900) e uma data máxima (por exemplo, hoje)
            DateTime dataMinima = new DateTime(1900, 1, 1);
            DateTime dataMaxima = DateTime.Today;

            // Valida se a data está dentro do intervalo permitido
            return dataNascimento >= dataMinima && dataNascimento <= dataMaxima;
        }

        private int GerarCodigoFuncionario()
        {
            Random random = new Random();
            return random.Next(10000, 99999); // Exemplo de geração de código aleatório
        }

        private void InserirFuncionario(int codFuncionario, string rg, string cpf, DateTime dataNascimento, string primeiroNome, string ultimoNome)
        {
            // Define o código do cargo como 2 (Funcionário Comum)
            int codCargo = 2; // Cargo padrão

            // Atualiza a consulta para incluir os nomes
            string query = "INSERT INTO funcionario (cod_funcionario, cod_cargo, rg, cpf, data_nascimento, primeiro_nome, ultimo_nome) VALUES (@CodFuncionario, @CodCargo, @Rg, @Cpf, @DataNascimento, @primeiroNome, @ultimoNome)";

            using (SqlConnection con = bancoDeDados.Conectar()) // Conectar() já abre a conexão
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CodFuncionario", codFuncionario);
                    cmd.Parameters.AddWithValue("@CodCargo", codCargo); // Usando o cargo padrão
                    cmd.Parameters.AddWithValue("@Rg", rg);
                    cmd.Parameters.AddWithValue("@Cpf", cpf);
                    cmd.Parameters.AddWithValue("@DataNascimento", dataNascimento);
                    cmd.Parameters.AddWithValue("@primeiroNome", primeiroNome);
                    cmd.Parameters.AddWithValue("@ultimoNome", ultimoNome);
                    cmd.ExecuteNonQuery(); // Executa a consulta
                }
            }
        }

        private void Page1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!deveSalvarDados) // Se não houver necessidade de salvar dados, exibe a mensagem
            {
                DialogResult resultado = MessageBox.Show("Tem certeza que deseja sair? Todos os dados não salvos serão perdidos.", "Confirmação", MessageBoxButtons.YesNo);
                e.Cancel = (resultado == DialogResult.No); // Cancela o fechamento se o usuário não confirmar
            }
        }

        private void botao_voltar_Click(object sender, EventArgs e)
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
