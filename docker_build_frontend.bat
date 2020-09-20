set DOCKER_REGISTRY=%1

docker build -t %DOCKER_REGISTRY%/web-crawler-frontend -f ./Frontend/Dockerfile ./Frontend
docker push %DOCKER_REGISTRY%/web-crawler-frontend