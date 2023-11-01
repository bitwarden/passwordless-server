# ** Build

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /tmp/pwdls/

COPY Directory.Build.props ./
COPY src/Common src/Common
COPY src/Service src/Service
COPY src/Api src/Api

RUN dotnet publish src/Api/ \
    --configuration Release \
    --self-contained \
    --use-current-runtime \
    --output src/Api/bin/publish/

# ** Run

# Use `runtime-deps` instead of `runtime` because we have a self-contained assembly
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine AS run
WORKDIR /opt/pwdls/

EXPOSE 80
EXPOSE 443

COPY --from=build /tmp/pwdls/src/Api/bin/publish ./
ENTRYPOINT ["./Passwordless.Api"]