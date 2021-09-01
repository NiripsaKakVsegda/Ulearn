#!/bin/sh
docker build -t python-visualizer-api .
docker rm python-visualizer-api -f
docker rm python-visualizer-api-proxy -f
docker-compose up --detach