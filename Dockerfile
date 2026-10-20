# Stage 1: Build e Publicação dos Artefatos
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia apenas os arquivos .csproj preservando a estrutura de pastas
# (Permite que o Docker utilize cache de camadas para o restore de pacotes)
COPY src/FCG02.UsersAPI.Domain/FCG02.UsersAPI.Domain.csproj src/FCG02.UsersAPI.Domain/
COPY src/FCG02.UsersAPI.Application/FCG02.UsersAPI.Application.csproj src/FCG02.UsersAPI.Application/
COPY src/FCG02.UsersAPI.Infrastructure/FCG02.UsersAPI.Infrastructure.csproj src/FCG02.UsersAPI.Infrastructure/
COPY src/FCG02.UsersAPI.API/FCG02.UsersAPI.API.csproj src/FCG02.UsersAPI.API/

# Restaura dependências do NuGet
RUN dotnet restore src/FCG02.UsersAPI.API/FCG02.UsersAPI.API.csproj

# Copia todo o código-fonte restante
COPY src/ src/

# Compila e publica em modo Release otimizado
RUN dotnet publish src/FCG02.UsersAPI.API/FCG02.UsersAPI.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Mínimo e Seguro para Produção
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Executa com usuário sem privilégios de root (segurança nativa .NET 10)
USER app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copia estritamente os artefatos compilados do Stage 1
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG02.UsersAPI.API.dll"]
