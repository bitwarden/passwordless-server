docker stop pass
docker rm pass
docker build . -f self-host/Dockerfile -t bitwarden/passwordless
docker run --name pass -p 5701:5701 bitwarden/passwordless