using Npgsql;

namespace DevBankWithDotnet.Repositories;

public class NpgsqlContext
{

    private readonly IConfiguration configuration;
    private NpgsqlConnection? _connection;


    public NpgsqlContext(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    ~NpgsqlContext()
    {
        _connection?.Close();
        _connection?.Dispose();
    }

    public NpgsqlConnection Connection() {

        if(_connection == null)
        {
            _connection = new NpgsqlConnection(configuration.GetConnectionString("DevBank")!);
        }

        if (_connection.State != System.Data.ConnectionState.Open) { 
            _connection.Open();
        }

        return _connection;
    }

    
}
