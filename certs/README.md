## Generation of a Self Signed Certificate
Generation of a self-signed SSL certificate involves a simple 3-step procedure:

__STEP 1__: Create the server private key
```sh
openssl genrsa -out cert.key 2048
```
__STEP 2__: Create the certificate signing request (CSR)
```sh
openssl req -new -key cert.key -out cert.csr
```
__STEP 3__: Sign the certificate using the private key and CSR
```sh
openssl x509 -req -days 3650 -in cert.csr -signkey cert.key -out cert.crt
```
Congratulations! You now have a self-signed SSL certificate valid for 10 years.
