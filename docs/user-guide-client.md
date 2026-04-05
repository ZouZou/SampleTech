# Client Portal — User Guide

**Audience:** Policyholders (insurance clients)  
**URL:** `/client`  
**Role required:** `Client`

---

## Overview

The Client Portal lets you view your insurance policies, access policy documents, file claims, and contact support — all in one place. No calls or paperwork needed for routine tasks.

---

## Getting Started

### Logging In

1. Go to the platform login page (`/login`).
2. Enter the email address and password provided by your agent or broker.
3. Click **Sign In**.
4. You are taken directly to your dashboard at `/client/dashboard`.

> If you don't have login credentials, contact your insurance agent or broker. They will set up your account.

### Resetting Your Password

If you forget your password:

1. On the login page, click **Forgot Password**.
2. Enter your email address and click **Send Reset Link**.
3. Check your inbox for a password reset email.
4. Click the link in the email and follow the prompts to set a new password.

---

## Dashboard (`/client/dashboard`)

Your dashboard is the first thing you see after logging in. It shows:

- **Active Policies** — a summary of your current coverage
- **Upcoming Renewals** — policies expiring soon
- **Open Claims** — any claims you've filed that are still being processed
- **Recent Activity** — latest updates to your account

---

## Viewing Your Policies (`/client/policies`)

### Policy List

The **Policies** page shows all your insurance policies — active, expired, and pending.

Each row shows:

- **Policy Number**
- **Line of Business** (e.g., Property, Liability)
- **Status** — Active, Expired, or Pending
- **Coverage Period** — effective and expiry dates
- **Premium**

### Policy Detail (`/client/policies/:id`)

Click any policy to open its detail view. Here you'll find:

- **Coverage Summary** — what is and isn't covered
- **Limits and Deductibles** — the financial terms of your policy
- **Insured Details** — the name and address on the policy
- **Agent/Broker** — who manages this policy and their contact info

### Downloading Policy Documents

Policy documents (your certificate of insurance, policy wording, endorsements) are attached to your policy record.

1. Open the policy detail.
2. Scroll to the **Documents** section.
3. Click the document name to download it as a PDF.

> Keep your certificate of insurance handy — you may need it for third-party contract requirements or loan applications.

---

## Filing a Claim (`/client/policies/:id`)

If something happens that may be covered by your insurance, file a claim as soon as possible.

### Before You File

- Ensure you have the policy number handy (visible on the policy detail page).
- Gather basic information: date of the incident, a description of what happened, and an estimate of the loss if possible.

### Step-by-Step

1. Open the policy the claim relates to from your policy list.
2. Click **File a Claim**.
3. Fill in the claim form:
   - **Date of Loss** — the date the incident occurred
   - **Description** — a clear description of what happened and what was damaged or lost
   - **Estimated Loss Amount** — your best estimate (you can update this later)
4. Click **Submit Claim**.

You'll see the claim appear in your dashboard under **Open Claims**.

### What Happens Next

After you submit:

1. The claim is logged and assigned to the underwriting team for review.
2. You may receive a follow-up email or call requesting additional documentation (photos, receipts, police reports).
3. Once reviewed, the claim status will update to `Approved` or `Declined`.
4. You'll be notified by email at each major status change.

### Claim Statuses

| Status | Meaning |
|--------|---------|
| Submitted | Received and awaiting assignment |
| Under Review | Being evaluated by the claims team |
| Pending Documentation | Additional info required from you |
| Approved | Claim approved; settlement in progress |
| Declined | Claim was not covered under your policy |
| Closed | Fully resolved |

---

## Contacting Support (`/client/support`)

If you have a question that isn't answered here, or need to speak with someone:

1. Navigate to **Support** in the main menu.
2. Fill in the support form:
   - **Subject** — brief summary of your question
   - **Message** — full description of your inquiry
   - **Related Policy** (optional) — select the relevant policy if applicable
3. Click **Send**.

You will receive a confirmation email and a member of the support team will respond within 1 business day.

> For urgent matters (active incidents, immediate coverage questions), call the phone number shown on your policy documents.

---

## Frequently Asked Questions

**Q: I can see my policy but it shows "Expired." What do I do?**  
Contact your agent or broker to start the renewal process. Their contact information is shown on the policy detail page.

**Q: My claim status hasn't changed in several days.**  
Log in and check the claim detail for any notes from the claims team. If they've requested documentation, that will appear there. If no action is pending on your end, contact support.

**Q: I need to update my contact information.**  
Contact your agent or broker — they manage your insured record and can update it on your behalf.

**Q: I need proof of insurance quickly.**  
Open your active policy and download the Certificate of Insurance from the Documents section. This is available immediately, 24/7.
