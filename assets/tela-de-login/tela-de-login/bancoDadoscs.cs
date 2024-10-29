using System;
using Microsoft.Data.SqlClient;

namespace tela_de_logins
{
    internal class BancoDeDados
    {
        private string nomeBanco;

        public BancoDeDados(string nomeBanco)
        {
            this.nomeBanco = nomeBanco;
        }

        // Método para criar o banco de dados se não existir
        public void CriarBancoDeDadosSeNaoExistir()
        {
            // Conexão para a master database
            string connectionString = $"Server=mendonça\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            string createDbQuery = $@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{nomeBanco}')
                BEGIN
                    CREATE DATABASE {nomeBanco}
                END";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(createDbQuery, con))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"Banco de dados '{nomeBanco}' criado ou já existe.");
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Erro ao criar o banco de dados: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message);
            }
        }

        // Método para conectar ao banco de dados
        public SqlConnection Conectar()
        {
            string connectionString = $"Server=mendonça\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";

            // Tente abrir a conexão
            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                con.Open(); // Abre a conexão
                return con; // Retorna a conexão aberta
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Erro ao conectar ao banco de dados: {ex.Message}");
                throw; // Lança a exceção novamente para tratamento posterior
            }
        }
        public bool VerificaBancoDeDadosExistente()
        {
            string connectionString = $"Server=mendonça\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            string query = $"SELECT database_id FROM sys.databases WHERE name = @DatabaseName";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DatabaseName", nomeBanco);
                    con.Open();
                    return cmd.ExecuteScalar() != null; // Retorna true se o banco de dados existir
                }
            }
        }


        // Método para criar tabelas se não existirem
        public void CriarTabelasSeNaoExistirem()
        {
            // Verificar se o banco de dados existe antes de tentar criar as tabelas
            //tring de conexão
string connectionString = $"Server=mendonça\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";

            //Verificação da existência do banco de dados
string checkDbQuery = $@"
IF EXISTS (SELECT * FROM sys.databases WHERE name = '{nomeBanco}')
BEGIN
  
    -- Criação das tabelas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'cargo')
BEGIN
    CREATE TABLE cargo(
        cod_cargo INT PRIMARY KEY,
        nome_cargo VARCHAR(50) UNIQUE,
        descricao VARCHAR(100),
        departamento VARCHAR(50)
    );

    -- Inserção de valores padrão na tabela 'cargo'
    INSERT INTO cargo (cod_cargo, nome_cargo, descricao, departamento)
    VALUES 
        (1, 'Administrador', 'Responsável por gerenciar as operações do sistema.', 'Gestão'),
        (2, 'Funcionário Comum', 'Realiza tarefas operacionais na fazenda.', 'Operações'),
        (3, 'Técnico de Cultivo', 'Especialista em práticas de cultivo e monitoramento.', 'Técnico'),
        (4, 'Gerente de Produção', 'Supervisiona a produção e o rendimento.', 'Gestão'),
        (5, 'Assistente Administrativo', 'Auxilia nas atividades administrativas.', 'Gestão'),
        (6, 'Supervisor de Colheita', 'Coordena o processo de colheita.', 'Operações');


    END
    
    -- Criação da tabela 'funcionario'
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'funcionario')
BEGIN
    CREATE TABLE funcionario(
        cod_funcionario INT PRIMARY KEY,
        cod_cargo INT NOT NULL,
        data_nascimento DATE,
        primeiro_nome VARCHAR(30),
        ultimo_nome VARCHAR(30),
       
        cpf VARCHAR(11) UNIQUE NOT NULL, -- CPF sem formatação
        rg VARCHAR(10) UNIQUE NOT NULL,  -- RG sem formatação
        FOREIGN KEY (cod_cargo) REFERENCES cargo(cod_cargo)
    );

    -- Inserção de um funcionário padrão que será o administrador
    INSERT INTO funcionario (cod_funcionario, cod_cargo, data_nascimento, primeiro_nome, ultimo_nome, cpf, rg)
    VALUES 
        (1, 1, '1990-01-01', 'Admin', 'Responsável', '34577999030', '491685452'); -- Exemplo de dados sem formatação
END

   

    
       -- Criação da tabela 'usuario'
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'usuario')
BEGIN
    CREATE TABLE usuario(
        cod_usuario INT PRIMARY KEY,
        cod_funcionario INT NOT NULL,
        email VARCHAR(100) UNIQUE NOT NULL,
        status_conta VARCHAR(30),
        senha VARCHAR(50) NOT NULL,
        FOREIGN KEY (cod_funcionario) REFERENCES funcionario(cod_funcionario)
    );

    -- Inserção de um usuário padrão que será o administrador
    INSERT INTO usuario (cod_usuario, cod_funcionario, email, status_conta, senha)
    VALUES 
        (1, 1, 'admin@email.com', 'ativo', 'senha_admin'); -- Exemplo de dados do administrador
