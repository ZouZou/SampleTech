# Generated API Client

This directory contains the NSwag-generated TypeScript client for the InsurancePlatform API.

## Regenerating the client

```bash
npx nswag openapi2tsclient \
  /input:http://localhost:5000/openapi/v1.json \
  /output:src/api/insurance-platform-client.ts \
  /template:Angular \
  /injectHttpClient:true \
  /generateClientClasses:true \
  /generateClientInterfaces:true
```

Run this after any API changes to keep the client in sync.
