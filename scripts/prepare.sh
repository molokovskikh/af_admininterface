#!/bin/sh

npm install
bower install
bake packages:install notInteractive=true
bake packages:fix notInteractive=true
