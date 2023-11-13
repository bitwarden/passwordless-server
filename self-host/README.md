# Self-hosting

Follow these instructions to run Passwordless.dev in a self-hosted environment.

## Build the image

1. Clone the repository
2. Set your working directory to match the root of the cloned files, for example:
   `~/src/passwordless-server`
3. In your terminal execute:
    ```bash
    docker build -t bitwarden/passwordless . -f self-host/Dockerfile
    ```
   
## Run the image

1. In your terminal execute:
    ```bash
    docker run -d --name passwordless -p 5701:5701 bitwarden/passwordless
    ```