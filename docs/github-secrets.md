# GitHub Actions — Required Secrets

All secrets are configured at the repository level in
**Settings → Secrets and variables → Actions**.

No secret value is ever hard-coded in workflow files or application source.

---

## Currently Active (CI phase)

| Secret | Used in | Description |
|--------|---------|-------------|
| `GITHUB_TOKEN` | `cd-staging.yml` | Auto-injected by GitHub Actions. Grants write access to GHCR for image push. No manual setup required. |

---

## Required Before First Staging Deployment

| Secret | Used in | Description |
|--------|---------|-------------|
| `REGISTRY_USERNAME` | `cd-staging.yml` | Username for the container registry (if switching from GHCR to ECR/other). |
| `REGISTRY_PASSWORD` | `cd-staging.yml` | Password / access token for the container registry. |

### If using AWS ECR (recommended for production)

| Secret | Description |
|--------|-------------|
| `AWS_ACCESS_KEY_ID` | IAM key for ECR push; scope to `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`, `ecr:PutImage`, `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload`. |
| `AWS_SECRET_ACCESS_KEY` | Paired secret for the IAM key above. |
| `AWS_REGION` | e.g. `us-east-1` |
| `ECR_REGISTRY` | e.g. `123456789.dkr.ecr.us-east-1.amazonaws.com` |

---

## Future: Staging/Production Deploy

| Secret | Description |
|--------|-------------|
| `KUBECONFIG_STAGING` | Base64-encoded kubeconfig for the staging Kubernetes cluster (added when cluster is provisioned). |
| `KUBECONFIG_PROD` | Base64-encoded kubeconfig for the production cluster (added at prod go-live). |
| `DEPLOY_KEY_STAGING` | SSH deploy key if using ArgoCD / Flux GitOps pattern. |

---

## Secret Rotation Policy

- All long-lived secrets (IAM keys, deploy keys) must be rotated **every 90 days**.
- Rotation is tracked in the SOC 2 evidence log (Jira / Linear — TBD).
- Use IAM roles via OIDC (`aws-actions/configure-aws-credentials`) instead of static
  access keys as soon as the AWS account is set up — eliminates `AWS_ACCESS_KEY_ID`
  and `AWS_SECRET_ACCESS_KEY` entirely.

---

## OIDC (Preferred — no long-lived AWS credentials)

Replace static IAM key secrets with federated OIDC trust:

```yaml
- name: Configure AWS credentials (OIDC)
  uses: aws-actions/configure-aws-credentials@v4
  with:
    role-to-assume: arn:aws:iam::ACCOUNT_ID:role/github-actions-ecr-push
    aws-region: us-east-1
```

Required one-time setup: create an IAM OIDC identity provider for
`token.actions.githubusercontent.com` and a role that trusts it scoped to this repo.
See: <https://docs.github.com/en/actions/security-for-github-actions/security-hardening-your-deployments/configuring-openid-connect-in-amazon-web-services>
