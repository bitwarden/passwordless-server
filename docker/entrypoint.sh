#!/bin/bash

# Set up user group
PGID="${PGID:-1000}"
addgroup --gid $PGID bitwarden

# Set up user
PUID="${PUID:-1000}"
adduser --no-create-home --shell /bin/bash --disabled-password --uid $PUID --gid $PGID --gecos "" bitwarden

# Translate environment variables for application settings
MYSQL_CONNECTION_STRING_API="server=$BWP_DB_SERVER;port=${BWP_DB_PORT:-3306};database=$BWP_DB_DATABASE_API;user=$BWP_DB_USERNAME;password=$BWP_DB_PASSWORD"
POSTGRESQL_CONNECTION_STRING_API="Host=$BWP_DB_SERVER;Port=${BWP_DB_PORT:-5432};Database=$BWP_DB_DATABASE_API;Username=$BWP_DB_USERNAME;Password=$BWP_DB_PASSWORD"
MSSQL_CONNECTION_STRING_API="Server=$BWP_DB_SERVER,${BWP_DB_PORT:-1433};Database=$BWP_DB_DATABASE_API;User Id=$BWP_DB_USERNAME;Password=$BWP_DB_PASSWORD;Encrypt=True;TrustServerCertificate=True"
SQLITE_CONNECTION_STRING_API="Data Source=$BWP_DB_FILE_API;"

MYSQL_CONNECTION_STRING_ADMIN="server=$BWP_DB_SERVER;port=${BWP_DB_PORT:-3306};database=$BWP_DB_DATABASE_ADMIN;user=$BWP_DB_USERNAME;password=$BWP_DB_PASSWORD"
POSTGRESQL_CONNECTION_STRING_ADMIN="Host=$BWP_DB_SERVER;Port=${BWP_DB_PORT:-5432};Database=$BWP_DB_DATABASE_ADMIN;Username=$BWP_DB_USERNAME;Password=$BWP_DB_PASSWORD"
MSSQL_CONNECTION_STRING_ADMIN="Server=$BWP_DB_SERVER,${BWP_DB_PORT:-1433};Database=$BWP_DB_DATABASE_ADMIN;User Id=$BWP_DB_USERNAME;Password=$BWP_DB_PASSWORD;Encrypt=True;TrustServerCertificate=True"
SQLITE_CONNECTION_STRING_ADMIN="Data Source=$BWP_DB_FILE_ADMIN;"

export DatabaseProvider=$BWP_DB_PROVIDER

if [ "$BWP_DB_PROVIDER" = "mysql" -o "$BWP_DB_PROVIDER" = "mariadb" ]; then
  export ConnectionStrings__mysql__api=${ConnectionStrings__mysql__api:-$MYSQL_CONNECTION_STRING_API}
  export ConnectionStrings__mysql__admin=${ConnectionStrings__mysql__admin:-$MYSQL_CONNECTION_STRING_ADMIN}
elif [ "$BWP_DB_PROVIDER" = "postgresql" -o "$BWP_DB_PROVIDER" = "postgres" ]; then
  export ConnectionStrings__postgresql__api=${ConnectionStrings__postgresql__api:-$POSTGRESQL_CONNECTION_STRING_API}
  export ConnectionStrings__postgresql__admin=${ConnectionStrings__postgresql__admin:-$POSTGRESQL_CONNECTION_STRING_ADMIN}
elif [ "$BWP_DB_PROVIDER" = "mssql" -o "$BWP_DB_PROVIDER" = "sqlserver" ]; then
  export ConnectionStrings__mssql__api=${ConnectionStrings__mssql__api:-$MSSQL_CONNECTION_STRING_API}
  export ConnectionStrings__mssql__admin=${ConnectionStrings__mssql__admin:-$MSSQL_CONNECTION_STRING_ADMIN}
else
  export ConnectionStrings__sqlite__api=${ConnectionStrings__sqlite__api:-$SQLITE_CONNECTION_STRING_API}
  export ConnectionStrings__sqlite__admin=${ConnectionStrings__sqlite__admin:-$SQLITE_CONNECTION_STRING_ADMIN}
fi

# Generate SSL certificates
if [ "$BWP_ENABLE_SSL" = "true" -a ! -f /etc/bitwarden_passwordless/${BWP_SSL_KEY:-ssl.key} ]; then
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
  -config <(cat /usr/lib/ssl/openssl.cnf <(printf "[SAN]\nsubjectAltName=DNS:${BWP_DOMAIN:-localhost}\nbasicConstraints=CA:true")) \
  -subj "/C=US/ST=California/L=Santa Barbara/O=Bitwarden Inc./OU=Bitwarden Passwordless/CN=${BWP_DOMAIN:-localhost}"
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