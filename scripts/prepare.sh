#!/bin/sh

npm install
bower install
bake packages notInteractive=true