END


    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'telefone')
    BEGIN
        CREATE TABLE telefone (
            cod_telefone INT PRIMARY KEY,
            ddi VARCHAR(2),
            ddd VARCHAR(2),
            numero_telefone VARCHAR(15), -- Modificado para permitir formatação
            CONSTRAINT telefone_unico UNIQUE (ddd, numero_telefone)
        );
    END

    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'funcionario_telefone')
    BEGIN
        CREATE TABLE funcionario_telefone(
            cod_funcionario INT,
            cod_telefone INT,
            PRIMARY KEY (cod_funcionario, cod_telefone),
            FOREIGN KEY (cod_funcionario) REFERENCES funcionario(cod_funcionario),
            FOREIGN KEY (cod_telefone) REFERENCES telefone(cod_telefone)
        );
    END

   
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'estado')
BEGIN
    CREATE TABLE estado (
        cod_estado INT PRIMARY KEY,
        unidade_federativa VARCHAR(2) UNIQUE -- A UF deve ter 2 caracteres
    );

    -- Inserindo os estados do Brasil
    INSERT INTO estado (cod_estado, unidade_federativa) VALUES
    (1, 'AC'), -- Acre
    (2, 'AL'), -- Alagoas
    (3, 'AP'), -- Amapá
    (4, 'AM'), -- Amazonas
    (5, 'BA'), -- Bahia
    (6, 'CE'), -- Ceará
    (7, 'DF'), -- Distrito Federal
    (8, 'ES'), -- Espírito Santo
    (9, 'GO'), -- Goiás
    (10, 'MA'), -- Maranhão
    (11, 'MT'), -- Mato Grosso
    (12, 'MS'), -- Mato Grosso do Sul
    (13, 'MG'), -- Minas Gerais
    (14, 'PA'), -- Pará
    (15, 'PB'), -- Paraíba
    (16, 'PR'), -- Paraná
    (17, 'PE'), -- Pernambuco
    (18, 'PI'), -- Piauí
    (19, 'RJ'), -- Rio de Janeiro
    (20, 'RN'), -- Rio Grande do Norte
    (21, 'RS'), -- Rio Grande do Sul
    (22, 'RO'), -- Rondônia
    (23, 'RR'), -- Roraima
    (24, 'SC'), -- Santa Catarina
    (25, 'SP'), -- São Paulo
    (26, 'SE'), -- Sergipe
    (27, 'TO'); -- Tocantins
