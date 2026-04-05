# Underwriter Portal — User Guide

**Audience:** Insurance underwriters  
**URL:** `/underwriter`  
**Role required:** `Underwriter`

---

## Overview

The Underwriter Portal is where submissions from agents and brokers are reviewed, rated, and converted into quoted policies. Underwriters control the pricing and accept/decline decisions for every risk on the platform.

---

## Getting Started

### Logging In

1. Navigate to the platform login page (`/login`).
2. Enter your email address and password.
3. Click **Sign In**.
4. You are redirected automatically to `/underwriter/dashboard`.

---

## Dashboard (`/underwriter/dashboard`)

Your dashboard surfaces the information you need to manage your workload:

- **Queue Summary** — number of submissions by status (Submitted, In Review, Pending Decision)
- **Quotes Issued Today** — count and total premium
- **Expiring Quotes** — quotes approaching their expiry date that need action
- **Recently Assigned** — newest submissions assigned to you

---

## Submission Queue (`/underwriter/queue`)

The queue is your primary worklist. It lists all submissions assigned to you, ordered by priority (newest first by default).

### Submission Statuses

| Status | Meaning |
|--------|---------|
| Draft | Created by agent/broker, not yet submitted |
| Submitted | Sent by agent/broker, awaiting underwriter pick-up |
| In Review | Checked out by an underwriter |
| Approved | Decision made — quote issued |
| Declined | Decision made — submission rejected |

### Picking Up a Submission

1. Find the submission in the queue. Use the search bar or status filter to narrow results.
2. Click the submission row to open it.
3. Review all risk details provided by the agent/broker:
   - Insured information (name, contact, address)
   - Line of business and coverage requested
   - Any supporting notes
4. Click **Begin Review** — the submission moves to `In Review` status.

### Transitioning a Submission

Once you've reviewed the risk:

- **Approve** — moves the submission to `Approved` and unlocks quote issuance
- **Decline** — moves to `Declined`; the agent/broker is notified
- **Request More Info** — moves to a `Pending Info` holding state; add a note explaining what is needed

---

## Quotes (`/underwriter/quotes`)

### Issuing a Quote

After approving a submission:

1. Open the approved submission.
2. Click **Issue Quote**.
3. Review the auto-calculated premium from the rate engine. The system applies the rate table configured for the relevant line of business.
4. Adjust coverage terms if needed (effective date, expiry date, deductibles).
5. Click **Issue** — the quote is created in `Issued` status and the agent/broker is notified.

### Re-rating a Quote

If risk details changed or you want to test different coverage parameters:

1. Open the quote.
2. Click **Re-rate**.
3. Adjust the inputs and click **Calculate**.
4. Review the updated premium.
5. Click **Confirm Re-rate** to save — the quote version number increments.

### Rate Preview (Before Issuing)

You can preview a rating result without creating a quote:

1. On the **Issue Quote** screen, modify coverage parameters.
2. Click **Preview Rate** — the estimated premium appears without saving.

### Quote Statuses

| Status | Meaning |
|--------|---------|
| Draft | Created but not yet sent to agent |
| Issued | Sent to agent/broker awaiting their accept/decline |
| Accepted | Agent/broker confirmed on behalf of client |
| Declined | Agent/broker rejected the quote |
| Expired | Quote validity period elapsed without action |

### Transitioning a Quote

Use the action buttons on the quote detail screen:

- **Issue** — send the quote to the agent/broker
- **Expire** — manually expire a quote past its valid date
- Acceptance and Decline transitions are performed by the agent/broker

---

## Rate Tables

Rate tables define the premium calculation rules by line of business. Admins create and maintain rate tables; underwriters can view them to understand how premiums are derived.

1. Navigate to `/underwriter/quotes` and open any quote.
2. The **Rate Table Used** field shows which table was applied.
3. Click the table name to see the factor matrix (base rate, coverage multipliers, risk adjustments).

> To request a new rate table or modify an existing one, contact your platform Admin.

---

## Viewing Policies

Once a quote is accepted, it becomes a policy. You can view policies associated with your quotes:

1. Open the accepted quote.
2. Click **View Policy** — opens the policy detail.
3. From there you can attach endorsements or policy documents (PDF/Word, max 20 MB).

---

## Troubleshooting

| Issue | Resolution |
|-------|-----------|
| Cannot see a submission | Submissions are tenant-scoped. Confirm the submission is assigned to your tenant and to you specifically. |
| Premium calculation seems wrong | Check the Rate Table applied to the submission's line of business in Tenant Config (Admin). |
| Quote expired unexpectedly | Quote validity is set at issuance. Check the expiry date in the quote detail. |
| Cannot transition submission | Verify the submission is in a valid state for the intended transition (e.g., must be Submitted before In Review). |
