docker stop passwordless
docker rm passwordless
docker build . -f self-host/Dockerfile -t bitwarden/passwordless

docker run \
  --name passwordless \
  -p 5701:5701 \
  -e BWP_PORT=5701 \
  -e BWP_DOMAIN=example.local \
  -e BWP_ENABLE_SSL=true \
  -e BWP_ENABLE_SSL_CA=true \
  -e BWP_SSL_CA_CERT=ssl.crt \
  -v /your/path/passwordless-self-hosting:/etc/bitwarden_passwordless \
  bitwarden/passwordless