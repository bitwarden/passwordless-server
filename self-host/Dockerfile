# syntax = docker/dockerfile:1.2
###############################################
#                 Build stage                 #
###############################################
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS dotnet-build

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
    && rm -rf /var/lib/apt/lists/* 

# Copy csproj files as distinct layers
WORKDIR /source
COPY src/AdminConsole/*.csproj ./src/AdminConsole/
COPY src/Api/*.csproj ./src/Api/
COPY src/ApiHelpers/*.csproj ./src/ApiHelpers/
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
COPY src/ApiHelpers/. ./src/ApiHelpers/
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

###############################################
#                  App stage                  #
###############################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim
ARG TARGETPLATFORM
LABEL com.bitwarden.product="bitwarden"
LABEL com.bitwarden.project="passwordless"
ENV ASPNETCORE_ENVIRONMENT=Production
ENV BWP_ENABLE_ADMIN=true
ENV BWP_ENABLE_API=true
ENV BWP_DB_FILE_API="/etc/bitwarden_passwordless/api.db"
ENV BWP_DB_FILE_ADMIN="/etc/bitwarden_passwordless/admin.db"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV Passwordless_ApiUrl="http://localhost:5000"
ENV PasswordlessManagement_ApiUrl="http://localhost:5000"
ENV SelfHosted=true

# Add packages
RUN apt-get update && apt-get install -y \
    curl \
    nginx \
    openssl \
    supervisor \
    tzdata \
    unzip \
    jq \
    && rm -rf /var/lib/apt/lists/*

# Create required directories
RUN mkdir -p /etc/bitwarden_passwordless/data-protection
RUN mkdir -p /etc/bitwarden_passwordless/logs
RUN mkdir -p /etc/supervisor
RUN mkdir -p /etc/supervisor.d
RUN mkdir -p /var/log/bitwarden
RUN mkdir -p /var/log/bitwarden_passwordless
RUN mkdir -p /var/log/nginx/logs
RUN mkdir -p /etc/nginx/http.d
RUN mkdir -p /var/run/nginx
RUN mkdir -p /var/lib/nginx/tmp
RUN touch /var/run/nginx/nginx.pid
RUN mkdir -p /app

# Copy all apps from dotnet-build stage
WORKDIR /app
COPY --from=dotnet-build /app ./

# Set up supervisord
COPY self-host/supervisord/*.ini /etc/supervisor.d/
COPY self-host/supervisord/supervisord.conf /etc/supervisor/supervisord.conf
RUN rm -f /etc/supervisord.conf

# Set up nginx
COPY self-host/nginx/nginx.conf /etc/nginx
COPY self-host/nginx/proxy.conf /etc/nginx
COPY self-host/nginx/mime.types /etc/nginx
COPY self-host/nginx/security-headers.conf /etc/nginx
COPY self-host/nginx/security-headers-ssl.conf /etc/nginx
COPY self-host/nginx/logrotate.sh /
RUN chmod +x /logrotate.sh

# Copy configuration templates
COPY self-host/hbs/nginx-config.hbs /etc/hbs/
COPY self-host/hbs/config.yaml /etc/hbs/

# Download hbs tool for generating final configurations
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ] ; then curl -L --output hbs.zip https://github.com/bitwarden/Handlebars.conf/releases/download/v1.3.0/hbs_linux-x64.zip; fi
RUN if [ "$TARGETPLATFORM" = "linux/arm/v7" ] ; then curl -L --output hbs.zip https://github.com/bitwarden/Handlebars.conf/releases/download/v1.3.0/hbs_linux-armv7.zip; fi
RUN if [ "$TARGETPLATFORM" = "linux/arm64" ] ; then curl -L --output hbs.zip https://github.com/bitwarden/Handlebars.conf/releases/download/v1.3.0/hbs_linux-arm64.zip; fi

# Extract hbs
RUN unzip hbs.zip -d /usr/local/bin && rm hbs.zip
RUN chmod +x /usr/local/bin/hbs

# Copy entrypoint script and make it executable
COPY self-host/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

VOLUME ["/etc/bitwarden_passwordless"]

WORKDIR /app

EXPOSE 5701

ENTRYPOINT ["/entrypoint.sh"]