END

    

    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'cidade')
    BEGIN
        CREATE TABLE cidade(
            cod_cidade INT PRIMARY KEY,
            nome_cidade VARCHAR(50),
            cod_estado INT,
            FOREIGN KEY (cod_estado) REFERENCES estado(cod_estado)
        );
    END

    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'bairro')
    BEGIN
        CREATE TABLE bairro(
            cod_bairro INT PRIMARY KEY,
            nome_bairro VARCHAR(50),
            cod_cidade INT,
            FOREIGN KEY (cod_cidade) REFERENCES cidade(cod_cidade)
        );
    END

    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'cep')
    BEGIN
        CREATE TABLE cep(
            cod_cep INT PRIMARY KEY,
            cep VARCHAR(8) UNIQUE
        );
    END

    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'endereco')
    BEGIN
        CREATE TABLE endereco(
            cod_endereco INT PRIMARY KEY,  -- Código do endereço
            cod_cidade INT NOT NULL,       -- Código da cidade
            cod_cep INT NOT NULL,          -- Código do CEP
            cod_bairro INT NOT NULL,       -- Código do bairro
            logradouro VARCHAR(100),       -- Logradouro do endereço
            numero VARCHAR(5),             -- Número do endereço
            FOREIGN KEY (cod_cidade) REFERENCES cidade(cod_cidade),  -- Chave estrangeira para 'cidade'
            FOREIGN KEY (cod_cep) REFERENCES cep(cod_cep),          -- Chave estrangeira para 'cep'
            FOREIGN KEY (cod_bairro) REFERENCES bairro(cod_bairro)   -- Chave estrangeira para 'bairro'
        );
    END

    -- Criação da tabela 'fazenda_urbana'
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'fazenda_urbana')
    BEGIN
        CREATE TABLE fazenda_urbana(
            cod_fazenda INT PRIMARY KEY,
            nome_fazenda VARCHAR(50) UNIQUE,
            razao_social VARCHAR(100),
            cnpj VARCHAR(14) UNIQUE,
            cod_endereco INT,
            tipo_cultivo VARCHAR(50),
            data_inicio DATE,
            FOREIGN KEY(cod_endereco) REFERENCES endereco(cod_endereco)
        );
    END

    -- Criação da tabela 'fazenda_urbana_endereco'
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'fazenda_urbana_endereco')
    BEGIN
        CREATE TABLE fazenda_urbana_endereco(
            cod_endereco INT NOT NULL,
            cod_fazenda INT NOT NULL,
            PRIMARY KEY(cod_endereco, cod_fazenda),
            FOREIGN KEY(cod_endereco) REFERENCES endereco(cod_endereco),
            FOREIGN KEY(cod_fazenda) REFERENCES fazenda_urbana(cod_fazenda)
        );
    END

    -- Criação da tabela 'fazenda_urbana_telefone'
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'fazenda_urbana_telefone')
    BEGIN
        CREATE TABLE fazenda_urbana_telefone(
            cod_telefone INT NOT NULL,
            cod_fazenda INT NOT NULL,
            PRIMARY KEY(cod_telefone, cod_fazenda),
            FOREIGN KEY(cod_telefone) REFERENCES telefone(cod_telefone),
            FOREIGN KEY(cod_fazenda) REFERENCES fazenda_urbana(cod_fazenda)
        );
    END

    -- Criação da tabela 'funcionario_endereco'
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'funcionario_endereco')
    BEGIN
        CREATE TABLE funcionario_endereco(
            cod_funcionario INT NOT NULL,
            cod_endereco INT NOT NULL,
            PRIMARY KEY(cod_funcionario, cod_endereco),
            FOREIGN KEY(cod_funcionario) REFERENCES funcionario(cod_funcionario),
            FOREIGN KEY(cod_endereco) REFERENCES endereco(cod_endereco)
        );
    END

    -- Criação da tabela 'fazenda_funcionario'
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'fazenda_funcionario')
    BEGIN
        CREATE TABLE fazenda_funcionario(
            cod_funcionario INT NOT NULL,
            cod_fazenda INT NOT NULL,
            PRIMARY KEY(cod_funcionario, cod_fazenda),
            FOREIGN KEY(cod_funcionario) REFERENCES funcionario(cod_funcionario),
            FOREIGN KEY(cod_fazenda) REFERENCES fazenda_urbana(cod_fazenda)
        );
    END

    
    -- Atualizando a tabela 'Plantacao'
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Plantacao')
    BEGIN
        CREATE TABLE Plantacao (
            cod_plantacao INT PRIMARY KEY,  -- Código da plantação
            especie varchar(100),           -- especie
            data_plantio DATE,               -- Data do plantio
            data_prevista DATE,
           
            tipo_plantacao VARCHAR(100),     -- Tipo da plantação
            saude_plantacao VARCHAR(100),     -- Saúde da plantação  
            producao_estimada INT            -- Produção estimada
        );
    END

    

    -- Criação da tabela 'Funcionario_Plantacao'
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Funcionario_Plantacao')
    BEGIN
        CREATE TABLE Funcionario_Plantacao (
            cod_plantacao INT NOT NULL,  -- Código da plantação
            cod_funcionario INT NOT NULL,  -- Código do funcionário
            PRIMARY KEY (cod_plantacao, cod_funcionario),  -- Chave primária composta
            FOREIGN KEY(cod_plantacao) REFERENCES Plantacao(cod_plantacao),  -- Chave estrangeira
            FOREIGN KEY(cod_funcionario) REFERENCES funcionario(cod_funcionario)  -- Chave estrangeira
        );
    END

    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Controle_Agua')
    BEGIN
        CREATE TABLE Controle_Agua (
            cod_controle INT PRIMARY KEY,  -- Código do controle
            cod_plantacao INT,              -- Código da plantação
            hora_inicial TIME,              -- Hora inicial do controle
            hora_final TIME,                -- Hora final do controle
            quantidade_agua DECIMAL(10, 2), -- Quantidade de água
            FOREIGN KEY(cod_plantacao) REFERENCES Plantacao(cod_plantacao)  -- Chave estrangeira
        );
    END
END
ELSE
BEGIN
    PRINT 'Banco de dados não existe.';
END



           IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Controle_Luz')
BEGIN
    CREATE TABLE Controle_Luz (
        cod_luz INT PRIMARY KEY,  -- Código da luz
        cod_plantacao INT NOT NULL,  -- Código da plantação
        hora_inicial TIME,  -- Hora inicial
        hora_final TIME,  -- Hora final
        duracao_luz Time,
        intensidade_luz DECIMAL(5, 2),
        data_inicial DATE,  -- Data inicial
        data_final DATE,  -- Data final
        fonte_luz VARCHAR(100),  -- Fonte da luz
        tipo_luz VARCHAR(100),  -- Tipo da luz
        FOREIGN KEY (cod_plantacao) REFERENCES Plantacao(cod_plantacao)  -- Chave estrangeira
    );
