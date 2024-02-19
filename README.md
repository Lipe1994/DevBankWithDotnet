### Submissão para Rinha de Backend, Segunda Edição: 2024/Q1 - Controle de Concorrência

![Imagem que representa csharp](./csharp_logo.png)

##### Stack:

    - Dotnet 8
    - C#
    - Postgres
    - Nginx

##### Repositório

- [Lipe1994/DevBankWithDotnet](https://github.com/Lipe1994/DevBankWithDotnet)

##### Filipe Ferreira:

- [@filipe-ferreira-425380123](https://www.linkedin.com/in/filipe-ferreira-425380123/) - Linkedin

- [@l1peferreira](https://www.instagram.com/l1peferreira/) - Instagram

###### Detalhes de execução e montagem do projeto:

- Como dotnet cria um diretório root para solução um nível acima do projeto, é necessário entrar no diretório do projeto:

```shell
cd DevBankWithDotnet
```

- Dockerizar o projeto:

```shell
docker build -t lipeferreira1609/dev_bank_with_dotnet:latest .
```

- Montar um volume com docker-compose:

```shell
docker-compose up -d
```

- Desmontar um volume com docker-compose:

```shell
docker-compose down
```
