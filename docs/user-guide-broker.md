# Broker Portal — User Guide

**Audience:** Insurance brokers  
**URL:** `/broker`  
**Role required:** `Broker`

---

## Overview

The Broker Portal gives brokers a high-level view of their entire book of business. Brokers oversee multiple agencies and agents, track portfolio health, monitor commissions, and analyze business performance. Brokers can also submit quotes directly, just like agents.

---

## Getting Started

### Logging In

1. Navigate to the platform login page (`/login`).
2. Enter your email address and password.
3. Click **Sign In**.
4. You are redirected automatically to `/broker/dashboard`.

---

## Dashboard (`/broker/dashboard`)

The broker dashboard aggregates KPIs across your whole book:

- **Total Premium Written** — gross written premium across all active policies
- **Active Policies** — count of in-force policies in your portfolio
- **Open Submissions** — pending submissions across all your agencies
- **Commissions YTD** — total commission earned year-to-date
- **Agency Performance Snapshot** — top and bottom performing agencies by premium volume

---

## Agency Overview (`/broker/agencies`)

This view shows the agencies operating under your brokerage.

### What You Can See

For each agency:

- **Agency Name** and lead agent
- **Policies Written** (current period vs. prior period)
- **Premium Volume** — total premium across active policies
- **Open Submissions** — submissions currently in the underwriting pipeline
- **Quote Acceptance Rate** — percentage of issued quotes that were accepted

### Drilling Into an Agency

Click an agency row to see:

- Individual agent performance within that agency
- Submission list for that agency (with status filters)
- Policies bound by that agency

Use this view during performance reviews or when investigating production trends.

---

## Portfolio Dashboard (`/broker/portfolio`)

The Portfolio Dashboard gives you analytics across your entire book of business.

### Metrics Available

- **Premium by Line of Business** — pie/bar chart of your portfolio mix
- **Policies by Status** — active, expired, and pending renewal
- **Renewal Pipeline** — policies expiring in the next 30/60/90 days
- **Claims Ratio** — claims filed vs. premium written (loss ratio indicator)

### Filters

Use the filter bar to narrow analysis by:

- Agency
- Line of Business
- Time period (month, quarter, year)

> The renewal pipeline view is especially important: flag expiring policies early so agents can reach out to clients before coverage lapses.

---

## Commission Summary (`/broker/commissions`)

The Commission Summary tracks your earnings.

### What's Shown

- **Commission Earned** — by policy and by period
- **Pending Commissions** — on policies not yet in Active status
- **Commission by Agency** — breakdown of which agencies are driving earnings
- **Monthly Trend** — chart showing commission earned over time

### Interpreting Commission Entries

| Status | Meaning |
|--------|---------|
| Pending | Policy is bound but not yet active or payment not confirmed |
| Earned | Policy is active; commission is confirmed |
| Reversed | Policy was cancelled; commission clawed back |

> Commission schedules are configured by your Admin. If a commission amount looks incorrect, contact your platform administrator.

---

## Submitting a Quote as a Broker

Brokers can submit quotes directly to underwriting, same as agents.

1. Navigate to the **Agency Overview** and open the appropriate agency, or use the global **New Quote** button.
2. Follow the same quote submission steps as in the Agent Portal:
   - Select or create a client (insured)
   - Choose line of business
   - Fill in coverage details
   - Submit to underwriting
3. The submission appears in your agency's submission list and in the underwriter's queue.

For full step-by-step instructions, see the [Agent Portal User Guide](user-guide-agent.md), section "Submitting a Quote."

---

## Filing a Claim

Brokers can file claims on behalf of clients against active policies:

1. Navigate to **Portfolio Dashboard** and find the relevant policy.
2. Open the policy and click **File Claim**.
3. Provide loss date, description, and estimated amount.
4. Click **Submit Claim**.

---

## Troubleshooting

| Issue | Resolution |
|-------|-----------|
| Agency not appearing in overview | Agencies are linked to your broker account by an Admin. Contact your Admin to verify the association. |
| Commission amounts seem incorrect | Commission schedules are configured in Tenant Config. Contact your Admin to review the setup. |
| Portfolio data not updating | Data refreshes on page load. Use the browser refresh or the **Refresh** button. If the issue persists, contact support. |
| Cannot see an agent's submissions | Confirm the agent is assigned to an agency under your brokerage in the User Management screen (Admin). |
