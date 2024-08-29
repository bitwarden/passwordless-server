#!/bin/bash

# Note: it's CRUCIAL that this file has Unix line endings (LF) and not Windows line endings (CRLF).
# Otherwise, the Docker container will fail to start with a confusing error message (file not found).

# Set up user group
PGID="${PGID:-1000}"
addgroup --gid $PGID bitwarden

# Set up user
PUID="${PUID:-1000}"
adduser --no-create-home --shell /bin/bash --disabled-password --uid $PUID --gid $PGID --gecos "" bitwarden

mounted_dir='/etc/bitwarden_passwordless';

############
# Database #
############
export DatabaseProvider=$BWP_DB_PROVIDER

if [ "$BWP_DB_PROVIDER" = "mysql" ] || [ "$BWP_DB_PROVIDER" = "mariadb" ]; then
  MYSQL_CONNECTION_STRING_API="server=$BWP_DB_SERVER;port=${BWP_DB_PORT:-3306};database=$BWP_DB_DATABASE_API;user=$BWP_DB_USERNAME;password=$BWP_DB_PASSWORD"
  MYSQL_CONNECTION_STRING_ADMIN="server=$BWP_DB_SERVER;port=${BWP_DB_PORT:-3306};database=$BWP_DB_DATABASE_ADMIN;user=$BWP_DB_USERNAME;password=$BWP_DB_PASSWORD"
  export ConnectionStrings__mysql__api=${ConnectionStrings__mysql__api:-$MYSQL_CONNECTION_STRING_API}
  export ConnectionStrings__mysql__admin=${ConnectionStrings__mysql__admin:-$MYSQL_CONNECTION_STRING_ADMIN}
elif [ "$BWP_DB_PROVIDER" = "postgresql" ] || [ "$BWP_DB_PROVIDER" = "postgres" ]; then
  POSTGRESQL_CONNECTION_STRING_API="Host=$BWP_DB_SERVER;Port=${BWP_DB_PORT:-5432};Database=${BWP_DB_DATABASE_API:-Api};Username=$BWP_DB_USERNAME;Password=$BWP_DB_PASSWORD"
  POSTGRESQL_CONNECTION_STRING_ADMIN="Host=$BWP_DB_SERVER;Port=${BWP_DB_PORT:-5432};Database=${BWP_DB_DATABASE_ADMIN:-Admin};Username=$BWP_DB_USERNAME;Password=$BWP_DB_PASSWORD"
  export ConnectionStrings__postgresql__api=${ConnectionStrings__postgresql__api:-$POSTGRESQL_CONNECTION_STRING_API}
  export ConnectionStrings__postgresql__admin=${ConnectionStrings__postgresql__admin:-$POSTGRESQL_CONNECTION_STRING_ADMIN}
elif [ "$BWP_DB_PROVIDER" = "mssql" ] || [ "$BWP_DB_PROVIDER" = "sqlserver" ]; then
  MSSQL_CONNECTION_STRING_API="Server=$BWP_DB_SERVER,${BWP_DB_PORT:-1433};Database=${BWP_DB_DATABASE_API:-Api};User Id=${BWP_DB_USERNAME:-sa};Password=$BWP_DB_PASSWORD;Encrypt=True;TrustServerCertificate=True"
  MSSQL_CONNECTION_STRING_ADMIN="Server=$BWP_DB_SERVER,${BWP_DB_PORT:-1433};Database=${BWP_DB_DATABASE_ADMIN:-Admin};User Id=${BWP_DB_USERNAME:-sa};Password=$BWP_DB_PASSWORD;Encrypt=True;TrustServerCertificate=True"
  export ConnectionStrings__mssql__api=${ConnectionStrings__mssql__api:-$MSSQL_CONNECTION_STRING_API}
  export ConnectionStrings__mssql__admin=${ConnectionStrings__mssql__admin:-$MSSQL_CONNECTION_STRING_ADMIN}
else
  SQLITE_CONNECTION_STRING_API="Data Source=$BWP_DB_FILE_API;"
  SQLITE_CONNECTION_STRING_ADMIN="Data Source=$BWP_DB_FILE_ADMIN;"
  export ConnectionStrings__sqlite__api=${ConnectionStrings__sqlite__api:-$SQLITE_CONNECTION_STRING_API}
  export ConnectionStrings__sqlite__admin=${ConnectionStrings__sqlite__admin:-$SQLITE_CONNECTION_STRING_ADMIN}
fi

#########################
# Url #
#########################
if [ "$BWP_DOMAIN" != "localhost" ] && [ "$BWP_ENABLE_SSL" != "false" ]; then
  echo "[Configuration] WARNING: Set environment variable 'BWP_ENABLE_SSL' to 'true' when 'BWP_DOMAIN' is not 'localhost'.";
  echo "[Configuration] WARNING: WebAuthn requires SSL when not running on 'localhost'. This could result in unexpected behavior.";
