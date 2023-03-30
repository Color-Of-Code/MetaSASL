#!/bin/bash

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

# set variables
export SASL_CONFIGURATION_DIR=`realpath "$SCRIPT_DIR/"`
export SASL_SOCKET_FILE=`realpath "$SCRIPT_DIR/mux"`

pushd "$SCRIPT_DIR/../src/MetaSASL.Server"
dotnet run
popd
