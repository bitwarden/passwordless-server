docker stop passwordless
docker rm passwordless
docker build . -f self-host/Dockerfile -t bitwarden/passwordless

docker run \
  --name passwordless \
  -p 5701:5701 \
  -e BWP_ENABLE_SSL=true \
  -e BWP_PORT=5701 \
  bitwarden/passwordless