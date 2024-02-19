Este projeto usa Dotnet 8

Como dotnet cria um diretório root para solução um nível acima do projeto, é necessário entrar no diretório do projeto:

```shell
cd DevBankWithDotnet
```

Dockerizar o projeto:

```shell
docker build -t lipeferreira1609/dev_bank_with_dotnet:latest .
```

Montar um volume com docker-compose:

```shell
docker-compose up -d
```

Desmontar um volume com docker-compose:

```shell
docker-compose down
```
