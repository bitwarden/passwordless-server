docker stop passwordless
docker rm passwordless
docker build . -f self-host/Dockerfile -t bitwarden/passwordless

docker run \
  --name passwordless \
  -p 5701:5701 \
  -e BWP_DOMAIN=example.com \
  -e BWP_ENABLE_SSL=false \
  -v /your/path/passwordless-self-hosting:/etc/bitwarden_passwordless \
  bitwarden/passwordless