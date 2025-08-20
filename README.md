# Project Setup Instructions

Follow the steps below to initialize SSL and start the project using Docker.

## 1. Initialize SSL Certificates

Run the following command to generate and configure SSL certificates:

```bash
bash init-ssl.sh
```

## 2. Start Docker Services
Start the Docker containers in detached mode:

```bash
docker compose up -d
```