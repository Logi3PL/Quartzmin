FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["./SelfHosting.API/SelfHosting.API.csproj", "./SelfHosting.API/"]
COPY ["./SelfHosting.Common/SelfHosting.Common.csproj", "./SelfHosting.Common/"]
COPY ["./SelfHosting.Repository/SelfHosting.Repository.csproj", "./SelfHosting.Repository/"]
COPY ["./SelfHosting.Services/SelfHosting.Services.csproj", "./SelfHosting.Services/"]
RUN dotnet restore "SelfHosting.API/SelfHosting.API.csproj"
COPY . .
WORKDIR "/src/SelfHosting.API"
RUN dotnet build "./SelfHosting.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./SelfHosting.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS="http://*:1455"
ENTRYPOINT ["dotnet", "SelfHosting.API.dll"]