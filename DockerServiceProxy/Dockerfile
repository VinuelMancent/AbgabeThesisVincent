FROM mcr.microsoft.com/dotnet/sdk:6.0.405-alpine3.17-amd64 as build-env
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0.13-alpine3.17-amd64 as runtime
WORKDIR /publish
COPY --from=build-env /publish .
EXPOSE 80
ENTRYPOINT ["./DockerServiceProxy"]