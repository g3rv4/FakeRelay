ARG ARCH=
FROM mcr.microsoft.com/dotnet/sdk:8.0.100-1-alpine3.18 AS builder
WORKDIR /src
COPY src /src/
RUN dotnet publish -c Release /src/FakeRelay.sln -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0.0-alpine3.18-${ARCH}
WORKDIR /app
ENV CONFIG_PATH=/data/config.json
COPY --from=builder /app /app
COPY entrypoint.sh /app/entrypoint.sh
ENTRYPOINT [ "/app/entrypoint.sh" ]
