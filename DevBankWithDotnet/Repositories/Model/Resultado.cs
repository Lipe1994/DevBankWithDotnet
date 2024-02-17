using System.Text.Json.Serialization;

namespace DevBankWithDotnet.Repositories.Model;

public class Resultado
{
    public int Limite { get; set; }
    public int Saldo { get; set; }

    [JsonIgnore]
    public int Versao { get; set; }
}
