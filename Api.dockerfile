LABEL org.opencontainers.image.title="Passwordless API Test Server"
LABEL org.opencontainers.image.description="Docker image of the Passwordless API, intended solely for development and integration testing purposes."
LABEL org.opencontainers.image.authors="Bitwarden"
LABEL org.opencontainers.image.url="https://github.com/passwordless/passwordless-server"
LABEL org.opencontainers.image.source="https://github.com/passwordless/passwordless-server/blob/main/Api.dockerfile"

# ** Build

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build

# Expose the target architecture set by the `docker build --platform` option, so that
# we can build the assembly for the correct platform.
ARG TARGETARCH

WORKDIR /tmp/pwdls/

COPY Directory.Build.props ./
COPY src/Common src/Common
COPY src/Service src/Service
COPY src/Api src/Api

RUN dotnet publish src/Api/ \
    --configuration Release \
    --self-contained \
    --use-current-runtime \
    --arch $TARGETARCH \
    --output src/Api/bin/publish/

# ** Run

# Use `runtime-deps` instead of `runtime` because we have a self-contained assembly
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine AS run
WORKDIR /opt/pwdls/

EXPOSE 80
EXPOSE 443

# Alpine image doesn't come with the ICU libraries pre-installed, so we need to install them manually.
# Technically, we shouldn't need globalization support in the API, but some EF queries fail without it at the moment.
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /tmp/pwdls/src/Api/bin/publish ./
ENTRYPOINT ["./Passwordless.Api"]