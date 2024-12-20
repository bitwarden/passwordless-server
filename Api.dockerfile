# ** Build

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build

# Expose the target architecture set by the `docker build --platform` option, so that
# we can build the assembly for the correct platform.
ARG TARGETARCH

# Allow setting the assembly version from the build command
ARG VERSION=0.0.0

WORKDIR /tmp/app/

COPY Directory.Build.props ./
COPY src/Common src/Common
COPY src/Service src/Service
COPY src/Api src/Api

RUN dotnet publish src/Api/ \
    -p:Version=$VERSION \
    --configuration Release \
    --self-contained \
    --use-current-runtime \
    --arch $TARGETARCH \
    --output src/Api/bin/publish/

# ** Run

# Use `runtime-deps` instead of `runtime` because we have a self-contained assembly
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime-deps:9.0 AS run

LABEL org.opencontainers.image.title="Passwordless API Test Server"
LABEL org.opencontainers.image.description="Docker image of the Passwordless API, intended solely for development and integration testing purposes."
LABEL org.opencontainers.image.authors="Bitwarden"
LABEL org.opencontainers.image.url="https://github.com/passwordless/passwordless-server"
LABEL org.opencontainers.image.source="https://github.com/passwordless/passwordless-server/blob/main/Api.dockerfile"

EXPOSE $ASPNETCORE_HTTP_PORTS

# Alpine image doesn't come with the ICU libraries pre-installed, so we need to install them manually.
# Technically, we shouldn't need globalization support in the API, but some EF queries fail without it at the moment.
# `libsodium` is required by the `NSec.Cryptography` package.
# RUN apk add --no-cache icu-libs libsodium gcompat
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Use the default non-root user for the app.
# This instruction must appear after all other instructions that require elevated access.
RUN mkdir -p /app
RUN chmod 777 /app
USER $APP_UID

WORKDIR /opt/app/
COPY --from=build /tmp/app/src/Api/bin/publish ./

ENV MAIL__FROM="test@lesspassword.dev"
ENV MAIL__PROVIDERS__0__NAME="file"
ENV MAIL__PROVIDERS__0__PATH="/app/mail.md"

ENTRYPOINT ["./Passwordless.Api"]