fi

export Passwordless__ApiUrl="https://${BWP_DOMAIN}/api/"
export PasswordlessManagement__ApiUrl="https://${BWP_DOMAIN}/api/"

echo "[Configuration] API public: $PasswordlessManagement__ApiUrl";

##############################################
# Generate ApiKey, ApiSecret & ManagementKey #
##############################################
generate_random_hex() {
  # shellcheck disable=SC2046
  # shellcheck disable=SC2005
  echo $(openssl rand -hex 16)
}

mounted_config="$mounted_dir/config.json";

if [ ! -d "$mounted_dir" ]; then
  echo "WARNING: Using non-persistent storage! Use the '--mount' parameter to map a persistent directory to the container's directory '$mounted_dir'.";
  mkdir "$mounted_dir";
fi

if [ ! -f "$mounted_config" ]; then
  echo "WARNING: '$mounted_config' does not exist and is being generated.";
  echo "{}" > "$mounted_config";
fi

api_key=$(jq -r '.Passwordless.ApiKey' "$mounted_config")
api_secret=$(jq -r '.Passwordless.ApiSecret' "$mounted_config")
management_key=$(jq -r '.PasswordlessManagement.ManagementKey' "$mounted_config")
salt_token=$(jq -r '.SALT_TOKEN' "$mounted_config")

if [ "$api_key" == "null" ] || [ "$api_secret" == "null" ] || [ "$management_key" == "null" ] || [ "$salt_token" == "null" ]; then
  if [ "$api_key" == "null" ]; then
    api_key="replaceme:public:00000000000000000000000000000000"
  fi
  if [ "$api_secret" == "null" ]; then
    api_secret="replaceme:secret:00000000000000000000000000000000"
  fi
  if [ "$management_key" == "null" ]; then
    management_key=$(generate_random_hex)
  fi
  if [ "$salt_token" == "null" ]; then
      salt_token=$(dd if=/dev/urandom bs=32 count=1 2>/dev/null | base64)
  fi

  mounted_temp="$mounted_dir/temp.json";
  jq --arg api_key "$api_key" --arg api_secret "$api_secret" --arg management_key "$management_key" --arg salt_token "$salt_token" \
    '.Passwordless.ApiKey = $api_key | .Passwordless.ApiSecret = $api_secret | .PasswordlessManagement.ManagementKey = $management_key | .SALT_TOKEN = $salt_token' \
    "$mounted_config" > "$mounted_temp"
  mv "$mounted_temp" "$mounted_config"
fi

# Magic Links
export MagicLinks__NewAccountTimeout="0.00:00:00"

# Configure overrides for the admin console app
jq '.ApplicationOverrides.admin.IsRateLimitBypassEnabled = true | .ApplicationOverrides.admin.IsMagicLinkQuotaBypassEnabled = true' \
  "$mounted_config" > "$mounted_temp"
mv "$mounted_temp" "$mounted_config"

# Generate SSL certificates
if [ "$BWP_ENABLE_SSL" = "true" ] && [ ! -f /etc/bitwarden_passwordless/${BWP_SSL_KEY:-ssl.key} ]; then
  openssl req \
  -x509 \
  -newkey rsa:4096 \
  -sha256 \
  -nodes \
  -days 36500 \
  -keyout /etc/bitwarden_passwordless/${BWP_SSL_KEY:-ssl.key} \
  -out /etc/bitwarden_passwordless/${BWP_SSL_CERT:-ssl.crt} \
  -reqexts SAN \
  -extensions SAN \
  -config <(cat /usr/lib/ssl/openssl.cnf <(printf "[SAN]\nsubjectAltName=DNS:${BWP_DOMAIN}\nbasicConstraints=CA:true")) \
  -subj "/C=US/ST=California/L=Santa Barbara/O=Bitwarden Inc./OU=Bitwarden Passwordless/CN=${BWP_DOMAIN}"
fi

# Launch a loop to rotate nginx logs on a daily basis
/bin/sh -c "/logrotate.sh loop >/dev/null 2>&1 &"

/usr/local/bin/hbs

# Enable/Disable services
sed -i "s/autostart=true/autostart=${BWP_ENABLE_ADMIN}/" /etc/supervisor.d/admin.ini
sed -i "s/autostart=true/autostart=${BWP_ENABLE_API}/" /etc/supervisor.d/api.ini

chown -R $PUID:$PGID \
    /app \
    /etc/bitwarden_passwordless \
    /etc/nginx/http.d \
    /etc/supervisor \
    /etc/supervisor.d \
    /var/lib/nginx \
    /var/log \
    /var/run/nginx \
    /run

exec setpriv --reuid=$PUID --regid=$PGID --init-groups /usr/bin/supervisord