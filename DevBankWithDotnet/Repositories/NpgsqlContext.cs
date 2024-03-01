using Npgsql;

namespace DevBankWithDotnet.Repositories;

public class NpgsqlContext
{

    private readonly IConfiguration configuration;
    private NpgsqlDataSource? _dataSource;


    public NpgsqlContext(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    ~NpgsqlContext()
    {
        _dataSource?.Dispose();
        _dataSource = null;
    }

    public NpgsqlDataSource Connection {

        get{
            if(_dataSource == null)
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DevBank")!);
                
                _dataSource = dataSourceBuilder.Build();
            }


            return _dataSource;
        }
    }

    
}
