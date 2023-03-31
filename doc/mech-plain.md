# PLAIN Mechanism

Ref: [rfc4616](./rfc/rfc4616.txt)

## Overview

The PLAIN mechanism allows a client to authenticate using a simple clear-text user/password.

**NB**: To be used:

* in combination with data confidentiality services provided by a lower layer!
* in protocols lacking a simple password authentication command.

## Specification

* Name: `PLAIN`
* Security layer: None (should only be used when adequate security services have been established)
* Capabilities:
  * transfer an authorization identity string (UTF-8 encoded). In client request,
    * empty:     act as the identity(client credentials)
    * non-empty: act as the identity represented by the string.

## Authentication Sequence

The mechanism consists of a single message, a UTF-8 string, from the client to the server.

```scheme
   message   = [authzid] UTF8NUL authcid UTF8NUL passwd
   authcid   = 1*SAFE ; MUST accept up to 255 octets
   authzid   = 1*SAFE ; MUST accept up to 255 octets
   passwd    = 1*SAFE ; MUST accept up to 255 octets
   UTF8NUL   = %x00 ; UTF-8 encoded NUL character

   SAFE      = UTF1 / UTF2 / UTF3 / UTF4
               ;; any UTF-8 encoded Unicode character except NUL

   UTF1      = %x01-7F ;; except NUL
   UTF2      = %xC2-DF UTF0
   UTF3      = %xE0 %xA0-BF UTF0 / %xE1-EC 2(UTF0) /
               %xED %x80-9F UTF0 / %xEE-EF 2(UTF0)
   UTF4      = %xF0 %x90-BF 2(UTF0) / %xF1-F3 3(UTF0) /
               %xF4 %x80-8F 2(UTF0)
   UTF0      = %x80-BF
```

The form of

* `authzid` is specific to the application-level protocol's SASL profile
* `authcid` and `passwd` productions are form-free

Server verifies (both must succeed):

1. `authcid` and `passwd` against the system authentication database
2. authentication credentials permit the client to act as the (presented or derived) `authzid`

The presented authentication identity and password strings, as well as the database authentication identity and password strings, are to be prepared before being used in the verification process. The [SASLPrep](./rfc/rfc4013.txt) profile of the [StringPrep](./rfc/rfc3454.txt) algorithm is the **RECOMMENDED** preparation algorithm.

When preparing the presented strings using [SASLPrep](./rfc/rfc4013.txt), the presented strings are to be treated as "query" strings (Section 7 of
[StringPrep](./rfc/rfc3454.txt)) (unassigned code points are allowed to appear in their prepared output).

When preparing the database strings using [SASLPrep](./rfc/rfc4013.txt), the database strings are to be treated as "stored" strings (Section 7 of [StringPrep](./rfc/rfc3454.txt)) (unassigned code points are prohibited from appearing in their prepared output).

Regardless of the preparation algorithm used, if the output of a non-invertible function (e.g., hash) of the expected string is stored, the string **MUST** be prepared before input to that function.

Regardless of the preparation algorithm used, if preparation fails or results in an empty string, verification **SHALL** fail.

When no authorization identity is provided, the server derives an authorization identity from the prepared representation of the provided authentication identity string. This ensures that the derivation of different representations of the authentication identity produces the same authorization identity.

The server **MAY** use the credentials to initialize any new authentication database, such as one suitable for [CRAM-MD5] or [DIGEST-MD5].

## Examples

### Example 1

User authentication.

```
S: * ACAP (SASL "CRAM-MD5") (STARTTLS)
C: a001 STARTTLS
S: a001 OK "Begin TLS negotiation now"
<TLS negotiation, further commands are under TLS layer>
S: * ACAP (SASL "CRAM-MD5" "PLAIN")
C: a002 AUTHENTICATE "PLAIN"
S: + ""
C: {21}
C: <NUL>tim<NUL>tanstaaftanstaaf
S: a002 OK "Authenticated"
```

### Example 2

Used to attempt to assume the identity of another user. (Here, the server rejects the request)

```
S: * ACAP (SASL "CRAM-MD5") (STARTTLS)
C: a001 STARTTLS
S: a001 OK "Begin TLS negotiation now"
<TLS negotiation, further commands are under TLS layer>
S: * ACAP (SASL "CRAM-MD5" "PLAIN")
C: a002 AUTHENTICATE "PLAIN" {20+}
C: Ursel<NUL>Kurt<NUL>xipj3plmq
S: a002 NO "Not authorized to requested authorization identity"
```

## Pseudo-Code "verification"

Using hashed passwords and the SASLprep preparation function.

```php
   boolean Verify(string authzid, string authcid, string passwd) {
     string pAuthcid = SASLprep(authcid, true);  # prepare authcid
     string pPasswd  = SASLprep(passwd,  true);  # prepare passwd
     if (pAuthcid == NULL || pPasswd == NULL) {
       return false;     # preparation failed
     }
     if (pAuthcid == "" || pPasswd == "") {
       return false;     # empty prepared string
     }

     storedHash = FetchPasswordHash(pAuthcid);
     if (storedHash == NULL || storedHash == "") {
       return false;     # error or unknown authcid
     }

     if (!Compare(storedHash, Hash(pPasswd))) {
       return false;     # incorrect password
     }

     if (authzid == NULL ) {
       authzid = DeriveAuthzid(pAuthcid);
       if (authzid == NULL || authzid == "") {
           return false; # could not derive authzid
       }
     }

     if (!Authorize(pAuthcid, authzid)) {
       return false;     # not authorized
     }

     return true;
   }
```

The second parameter of the `SASLprep`:
* `true`: unassigned code points are allowed in the input
* `false`: when SASLprep function is called to prepare the password prior to computing the stored hash

The second parameter provided to the `Authorize` MAY need preparation: Cf. The application-level SASL profile.

**Note**:

* `DeriveAuthzid` and `Authorize` require knowledge and understanding of mechanism and the application-level protocol specification and/or implementation details to implement.
* `Authorize` outcome is clearly dependent on details of the local authorization model and policy. Both functions may be dependent on other factors as well.

##  Security

The server gains the ability to impersonate the user to all services with the same password regardless of any encryption provided by TLS or other confidentiality protection mechanisms.

Clients are encouraged to have an operational mode where all mechanisms that are likely to reveal the user's password to the server are disabled.

General [SASL](./rfc/rfc4422.txt) security considerations apply to this mechanism.

Unicode, [UTF-8](./rfc/rfc3629.txt), and [StringPrep](./rfc/rfc3454.txt) security considerations also apply.

## References

* [ACAP](./rfc/rfc2244.txt) "ACAP -- Application Configuration Access Protocol", RFC 2244, (November 1997)
* [CRAM-MD5] "The CRAM-MD5 SASL Mechanism", Work in Progress, June 2006.
* [DIGEST-MD5] "Using Digest Authentication as a SASL Mechanism", Work in Progress, June 2006.
* [IANA-SASL](./iana/sasl-mechanisms.txt) SIMPLE AUTHENTICATION AND SECURITY LAYER (SASL) MECHANISMS <http://www.iana.org/assignments/sasl-mechanisms>
* [SMTP-AUTH] "SMTP Service Extension for Authentication", RFC 2554, March 1999.
