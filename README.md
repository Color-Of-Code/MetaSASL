# MetaSASL

SASL authentication server implementation solving a "meta" setup in a simple way.

Instead of setting up a [meta openldap server](https://ltb-project.org/documentation/sasl_delegation.html#pass-through-authentication-on-several-ldap-directories-with-openldap-meta-backend)
for various backends, you can directly use this server and configure the multiple backends directly, thus removing several layers of complexity.

The MetaSASL.Server handles the PLAIN mechanism towards a SASL service and can act as a drop in replacement for `saslauthd`.

The MetaSASL.Server adds the feature to be able to safely handle multiple backends for authentication (at the moment `ldap` services only)

## Server configuration

Two files are needed to configure the server:

* `.secrets.yaml`: file containing the passwords for the realms (keep this secret)
* `sasl.yaml`: configuration of the realms

Two environment variables are used as well:

* `SASL_CONFIGURATION_DIR`: specifies the direction where the config files reside
* `SASL_SOCKET_FILE`: specifies the path to the unix socket to use for communicating

## Testing

### Prerequisites

`docker` and `testsaslauthd` are the tools needed to test the setup.

```bash
sudo apt install sasl2-bin
```

### Get OpenLdap up and running

Start a demo openldap server (listening on 10389 and 10636) inside one shell with:

```bash
bash test/start_open_ldap_server.sh
```

### Start the sasl server

Start the sasl server (from within a shell inside the dev container):

```bash
bash test/start_sasl_server.sh
```

The SASL server is configured to filter on the crew members (exclude admins, see `test/sasl.yaml` contents)

### Test with testsaslauthd

Once both services are running you should be able to test the authentication with `testsaslauthd`:

```bash
# Should fail admin group: (professor, zoidberg, hermes)
testsaslauthd -f test/mux -u professor -p professor -r futurama -s ldap

# Should success crew group: (fry, leela, bender)
testsaslauthd -f test/mux -u fry       -p fry       -r futurama -s ldap
```

This proves the setup can be used as a drop in replacement for a `saslauthd` setup, since we use
the `testsaslauthd` which is from the `sasl2-bin` package and not part of this repository.

It is enabling multiple realms easily, but at the moment you are constrained to use ldap services only as a backend.
