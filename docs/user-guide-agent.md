# Agent Portal — User Guide

**Audience:** Insurance agents  
**URL:** `/agent`  
**Role required:** `Agent`

---

## Overview

The Agent Portal is where agents manage their client roster, submit new insurance quote requests to underwriters, and track those quotes through to policy issuance. Agents are the primary point of contact between clients and the underwriting team.

---

## Getting Started

### Logging In

1. Navigate to the platform login page (`/login`).
2. Enter your email address and password.
3. Click **Sign In**.
4. You are redirected automatically to `/agent/dashboard`.

---

## Dashboard (`/agent/dashboard`)

Your dashboard gives you a snapshot of your pipeline:

- **Total Clients** — number of clients you manage
- **Open Submissions** — submissions you've sent that are awaiting underwriter action
- **Quotes Awaiting Response** — issued quotes that need your accept/decline
- **Active Policies** — policies currently in force for your clients

---

## Managing Clients (`/agent/clients`)

Before you can submit a quote, you need a client (insured) record.

### Adding a New Client

1. Click **New Client**.
2. Fill in the client details:
   - **Full Name** (individual) or **Company Name** (commercial)
   - **Contact Email**
   - **Phone Number**
   - **Mailing Address**
3. Click **Save Client**.
4. The client appears in your client list and is now available for submissions.

### Editing a Client

1. Find the client in the list (search by name or email).
2. Click the client row.
3. Update fields as needed and click **Save**.

### Client Record Contents

A client record holds:

- Contact information
- Linked submissions and their statuses
- Active policies
- Claims history

---

## Submitting a Quote (`/agent/new-quote`)

Submitting a quote sends a risk to underwriting for review and pricing.

### Step-by-Step

1. Click **New Quote** in the navigation or on your dashboard.
2. **Select Client** — choose from your existing clients or create a new one inline.
3. **Line of Business** — select the coverage type (e.g., Commercial General Liability, Property).
4. **Coverage Details** — fill in the risk information fields. These vary by line of business but typically include:
   - Coverage limits requested
   - Effective date
   - Property details or business description
   - Any relevant risk history (prior claims, etc.)
5. **Add Notes** (optional) — any context for the underwriter.
6. Click **Submit to Underwriting**.

The submission is created in `Submitted` status. An underwriter will pick it up from their queue.

### Submission Statuses

| Status | Meaning |
|--------|---------|
| Draft | Started but not yet submitted |
| Submitted | Sent to underwriting — no action needed from you yet |
| In Review | An underwriter is actively reviewing it |
| Pending Info | Underwriter needs additional information — check notes |
| Approved | Underwriter approved the risk — a quote will follow |
| Declined | Underwriter declined the risk |

---

## Tracking Quote Status (`/agent/status`)

The Quote Status page shows all submissions and their associated quotes in one view.

### Responding to a Pending Info Request

1. Find the submission with status `Pending Info`.
2. Click **View Details**.
3. Read the underwriter's note explaining what's needed.
4. Click **Reply / Provide Info** and enter the requested details.
5. Click **Resubmit** — the submission returns to `Submitted` status.

### Accepting or Declining an Issued Quote

When an underwriter issues a quote, you'll see it appear in **Quotes Awaiting Response**.

1. Click the quote to open the detail view.
2. Review:
   - Premium amount
   - Coverage terms and limits
   - Effective and expiry dates
3. Discuss with your client before responding.
4. Click **Accept** to bind the quote (it becomes a policy) or **Decline** to reject it.

> Once accepted, the quote converts to a policy. The client will be notified and can view it in their Client Portal.

---

## Filing a Claim on Behalf of a Client

If a client reports a loss, you can file a claim from the Agent Portal:

1. Open the client record.
2. Navigate to **Policies** and open the relevant active policy.
3. Click **File Claim**.
4. Fill in:
   - **Date of Loss**
   - **Description of Incident**
   - **Estimated Loss Amount** (if known)
5. Click **Submit Claim**.

The claim is logged and routed to the underwriting team for review. The client can also see it in their Client Portal.

---

## Troubleshooting

| Issue | Resolution |
|-------|-----------|
| Client not appearing in submission form | Ensure the client record was saved successfully in `/agent/clients` |
| Submission stuck in Submitted for a long time | Check with the underwriting team — submissions are processed by priority |
| Quote expired before client responded | Contact your underwriter to re-issue; original submission data can be reused |
| Cannot file a claim | Claims require an active policy. Verify the policy is in Active status. |
