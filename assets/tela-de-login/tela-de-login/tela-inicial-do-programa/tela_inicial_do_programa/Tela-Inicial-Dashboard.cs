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
            CalcularMediaAguaPorHora(); // Chama o método para calcular a média de água
            CalcularMediaLuzPorHora(); // Chama o método para calcular a média de luz
            CarregarTemperatura(); // Chama o método para carregar a temperatura
            ExibirMonitoramento(currentIndex); // Exibir o monitoramento inicial
            this.AutoScaleMode = AutoScaleMode.Dpi; // Ajusta para o DPI do sistema
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

            ExecutarConsulta(query, reader =>
            {
                int codPlantacao = reader.GetInt32(0);
                decimal totalAguaGasta = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                int totalMinutos = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                double totalHoras = (double)totalMinutos / 60.0;

                var monitoramento = monitoramentos.Find(m => m.CodPlantacao == codPlantacao);
                if (monitoramento != null)
                {
                    monitoramento.MediaAguaPorHora = totalHoras > 0 ? (double)(totalAguaGasta / (decimal)totalHoras) : 0;
                }
            });
        }

        private void CalcularMediaLuzPorHora()
        {
            string query = @"
    SELECT p.cod_plantacao, 
           SUM(DATEDIFF(HOUR, c.hora_inicial, c.hora_final)) AS TotalHoras,
           SUM(DATEDIFF(MINUTE, c.hora_inicial, c.hora_final)) AS TotalMinutos,
           COUNT(*) AS TotalRegistros
    FROM Plantacao p
    JOIN controle_Luz c ON p.cod_plantacao = c.cod_plantacao
    GROUP BY p.cod_plantacao";

            ExecutarConsulta(query, reader =>
            {
                int codPlantacao = reader.GetInt32(0);
                int totalHoras = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                int totalMinutos = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                int totalRegistros = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                // Cálculo da média de luz gasta por hora
                double mediaLuzPorHora = totalRegistros > 0 ? (double)totalMinutos / totalRegistros : 0;

                // Ajuste para que 6000.00% se torne 60.00%
                double intensidadeLuz = totalHoras > 0 ? (mediaLuzPorHora / totalHoras) * 100 / 100 : 0;

                var monitoramento = monitoramentos.Find(m => m.CodPlantacao == codPlantacao);
                if (monitoramento != null)
                {
                    monitoramento.MediaLuzPorHora = intensidadeLuz; // Armazena a intensidade
                }
            });
        }

        private void CarregarTemperatura()
        {
            string query = @"
    SELECT TOP 1 valor_temperatura 
    FROM controle_temperatura 
    ORDER BY data_registro DESC";

            ExecutarConsulta(query, reader =>
            {
                if (reader.Read())
                {
                    decimal valorTemperatura = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    txtTemperatura.Text = $"{valorTemperatura:F2} °C"; // Exibe a temperatura no controle
                }
            });
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

            ExecutarConsulta(query, reader =>
            {
                decimal totalAguaGasta = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                int totalMinutos = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                double totalHoras = totalMinutos / 60.0;

                double porcentagemAguaGasta = totalHoras > 0 ? ((double)totalAguaGasta / totalHoras) * 100 : 0;

                Monitoramento monitoramento = new Monitoramento
                {
                    CodPlantacao = Convert.ToInt32(reader["cod_plantacao"]),
                    Especie = reader["especie"].ToString(),
                    TipoPlantacao = reader["tipo_plantacao"].ToString(),
                    DataPlantio = Convert.ToDateTime(reader["data_plantio"]),
                    DataPrevista = Convert.ToDateTime(reader["data_prevista"]),
                    Saude = reader["saude_plantacao"].ToString(),
                    PorcentagemAguaGasta = porcentagemAguaGasta,
                };
                monitoramentos.Add(monitoramento);
            });
        }

        private void ExecutarConsulta(string query, Action<SqlDataReader> action)
        {
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
                            action(reader);
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

                txtAgua.Text = $"{monitoramento.MediaAguaPorHora:F2}%"; // Exibe a média de água
                txtLuz.Text = $"{monitoramento.MediaLuzPorHora:F2}%"; // Exibe a média de luz
                txtTemperatura.Text = $"{monitoramento.Temperatura:F2} °C"; // Exibe a temperatura

                CentralizarControleNaCaixa(txtEspecie, caixaEspecie);
                CentralizarControleNaCaixa(txtTipo, caixaTipo);
                CentralizarControleNaCaixa(txtDataDePLantio, caixaPlantado);
                CentralizarControleNaCaixa(txtPrevisao, caixaPrevisao);
                CentralizarControleNaCaixa(txtLuz, caixaLuz);
                CentralizarControleNaCaixa(txtAgua, caixaAgua);
                CentralizarControleNaCaixa(txtTemperatura, caixaTemperatura); // Certifique-se de ter uma caixa para a temperatura
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
                    imagemFolha.Image = Image.FromFile($"{caminhoImagens}folha_amarelaPIM.png");
                    break;
                case "Perigo":
                    txtSaude.ForeColor = Color.Red;
                    imagemFolha.Image = Image.FromFile($"{caminhoImagens}folha_vermelha.png");
                    break;
                default:
                    txtSaude.ForeColor = Color.Black;
                    break;



            }
            CentralizarImagemNaCaixa(); // Adicionando a centralização da imagem
            PosicionarSaudeNaParteInferior();
        }

        private void CentralizarControleNaCaixa(Control controle, Control caixa)
        {
            controle.Left = (caixa.Width - controle.Width) / 2;
            controle.Top = (caixa.Height - controle.Height) / 2;
        }

        private void CentralizarImagemNaCaixa()
        {
            if (imagemFolha.Image != null)
            {
                imagemFolha.SizeMode = PictureBoxSizeMode.Zoom; // Ajusta o modo de exibição da imagem
                imagemFolha.Location = new Point(
                    (caixaSaude.Width - imagemFolha.Width) / 2,
                    (caixaSaude.Height - imagemFolha.Height) / 2
                );
            }
        }

        private void PosicionarSaudeNaParteInferior()
        {
            int verticalOffset = 10;
            txtSaude.Location = new Point(
                (caixaSaude.Width - txtSaude.Width) / 2,
                caixaSaude.Height - txtSaude.Height - verticalOffset
            );
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
        public double MediaAguaPorHora { get; set; }
        public double MediaLuzPorHora { get; set; }
        public decimal Temperatura { get; set; } // Adicione esta linha
    }
}
