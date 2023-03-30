#!/bin/bash

# see https://github.com/rroemhild/docker-test-openldap
docker pull rroemhild/test-openldap
docker run --rm -p 10389:10389 rroemhild/test-openldap

# LDAPS does not work because of the expired certificate!
# But this is there to test the app not ssl