END        

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Controle_Temperatura')
BEGIN
    CREATE TABLE Controle_Temperatura (
        cod_temperatura INT PRIMARY KEY,  -- Código da temperatura
        cod_plantacao INT,  -- Código da plantação
        hora_inicial TIME,  -- Hora inicial
        hora_final TIME,  -- Hora final
        duracao TIME,  -- Duração
        data_inicial DATE,  -- Data inicial
        data_final DATE,  -- Data final
        temperatura DECIMAL(3,1),  -- Temperatura
        FOREIGN KEY (cod_plantacao) REFERENCES Plantacao(cod_plantacao)  -- Chave estrangeira
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Controle_Pragas_Doencas')
BEGIN
    CREATE TABLE Controle_Pragas_Doencas (
        cod_pragas_doencas INT PRIMARY KEY,  -- Código das pragas/doenças
        cod_plantacao INT NOT NULL,  -- Código da plantação
        nome_comum VARCHAR(100),  -- Nome comum
        nome_cientifico VARCHAR(100),  -- Nome científico
        tipo VARCHAR(100),  -- Tipo
        data_deteccao DATE,  -- Data de detecção
        eficacia DECIMAL(3,2),  -- Eficácia
        severidade VARCHAR(100),  -- Severidade
        metodo_controle VARCHAR(255),  -- Método de controle
        FOREIGN KEY (cod_plantacao) REFERENCES Plantacao(cod_plantacao)  -- Chave estrangeira
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Colheita')
BEGIN
    CREATE TABLE Colheita (
        cod_colheita INT PRIMARY KEY,  -- Código da colheita
        cod_plantacao INT,  -- Código da plantação
        cod_funcionario INT,  -- Código do funcionário
        quantidade_colhida INT,  -- Quantidade colhida
        data_colhida DATE,  -- Data da colheita
        FOREIGN KEY (cod_plantacao) REFERENCES Plantacao(cod_plantacao),  -- Chave estrangeira
        FOREIGN KEY (cod_funcionario) REFERENCES funcionario(cod_funcionario)  -- Chave estrangeira
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Produto')
BEGIN
    CREATE TABLE Produto (
        cod_produto INT PRIMARY KEY,  -- Código do produto
        cod_colheita INT NOT NULL,  -- Código da colheita
        tipo_produto VARCHAR(100),  -- Tipo de produto
        FOREIGN KEY (cod_colheita) REFERENCES Colheita(cod_colheita)  -- Chave estrangeira
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Qualidade')
BEGIN
    CREATE TABLE Qualidade (
        cod_qualidade INT PRIMARY KEY,  -- Código da qualidade
        cod_produto INT,  -- Código do produto
        classificacao VARCHAR(100),  -- Classificação
        data_avaliacao DATE,  -- Data da avaliação
        FOREIGN KEY (cod_produto) REFERENCES Produto(cod_produto)  -- Chave estrangeira
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Estoque')
BEGIN
    CREATE TABLE Estoque (
        cod_estoque INT PRIMARY KEY,  -- Código do estoque
        cod_produto INT NOT NULL,  -- Código do produto
        data_entrada DATE,  -- Data de entrada
        estado_produto VARCHAR(100),  -- Estado do produto
        quantidade INT,  -- Quantidade
        data_validade_produto DATE,  -- Data de validade do produto
        lote VARCHAR(100),  -- Lote
        data_saida DATE,  -- Data de saída
        FOREIGN KEY (cod_produto) REFERENCES Produto(cod_produto)  -- Chave estrangeira
    );
END

-- Modificando o nome da coluna 'tipo_morango' para 'tipo_produto' na tabela 'Produto'
EXEC sp_rename 'Produto.tipo_produto', 'tipo_produto', 'COLUMN';

-- Modificando o tipo da coluna 'eficácia' na tabela 'Controle_Pragas_Doencas'
ALTER TABLE Controle_Pragas_Doencas
ALTER COLUMN eficacia DECIMAL(5,2);  -- Modifica a coluna 'eficácia'";


            // Conexão ao banco de dados recém-criado
            using (SqlConnection con = Conectar())
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand(checkDbQuery, con))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Tabelas criadas ou já existem.");
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw new Exception("Erro ao criar as tabelas: " + sqlEx.Message);
                }
            }

        }
    }
}
