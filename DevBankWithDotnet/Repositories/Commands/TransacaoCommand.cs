﻿namespace DevBankWithDotnet.Repositories.Commands;

public struct TransacaoCommand
{
    public double Valor { get; set; }
    public char Tipo { get; set; }
    public string? Descricao { get; set; }
}
