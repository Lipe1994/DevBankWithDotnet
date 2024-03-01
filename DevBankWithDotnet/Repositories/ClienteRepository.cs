using Dapper;
using DevBankWithDotnet.Repositories.Commands;
using DevBankWithDotnet.Repositories.Model;
using Npgsql;

namespace DevBankWithDotnet.Repositories;

public class ClienteRepository
{
    private readonly NpgsqlContext npgsqlContext;
    private readonly ILogger<ClienteRepository> logger;

    public ClienteRepository(NpgsqlContext npgsqlContext, ILogger<ClienteRepository> logger)
    {
        this.npgsqlContext = npgsqlContext;
        this.logger = logger;
    }

    public async Task<Resultado?> AdicionarTransacao(int clienteId, TransacaoCommand command, CancellationToken cancellationToken)
    {
        var novoValor = (int)command.Valor;
        using (NpgsqlConnection connection = await npgsqlContext.Connection.OpenConnectionAsync())
        {
            using (var transaction = connection.BeginTransaction())
            {
                Resultado? resultado;
                switch (command.Tipo)
                {
                    case 'd':
                        resultado = await connection.QueryFirstOrDefaultAsync<Resultado>(
                            new CommandDefinition(
                                @"WITH Updated as(
                                    UPDATE Cliente c SET Total=Total-@novoValor  
                                        WHERE Id=@Id
                                        AND abs(Total - @novoValor) <= Limite
                                    RETURNING Limite, Total) SELECT true LinhaAfetada, Limite, Total FROM Updated",
                            new
                            {
                                Id = clienteId,
                                novoValor,
                            },
                            transaction: transaction,
                            cancellationToken: cancellationToken));
                        break;
                    case 'c':
                        resultado = await connection.QueryFirstOrDefaultAsync<Resultado>(
                            new CommandDefinition(
                             @"WITH Updated as (
                                UPDATE public.Cliente c SET Total=Total+@novoValor  
                                WHERE Id=@Id
                             RETURNING Limite, Total) SELECT true LinhaAfetada, Limite, Total FROM Updated",
                            new
                            {
                                Id = clienteId,
                                novoValor,
                            },
                            transaction: transaction,
                            cancellationToken: cancellationToken));
                        break;
                    default:
                        return null;
                }

                if (resultado?.LinhaAfetada == null || !resultado!.LinhaAfetada)
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