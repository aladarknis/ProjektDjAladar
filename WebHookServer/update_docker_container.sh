#!/bin/sh

cd ../
OLD_BRANCH=`git branch | grep "^\*" | cut -c 3-`
git stash
git checkout master
git pull origin master
docker-compose up -d --build
git checkout $OLD_BRANCH
git stash pop

