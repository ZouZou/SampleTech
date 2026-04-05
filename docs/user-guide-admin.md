# Admin Portal — User Guide

**Audience:** Platform administrators  
**URL:** `/admin`  
**Role required:** `Admin`

---

## Overview

The Admin Portal is the control center for the entire insurance platform. Admins manage users, configure tenant settings, and maintain the audit trail. Only users with the `Admin` role can access this portal.

---

## Getting Started

### Logging In

1. Navigate to the platform login page (`/login`).
2. Enter your email address and password.
3. Click **Sign In**.
4. Upon successful authentication you are redirected automatically to `/admin/dashboard`.

> If you see an "Access Denied" screen, your account may not have the Admin role. Contact your system owner.

---

## Dashboard (`/admin/dashboard`)

The dashboard provides a real-time system overview:

- **Active Users** — count of currently active user accounts across all roles
- **Policies in Force** — total active policies on the platform
- **Open Submissions** — submissions pending underwriter action
- **Recent Activity** — latest state changes across the platform

Use the dashboard as your daily health-check before diving into other tasks.

---

## User Management (`/admin/users`)

User Management is where you create accounts, assign roles, and disable access.

### Creating a New User

1. Click **New User** in the top-right corner.
2. Fill in:
   - **Full Name**
   - **Email Address** (used as login credential)
   - **Role** — select one of: `Admin`, `Underwriter`, `Agent`, `Broker`, `Client`
   - **Temporary Password** — the user will be prompted to change it on first login
3. Click **Create User**.
4. The new user receives an email with login instructions.

### Editing a User

1. Find the user in the table (search by name or email).
2. Click the row to open the user detail panel.
3. Update the fields you need and click **Save**.

### Disabling a User

1. Open the user detail panel.
2. Toggle **Active** to off.
3. Click **Save**.

Disabled users cannot log in. Their historical data (submissions, quotes, policies) is preserved.

### Role Reference

| Role | Portal Access | Key Permissions |
|------|--------------|-----------------|
| Admin | Admin Portal | Full system access |
| Underwriter | Underwriter Portal | Review submissions, issue/decline quotes |
| Agent | Agent Portal | Create clients, submit quotes |
| Broker | Broker Portal | Agency oversight, commissions |
| Client | Client Portal | View policies, file claims |

---

## Tenant Configuration (`/admin/tenant-config`)

Tenant configuration controls organization-level settings.

### Key Settings

- **Organization Name** — displayed on all documents and emails
- **Default Line of Business** — pre-populated on new submissions
- **Rate Table Assignment** — which rate tables are active for underwriting
- **Notification Preferences** — email notification triggers (new submission, quote issued, claim filed)

### Updating Settings

1. Modify the desired fields.
2. Click **Save Configuration**.

> Changes to rate table assignments take effect on new submissions only — existing submissions are not affected.

---

## Audit Log (`/admin/audit-log`)

The audit log records every significant action performed on the platform for compliance purposes.

### Reading the Log

Each log entry shows:

- **Timestamp** — exact date and time (UTC)
- **Actor** — the user who performed the action
- **Action** — what was done (e.g., `submission.transitioned`, `policy.document_uploaded`)
- **Entity** — the record affected (e.g., submission ID, policy number)
- **Details** — before/after values where applicable

### Filtering

Use the filter bar to narrow results by:

- Date range
- Actor (user email)
- Action type
- Entity type

### Exporting

Click **Export CSV** to download the filtered log for offline analysis or regulatory submission.

---

## Rate Table Management

Admins are the only role that can create and update rate tables used by underwriters during quoting.

> Detailed rate table management instructions are in the **Underwriter Portal guide**, section "Rate Tables." Admins access the same interface at the same URL.

---

## Troubleshooting

| Issue | Resolution |
|-------|-----------|
| User cannot log in | Check user is Active and their role is set correctly |
| Underwriter cannot see a submission | Verify the submission's tenant matches the underwriter's tenant |
| Rate table not applying | Confirm the rate table is Active and assigned to the correct line of business in Tenant Config |
| Audit log entry missing | Log entries are written asynchronously — allow up to 60 seconds; contact support if still missing |
