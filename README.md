# HitRefresh.WebSuit

This project aims to enable MobileSuit as a Service over Web.

## Generate Key-Pairs

```bash
# Use RSA2048
openssl genpkey -algorithm RSA -out private_key.pem -pkeyopt rsa_keygen_bits:2048

openssl rsa -pubout -in private_key.pem -out public_key.pem
```