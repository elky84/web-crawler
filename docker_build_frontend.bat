set DOCKER_REGISTRY=%1

cd Frontend

docker build -t %DOCKER_REGISTRY%/web-crawler-frontend .
docker push %DOCKER_REGISTRY%/web-crawler-frontend

cd ..