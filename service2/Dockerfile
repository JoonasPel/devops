FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

COPY ./wait-for-it.sh /wait-for-it.sh
RUN chmod +x wait-for-it.sh

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "service.dll"]