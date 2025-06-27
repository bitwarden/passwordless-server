# syntax = docker/dockerfile:1.6
###############################################
#                 Build stage                 #
###############################################
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build

# Docker buildx supplies the value for this arg
ARG TARGETPLATFORM

# Determine proper runtime value for .NET
# We put the value in a file to be read by later layers.
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
  RID=linux-x64 ; \
  elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
  RID=linux-arm64 ; \
  elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
  RID=linux-arm ; \
  fi \
  && echo "RID=$RID" > /tmp/rid.txt

# Add packages
RUN apt-get update && apt-get install -y \
  npm jq \
  curl unzip \
  && rm -rf /var/lib/apt/lists/*

# Copy csproj files as distinct layers
WORKDIR /source
COPY src/AdminConsole/*.csproj ./src/AdminConsole/
COPY src/Api/*.csproj ./src/Api/
COPY src/Service/*.csproj ./src/Service/
COPY src/Common/*.csproj ./src/Common/
COPY Directory.Build.props .

# Restore AdminConsole project dependencies and tools
WORKDIR /source/src/AdminConsole
RUN . /tmp/rid.txt && dotnet restore -r $RID

# Restore Api project dependencies and tools
WORKDIR /source/src/Api
RUN . /tmp/rid.txt && dotnet restore -r $RID

# Copy required project files
WORKDIR /source
COPY src/AdminConsole/. ./src/AdminConsole/
COPY src/Api/. ./src/Api/
COPY src/Service/. ./src/Service/
COPY src/Common/. ./src/Common/
COPY .git/. ./.git/

# Build Admin app
WORKDIR /source/src/AdminConsole
RUN npm install
RUN . /tmp/rid.txt && dotnet publish -c release -o /app/AdminConsole --no-restore --no-self-contained -r $RID

# Build Api app
WORKDIR /source/src/Api
RUN . /tmp/rid.txt && dotnet publish -c release -o /app/Api --no-restore --no-self-contained -r $RID

# Download hbs tool for generating final configurations
RUN apt-get update && apt-get install -y curl unzip && rm -rf /var/lib/apt/lists/*
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ] ; then curl -L --output hbs.zip https://github.com/bitwarden/Handlebars.conf/releases/download/v1.3.0/hbs_linux-x64.zip; fi
RUN if [ "$TARGETPLATFORM" = "linux/arm/v7" ] ; then curl -L --output hbs.zip https://github.com/bitwarden/Handlebars.conf/releases/download/v1.3.0/hbs_linux-armv7.zip; fi
RUN if [ "$TARGETPLATFORM" = "linux/arm64" ] ; then curl -L --output hbs.zip https://github.com/bitwarden/Handlebars.conf/releases/download/v1.3.0/hbs_linux-arm64.zip; fi

# Extract hbs
RUN unzip hbs.zip -d /app/hbs && rm hbs.zip
RUN chmod +x /app/hbs/hbs

# Download and extract packages needed for the final image
WORKDIR /packages
RUN apt-get update && apt-get download \
  nginx \
  openssl \
  supervisor \
  tzdata \
  jq \
  libc6 \
  libssl3 \
  libpcre2-8-0 \
  zlib1g \
  python3-minimal \
  python3-setuptools \
  libpython3.11-minimal \
  mime-support \
  && rm -rf /var/lib/apt/lists/*

# Extract packages to a staging directory
RUN mkdir -p /staging
RUN for pkg in *.deb; do \
  echo "Extracting $pkg"; \
  dpkg-deb -x "$pkg" /staging; \
  done

# Ensure proper symlinks and paths for extracted binaries
SHELL ["/bin/bash", "-o", "pipefail", "-c"]
RUN find /staging -type f -executable -name "nginx" -o -name "supervisord" -o -name "jq" -o -name "openssl" | head -10

# Create necessary directory structure
RUN mkdir -p /staging/etc/bitwarden_passwordless/data-protection
RUN mkdir -p /staging/etc/bitwarden_passwordless/logs
RUN mkdir -p /staging/etc/supervisor
RUN mkdir -p /staging/etc/supervisor.d
RUN mkdir -p /staging/var/log/bitwarden
RUN mkdir -p /staging/var/log/bitwarden_passwordless
RUN mkdir -p /staging/var/log/nginx/logs
RUN mkdir -p /staging/etc/nginx/http.d
RUN mkdir -p /staging/var/run/nginx
RUN mkdir -p /staging/var/lib/nginx/tmp
RUN touch /staging/var/run/nginx/nginx.pid
RUN mkdir -p /staging/app

# Copy configuration files and set permissions in build stage
COPY self-host/supervisord/*.ini /staging/etc/supervisor.d/
COPY self-host/supervisord/supervisord.conf /staging/etc/supervisor/supervisord.conf
# Remove default supervisord.conf to avoid conflicts
RUN rm -f /staging/etc/supervisord.conf
COPY self-host/nginx/nginx.conf /staging/etc/nginx/
COPY self-host/nginx/proxy.conf /staging/etc/nginx/
COPY self-host/nginx/mime.types /staging/etc/nginx/
COPY self-host/nginx/security-headers.conf /staging/etc/nginx/
COPY self-host/nginx/security-headers-ssl.conf /staging/etc/nginx/
COPY self-host/nginx/logrotate.sh /staging/
RUN chmod +x /staging/logrotate.sh

# Copy configuration templates
RUN mkdir -p /staging/etc/hbs
COPY self-host/hbs/nginx-config.hbs /staging/etc/hbs/
COPY self-host/hbs/config.yaml /staging/etc/hbs/

# Copy entrypoint script and make it executable
COPY self-host/entrypoint.sh /staging/entrypoint.sh
RUN chmod +x /staging/entrypoint.sh

###############################################
#                  App stage                  #
###############################################
FROM mcr.microsoft.com/dotnet/aspnet:9.0-azurelinux3.0-distroless
ARG TARGETPLATFORM
LABEL com.bitwarden.product="bitwarden"
LABEL com.bitwarden.project="passwordless"
ENV ASPNETCORE_ENVIRONMENT=Production
ENV BWP_ENABLE_ADMIN=true
ENV BWP_ENABLE_API=true
ENV BWP_DB_FILE_API="/etc/bitwarden_passwordless/api.db"
ENV BWP_DB_FILE_ADMIN="/etc/bitwarden_passwordless/admin.db"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV SelfHosted=true
ENV RP_PORT=5701

# Copy extracted packages and directory structure from build stage
COPY --from=build /staging /

# Copy all apps from dotnet-build stage
WORKDIR /app
COPY --from=build /app ./

# Copy hbs tool from build stage
COPY --from=build /app/hbs/hbs /usr/local/bin/hbs

VOLUME ["/etc/bitwarden_passwordless"]

WORKDIR /app

EXPOSE 5701

ENTRYPOINT ["/entrypoint.sh"]