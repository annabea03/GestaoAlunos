using MySql.Data.MySqlClient;
using System;
using System.Data.SqlClient;
using System.Security.Cryptography; // Necessário para evitar um warning em alguns compiladores como eu falei no relatório, mas pode ser removido.

namespace GestaoAlunos
{
    class Program
    {
        //String de Conexão - Corrigida a porta (3306) e mantido senha
        private const string STRING_CONEXAO = "server=localhost; uid=root;pwd=SuaSenhaAqui; database=escola; port=3306";

        static void Main(string[] args)
        {
            Console.WriteLine("Gerenciador de Alunos - C# e MySQL");
            bool executando = true;

            while (executando)
            {
                // Exibe o Menu
                ExibirMenu();
                string opcao = Console.ReadLine();
                Console.Clear();

                // Bloco usando a conexão (corrigida a sintaxe)
                using (MySqlConnection conexao = new MySqlConnection(STRING_CONEXAO))
                {
                    try
                    {
                        conexao.Open();
                        Console.WriteLine("Conexão aberta com sucesso!");

                        // Implementei Switch Case
                        switch (opcao)
                        {
                            case "1":
                                CadastrarAluno(conexao);
                                break;
                            case "2":
                                ListarAlunos(conexao);
                                break;
                            case "4":
                                AtualizarAluno(conexao);
                                break;
                            case "5":
                                ExcluirAluno(conexao);
                                break;
                            case "6":
                                ExibirTotalAlunos(conexao);
                                break;
                            case "7":
                                executando = false;
                                Console.WriteLine("Saindo do sistema...");
                                break;
                            default:
                                Console.WriteLine("Opção inválida ou não implementada (Busca por nome). Pressione Enter para continuar.");
                                break;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        [cite_start]// Tratamento de Exceções [cite: 99, 106, 181]
                        Console.WriteLine($"Erro ao conectar ou executar comando: {ex.Message}");
                    }
                }

                if (executando)
                {
                    Console.WriteLine("\nPressione Enter para voltar ao menu.");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }

        static void ExibirMenu()
        {
            // O menu completo
            Console.WriteLine("--- Menu Principal ---");
            Console.WriteLine("1. Cadastrar aluno");
            Console.WriteLine("2. Listar todos os alunos");
            Console.WriteLine("3. Buscar aluno por nome (Não consegui implementar bem");
            Console.WriteLine("4. Atualizar aluno");
            Console.WriteLine("5. Excluir aluno");
            Console.WriteLine("6. Exibir total de alunos");
            Console.WriteLine("7. Sair");
            Console.Write("Escolha uma opção: ");
        }

        // --- métodos crud basicos ---

        static void CadastrarAluno(MySqlConnection conexao)
        {
            Console.WriteLine("--- Cadastro de Aluno ---");
            Console.Write("Nome: ");
            string nomeDigitado = Console.ReadLine();

            Console.Write("Idade: ");
            [cite_start] if (!int.TryParse(Console.ReadLine(), out int idadeDigitada)) // Validação básica [cite: 182]
            {
                Console.WriteLine("Erro: Idade deve ser um número inteiro.");
                return;
            }

            Console.Write("Curso: ");
            string cursoDigitado = Console.ReadLine();

            [cite_start]// Uso de parâmetros para segurança (SQL Injection) 
            string sqlInsert = "INSERT INTO alunos (Nome, Idade, Curso) VALUES (@nome, @idade, @curso)";

            using (MySqlCommand cmd = new MySqlCommand(sqlInsert, conexao))
            {
                // Corrigido o Erro 4 (troca de parâmetros no AddWithValue)
                cmd.Parameters.AddWithValue("@nome", nomeDigitado);
                cmd.Parameters.AddWithValue("@idade", idadeDigitada);
                cmd.Parameters.AddWithValue("@curso", cursoDigitado);

                int linhas = cmd.ExecuteNonQuery(); // Retorna linhas afetadas 
                Console.WriteLine($"{linhas} registro(s) inserido(s) com sucesso!");
            }
        }

        static void ListarAlunos(MySqlConnection conexao)
        {
            Console.WriteLine("--- Lista de Alunos ---");
            string sqlSelect = "SELECT Id, Nome, Idade, Curso FROM alunos";

            using (MySqlCommand cmd = new MySqlCommand(sqlSelect, conexao))
            [cite_start] using (MySqlDataReader reader = cmd.ExecuteReader()) // Executa SELECT 
            {
                if (!reader.HasRows)
                {
                    Console.WriteLine("Nenhum aluno cadastrado.");
                    return;
                }

                [cite_start] while (reader.Read()) // Percorre os resultados linha por linha [cite: 124, 135]
                {
                    [cite_start]// Conversão dos valores [cite: 125]
                    int id = Convert.ToInt32(reader["Id"]);
                    string nome = reader.GetString("Nome");
                    int idade = reader.GetInt32("Idade");
                    string curso = reader.GetString("Curso");

                    Console.WriteLine($"ID: {id} - Nome: {nome}, Idade: {idade}, Curso: {curso}");
                }
            }
        }

        static void AtualizarAluno(MySqlConnection conexao)
        {
            Console.WriteLine("--- Atualização de Aluno ---");
            Console.Write("ID do aluno para atualizar: ");
            if (!int.TryParse(Console.ReadLine(), out int idParaAtualizar)) { return; }

            Console.Write("NOVO Nome: ");
            string novoNome = Console.ReadLine();

            Console.Write("NOVA Idade: ");
            if (!int.TryParse(Console.ReadLine(), out int novaIdade)) { return; }

            Console.Write("NOVO Curso: ");
            string novoCurso = Console.ReadLine();

            string sqlUpdate = "UPDATE alunos SET Nome = @nome, Idade = @idade, Curso = @curso WHERE Id = @id";

            using (MySqlCommand cmd = new MySqlCommand(sqlUpdate, conexao))
            {
                cmd.Parameters.AddWithValue("@nome", novoNome);
                cmd.Parameters.AddWithValue("@idade", novaIdade);
                cmd.Parameters.AddWithValue("@curso", novoCurso);
                cmd.Parameters.AddWithValue("@id", idParaAtualizar); // Cláusula WHERE obrigatória [cite: 160]

                int alterados = cmd.ExecuteNonQuery(); // Executa UPDATE 
                Console.WriteLine($"{alterados} registro(s) atualizado(s).");
            }
        }

        static void ExcluirAluno(MySqlConnection conexao)
        {
            Console.WriteLine("--- Exclusão de Aluno ---");
            Console.Write("ID do aluno para excluir: ");
            if (!int.TryParse(Console.ReadLine(), out int idParaExcluir)) { return; }

            string sqlDelete = "DELETE FROM alunos WHERE Id=@id";
            using (MySqlCommand cmd = new MySqlCommand(sqlDelete, conexao))
            {
                cmd.Parameters.AddWithValue("@id", idParaExcluir);

                int removidos = cmd.ExecuteNonQuery(); // Executa DELETE 
                Console.WriteLine($"{removidos} registro(s) excluído(s).");
            }
        }

        static void ExibirTotalAlunos(MySqlConnection conexao)
        {
            Console.WriteLine("--- Total de Alunos ---");
            string sqlCount = "SELECT COUNT(*) FROM alunos";

            using (MySqlCommand cmd = new MySqlCommand(sqlCount, conexao))
            {
                object resposta = cmd.ExecuteScalar(); // Obtém um único valor [cite: 116, 167]
                int total = Convert.ToInt32(resposta);
                Console.WriteLine($"Total de alunos cadastrados: {total}");
            }
        }
    }
}