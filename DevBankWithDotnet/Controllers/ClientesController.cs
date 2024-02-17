using DevBankWithDotnet.Repositories;
using DevBankWithDotnet.Repositories.Commands;
using DevBankWithDotnet.Repositories.Model;
using Microsoft.AspNetCore.Mvc;

namespace DevBankWithDotnet.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientesController : ControllerBase
{

    [HttpGet("{id:int}/extrato")]
    public async Task<ActionResult<Extrato>> ObterExtrato(int id, [FromServices] ClienteRepository repository, CancellationToken cancellationToken)
    {
        if (id < 1 || id > 5) 
        {
            var response = Content("");
            response.StatusCode = 404;
            return response;               
        }

        var res = await repository.ObterExtrato(id, cancellationToken);
        
        if (res == null)
        {
            var response = Content("");
            response.StatusCode = 422;
            return response;
        }

        return Ok(res);
    }

    [HttpPost("{id:int}/transacoes")]
    public async Task<ActionResult<Resultado>> AdicionarTransacao(int id, [FromBody] TransacaoCommand command, [FromServices] ClienteRepository repository, CancellationToken cancellationToken)
    {
        if (id < 1 || id > 5)
        {
            var response = Content("");
            response.StatusCode = 404;
            return response;
        }

        if (command.Descricao.Length > 10 ||
            string.IsNullOrWhiteSpace(command.Descricao) ||
            (command.Valor - (int)command.Valor) > 0 ||
            (command.Tipo != 'd' && command.Tipo != 'c'))
        {
            var response = Content("");
            response.StatusCode = 422;
            return response;
        }

        var res = await repository.AdicionarTransacao(id, command, cancellationToken);

        if (res == null) {
            var response = Content("");
            response.StatusCode = 422;
            return response;
        }

        return Ok(res);
    }
}
