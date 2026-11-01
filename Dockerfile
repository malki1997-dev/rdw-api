FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY RdwApi/RdwApi.csproj RdwApi/
RUN dotnet restore RdwApi/RdwApi.csproj
COPY RdwApi/ RdwApi/
RUN dotnet publish RdwApi/RdwApi.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "RdwApi.dll"]