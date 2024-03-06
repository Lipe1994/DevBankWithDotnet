using Dapper;
using DevBankWithDotnet.Repositories.Commands;
using DevBankWithDotnet.Repositories.Model;
using Npgsql;

namespace DevBankWithDotnet.Repositories;

public class ClienteRepository
{
    private readonly NpgsqlContext npgsqlContext;

    public ClienteRepository(NpgsqlContext npgsqlContext)
    {
        this.npgsqlContext = npgsqlContext;
    }

    public async Task<Resultado?> AdicionarTransacao(int clienteId, TransacaoCommand command, CancellationToken cancellationToken)
    {
        var novoValor = (int)command.Valor;
        using (NpgsqlConnection connection = await npgsqlContext.Connection.OpenConnectionAsync())
        {
                Resultado? resultado;
                switch (command.Tipo)
                {
                    case 'd':
                        resultado = await connection.QueryFirstOrDefaultAsync<Resultado>(
                            new CommandDefinition(@"SELECT _limite Limite, _total Total, LinhaAfetada FROM debitar(@Id, @Valor, @Descricao)",
                                new
                                {
                                    Id = clienteId,
                                    Valor = novoValor,
                                    Descricao = command.Descricao
                                },
                                cancellationToken: cancellationToken));
                        break;
                    case 'c':
                        resultado = await connection.QueryFirstOrDefaultAsync<Resultado>(
                            new CommandDefinition(@"SELECT _limite Limite, _total Total, LinhaAfetada  FROM creditar(@Id, @Valor, @Descricao)",
                            new
                            {
                                Id = clienteId,
                                Valor = novoValor,
                                Descricao = command.Descricao
                            },
                            cancellationToken: cancellationToken));
                        break;
                    default:
                        return null;
                }


                return resultado;
            
        }
    }


    public async Task<Extrato?> ObterExtrato(int clienteId, CancellationToken cancellationToken)
    {
        using (NpgsqlConnection connection = await npgsqlContext.Connection.OpenConnectionAsync())
        {

            var sql = @"
SELECT c.Id, c.Total, c.Limite, t.Valor, t.Descricao, t.Tipo, t.CriadoEm
   FROM Cliente c 
   left join Transacao t on t.ClienteId = c.Id 
WHERE c.Id = @Id
ORDER BY t.Id DESC 
LIMIT 10
";
            var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", clienteId);

            NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);


            if (!reader.HasRows)
            {
                return null;
            }

            var transacoes = new List<TransacaoResumo>();
            var saldo = new Saldo();

            bool isFirst = true;
            bool hasExtract = false;
            while (reader.Read())
            {
                if (isFirst) 
                {
                    saldo.Id = (Int16)reader["Id"];
                    saldo.Total = (Int32)reader["Total"];
                    saldo.Limite = (Int32)reader["Limite"];
                    saldo.DataExtrato = DateTime.Now;                        
                    isFirst = false;
                    hasExtract = reader["Valor"] is Int32;
                }

                if (hasExtract)
                    transacoes.Add(new TransacaoResumo
                    {
                        Valor = (Int32)reader["Valor"],
                        Tipo = ((string)reader["Tipo"])[0],
                        Descricao = (string)reader["Descricao"],
                        CriadoEm = (DateTime)reader["CriadoEm"]
                    });
            }

            return new Extrato() {
                Saldo = saldo,
                Transacoes = transacoes,
            };
        }
    }
}