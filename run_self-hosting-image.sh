docker stop passwordless
docker rm passwordless
docker build . -f self-host/Dockerfile -t bitwarden/passwordless

# For Cloudflare tunnel
docker run \
  --name passwordless \
  -p 5701:5701 \
  -e BWP_DOMAIN=yourdomain.com \
  -e BWP_ENABLE_SSL=false \
  -v /your/path/passwordless-self-hosting:/etc/bitwarden_passwordless \
  bitwarden/passwordless