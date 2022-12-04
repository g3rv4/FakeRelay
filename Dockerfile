# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:6.0.403-alpine3.16 AS builder
WORKDIR /src
COPY src /src/
RUN dotnet publish -c Release /src/FakeRelay.sln -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0.11-alpine3.16  
VOLUME ["/data"]
ENV CONFIG_PATH=/data/config.json
COPY --from=builder /app /app
COPY entrypoint.sh /app/entrypoint.sh
RUN ln -s /app/FakeRelay.Cli /bin/cli && ln -s /app/FakeRelay.Web /bin/web && chmod +x /app/entrypoint.sh
ENTRYPOINT [ "/app/entrypoint.sh" ]
