using DevBankWithDotnet.Repositories;
using DevBankWithDotnet.Repositories.Commands;
using DevBankWithDotnet.Repositories.Model;
using Microsoft.AspNetCore.Mvc;

namespace DevBankWithDotnet.Controllers;


[ApiController]
[Route("[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ILogger logger;

    public ClientesController(ILogger<ClientesController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("{id:int}/extrato")]
    public async Task<ActionResult<Extrato>> ObterExtrato(int id, [FromServices] ClienteRepository repository, CancellationToken cancellationToken)
    {
        if (id < 1 || id > 5) 
        {
            return NotFound();               
        }

        var res = await repository.ObterExtrato(id, cancellationToken);
        
        if (res == null)
        {
            return UnprocessableEntity();
        }

        return Ok(res);
    }

    
    [HttpPost("{id:int}/transacoes")]
    public async Task<ActionResult<Resultado>> AdicionarTransacao(int id, [FromBody] TransacaoCommand command, [FromServices] ClienteRepository repository, CancellationToken cancellationToken)
    {
        
        if (id < 1 || id > 5) 
        {
            return NotFound();               
        }

        if (string.IsNullOrWhiteSpace(command.Descricao) ||
            command.Descricao.Length > 10 ||
            (command.Valor - (int)command.Valor) > 0 ||
            (command.Tipo != 'd' && command.Tipo != 'c'))
        {
            return UnprocessableEntity();
        }

        var res = await repository.AdicionarTransacao(id, command, cancellationToken);

        if (res == null) {
            return UnprocessableEntity();
        }

        return Ok(res);
    }
}
