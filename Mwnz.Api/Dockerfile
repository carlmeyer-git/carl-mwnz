FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY openapi-companies.yaml .
COPY ["Mwnz.Api/Mwnz.Api.csproj", "Mwnz.Api/"]
RUN dotnet restore "Mwnz.Api/Mwnz.Api.csproj"

COPY Mwnz.Api/ Mwnz.Api/
WORKDIR /src/Mwnz.Api
RUN dotnet publish "Mwnz.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Mwnz.Api.dll"]
