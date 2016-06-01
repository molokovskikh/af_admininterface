#!/bin/sh

set -o errexit

npm install
bower install
bake packages:fix notInteractive=true
