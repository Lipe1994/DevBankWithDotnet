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
        using (NpgsqlConnection connection = npgsqlContext.Connection())
        {
            var sqlCommand = new CommandDefinition("SELECT Total Saldo, Limite, Versao FROM public.Cliente WHERE Id=@Id",
                new { Id = clienteId }, 
                cancellationToken: cancellationToken);

            var resultado = await connection.QueryFirstOrDefaultAsync<Resultado>(sqlCommand);

            if (resultado == null)
            {
                return null;
            }

            switch (command.Tipo)
            {
                case 'd':
                    if (int.Abs((resultado.Saldo - novoValor)) > resultado!.Limite)
                    {
                        return null;
                    }
                    resultado.Saldo -= novoValor;
                    break;
                case 'c':
                    resultado.Saldo += novoValor;
                    break;
                default:
                    return null;
            }

            using (var transaction = connection.BeginTransaction())
            {                
                var linhasAfetadas = await connection.ExecuteAsync(
                    new CommandDefinition("UPDATE public.Cliente SET Total=@Saldo, Versao=@Versao WHERE Id=@Id AND Versao=@OldVersion",
                    new
                    {
                        Id = clienteId,
                        resultado.Saldo,
                        OldVersion = resultado.Versao,
                        Versao = resultado.Versao++
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

                if (linhasAfetadas == 0)
                {
                    return null;
                }

                await connection.ExecuteAsync(
                    new CommandDefinition("INSERT INTO public.Transacao(Valor, Tipo, Descricao, ClienteId, CriadoEm) values(@Valor, @Tipo, @Descricao, @ClienteId, @CriadoEm)", new
                    {
                        command.Valor,
                        command.Tipo,
                        command.Descricao,
                        ClienteId = clienteId,
                        CriadoEm = DateTime.UtcNow
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken)
                );

                await transaction.CommitAsync();
                return resultado;
            }
        }
    }


    public async Task<Extrato?> ObterExtrato(int clienteId, CancellationToken cancellationToken)
    {
        using (NpgsqlConnection connection = npgsqlContext.Connection())
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