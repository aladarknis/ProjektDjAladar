#!/bin/sh

cd ../
git pull origin main
docker-compose up -d --build


