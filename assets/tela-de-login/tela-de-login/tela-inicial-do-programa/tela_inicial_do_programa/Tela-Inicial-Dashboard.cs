using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace tela_inicial_do_programa
{
    public partial class Tela_Inicial_Dashboard : Form
    {
        private string nomeBanco = "fazenda_urbana_Urban_Green_pim4";
        private string connectionString;
        private List<Monitoramento> monitoramentos = new List<Monitoramento>();
        private int currentIndex = 0;

        public Tela_Inicial_Dashboard()
        {
            connectionString = $"Server=mendonça\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";
            InitializeComponent();
            CarregarPlantacao();
            CalcularMediaAguaPorHora(); // Chamar o método aqui
            ExibirMonitoramento(currentIndex); // Exibir o monitoramento inicial
        }

        private void CalcularMediaAguaPorHora()
        {
            string query = @"
            SELECT p.cod_plantacao, 
                   SUM(c.quantidade_agua) AS TotalAguaGasta, 
                   SUM(DATEDIFF(MINUTE, c.hora_inicial, c.hora_final)) AS TotalMinutos
            FROM Plantacao p
            JOIN controle_Agua c ON p.cod_plantacao = c.cod_plantacao
            GROUP BY p.cod_plantacao";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int codPlantacao = reader.GetInt32(0);
                            decimal totalAguaGasta = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                            int totalMinutos = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                            double totalHoras = (double)totalMinutos / 60.0;
                            double mediaAguaPorHora = totalHoras > 0 ? (double)(totalAguaGasta / (decimal)totalHoras) : 0; // Evita divisão por zero

                            // Atualiza o monitoramento correspondente
                            var monitoramento = monitoramentos.Find(m => m.CodPlantacao == codPlantacao);
                            if (monitoramento != null)
                            {
                                monitoramento.MediaAguaPorHora = mediaAguaPorHora; // Armazena a média
                            }
                        }
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

        private void CarregarPlantacao()
        {
            string query = @"
            SELECT p.cod_plantacao, p.especie, p.tipo_plantacao, p.data_plantio, p.data_prevista, p.saude_plantacao,
                   SUM(c.quantidade_agua) AS TotalAguaGasta, 
                   SUM(DATEDIFF(MINUTE, c.hora_inicial, c.hora_final)) AS TotalMinutos
            FROM Plantacao p
            LEFT JOIN controle_Agua c ON p.cod_plantacao = c.cod_plantacao
            GROUP BY p.cod_plantacao, p.especie, p.tipo_plantacao, p.data_plantio, p.data_prevista, p.saude_plantacao";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal totalAguaGasta = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                            int totalMinutos = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                            double totalHoras = totalMinutos / 60.0;

                            // Cálculo da porcentagem de água gasta
                            double porcentagemAguaGasta = totalHoras > 0 ? ((double)totalAguaGasta / totalHoras) * 100 : 0;

                            Monitoramento monitoramento = new Monitoramento
                            {
                                CodPlantacao = Convert.ToInt32(reader["cod_plantacao"]),
                                Especie = reader["especie"].ToString(),
                                TipoPlantacao = reader["tipo_plantacao"].ToString(),
                                DataPlantio = Convert.ToDateTime(reader["data_plantio"]),
                                DataPrevista = Convert.ToDateTime(reader["data_prevista"]),
                                Saude = reader["saude_plantacao"].ToString(),
                                PorcentagemAguaGasta = porcentagemAguaGasta, // Armazena a porcentagem calculada
                            };
                            monitoramentos.Add(monitoramento);
                        }
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

        private void ExibirMonitoramento(int index)
        {
            if (index >= 0 && index < monitoramentos.Count)
            {
                var monitoramento = monitoramentos[index];
                txtEspecie.Text = monitoramento.Especie;
                txtTipo.Text = monitoramento.TipoPlantacao;
                txtDataDePLantio.Text = FormatarData(monitoramento.DataPlantio);
                txtPrevisao.Text = FormatarData(monitoramento.DataPrevista);
                AtualizarSaude(monitoramento.Saude);

                // Exibe a média de água por hora sem formatação adicional
                txtAgua.Text = monitoramento.MediaAguaPorHora.ToString("F2") + "%"; // Mostra o valor diretamente

                CentralizarControleNaCaixa(txtEspecie, caixaEspecie);
                CentralizarControleNaCaixa(txtTipo, caixaTipo);
                CentralizarControleNaCaixa(txtDataDePLantio, caixaPlantado);
                CentralizarControleNaCaixa(txtPrevisao, caixaPrevisao);
                PosicionarSaudeNaParteInferior();
            }
        }

        private string FormatarData(DateTime data)
        {
            string[] diasDaSemana = { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sab" };
            string diaAbreviado = diasDaSemana[(int)data.DayOfWeek];
            return $"{diaAbreviado}, {data:dd} {data:MMM}";
        }

        private void AtualizarSaude(string saude)
        {
            txtSaude.Text = saude;
            string caminhoImagens = @"C:\testePim\urban_Green\assets\tela-de-login\tela-de-login\tela-inicial-do-programa\img\";

            switch (saude)
            {
                case "Saudável":
                    txtSaude.ForeColor = Color.Green;
                    imagemFolha.Image = Image.FromFile($"{caminhoImagens}folha_verde.png");
                    break;
                case "Cuidado":
                    txtSaude.ForeColor = Color.Yellow;
                    imagemFolha.Image = Image.FromFile($"{caminhoImagens}folha_amarela.png");
                    break;
                case "Perigo":
                    txtSaude.ForeColor = Color.Red;
                    imagemFolha.Image = Image.FromFile($"{caminhoImagens}folha_vermelha.png");
                    break;
                default:
                    txtSaude.ForeColor = Color.Black;
                    imagemFolha.Image = null;
                    break;
            }

            CentralizarImagemNaCaixa(); // Certifique-se de que isso é chamado
            PosicionarSaudeNaParteInferior(); // Reposicione o texto também
        }

        private void Próximo_Click(object sender, EventArgs e)
        {
            if (currentIndex < monitoramentos.Count - 1)
            {
                currentIndex++;
                ExibirMonitoramento(currentIndex);
            }
            else
            {
                currentIndex = 0;
                ExibirMonitoramento(currentIndex);
            }
        }

        private void Anterior_Click(object sender, EventArgs e)
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                ExibirMonitoramento(currentIndex);
            }
            else
            {
                currentIndex = monitoramentos.Count - 1;
                ExibirMonitoramento(currentIndex);
            }
        }

        private void CentralizarControleNaCaixa(Control controle, Control caixa)
        {
            controle.Location = new Point(
                (caixa.Width - controle.Width) / 2,
                (caixa.Height - controle.Height) / 2
            );
        }

        private void PosicionarSaudeNaParteInferior()
        {
            int verticalOffset = 10;
            txtSaude.Location = new Point(
                (caixaSaude.Width - txtSaude.Width) / 2,
                caixaSaude.Height - txtSaude.Height - verticalOffset
            );
        }

        private void CentralizarImagemNaCaixa()
        {
            if (imagemFolha.Image != null)
            {
                imagemFolha.SizeMode = PictureBoxSizeMode.Zoom;
                imagemFolha.Location = new Point(
                    (caixaSaude.Width - imagemFolha.Width) / 2,
                    (caixaSaude.Height - imagemFolha.Height) / 2
                );
            }
        }

        public class Monitoramento
        {
            public int CodPlantacao { get; set; }
            public string Especie { get; set; }
            public string TipoPlantacao { get; set; }
            public DateTime DataPlantio { get; set; }
            public DateTime DataPrevista { get; set; }
            public string Saude { get; set; }
            public double PorcentagemAguaGasta { get; set; }
            public double MediaAguaPorHora { get; set; } // Armazena a média
        }
    }
}