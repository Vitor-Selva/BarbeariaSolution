FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Só o .csproj: a camada de restore fica em cache enquanto os pacotes não mudarem
COPY BarbeariaAPI.csproj ./
RUN dotnet restore

# Depois o código; alterar um .cs não refaz o restore
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "BarbeariaAPI.dll"]