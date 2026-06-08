# Governance Report: Speedy IT Systems Landscape

**Prepared for:** Top Management
**Classification:** Internal – Confidential
**Scope:** Enterprise Systems, Infrastructure & Risk Assessment

---

## 1. Executive Summary

This report provides a structured overview of Speedy's current IT systems landscape as used and managed by Top Management. The organisation operates two primary platforms — a **Reporting System** and an **Existing Information System** — both of which are critical to day-to-day operations and strategic decision-making.

The overall architecture is functional and reasonably structured, with clear separation of concerns between data extraction, analysis, and reporting. However, the assessment identifies **notable concentration risks**, **single points of failure**, and **areas where governance controls should be strengthened** to ensure resilience, security, and long-term sustainability.

---

## 2. Key Systems Overview

### 2.1 Reporting System
The Reporting System supports Top Management's analytical and decision-making needs. It comprises three core components:

| Component | Technology | Infrastructure |
|---|---|---|
| Sales Data Extractor | Java | Linux Server (Application) |
| Analysis Data Manager | Java | Linux Server (Application) |
| Sales Report Generator | Java | Linux Server (Application) |
| Reporting Database | Oracle Database | Linux Server (Database) |

The system extracts sales data directly from the Sales ERP Module and processes it through an analytical pipeline, culminating in report generation for Top Management consumption.

### 2.2 Existing Information System
The Existing Information System serves as the operational backbone, hosting both Sales and HR business processes:

| Component | Platform | Access Methods |
|---|---|---|
| Sales ERP Module | Microsoft Dynamics 365 | Web-Based UI, REST API |
| HR ERP Module | Microsoft Dynamics 365 | Web-Based UI, REST API |

The system is accessible via a **Web-Based User Interface** for end users and a **REST API** for programmatic integration, enabling connectivity with the Reporting System.

### 2.3 Network & Security Infrastructure
A **Firewall** is in place to separate the **Corporate LAN** from the **Public Internet**, providing a fundamental perimeter security boundary. All application and database workloads reside on dedicated Linux servers within the corporate environment.

---

## 3. Dependencies & Risks

### 3.1 Critical Dependencies

```
Top Management
    ├── Reporting System ──────────────────────────────┐
    │       ├── Sales Data Extractor (Java)            │
    │       │       └── EXTRACTS FROM: Sales ERP ──────┤
    │       ├── Analysis Data Manager (Java)           │
    │       └── Sales Report Generator (Java)          │
    │               └── Oracle Database                │
    │                       └── Linux Server (DB)      │
    └── Existing Information System                    │
            ├── Sales ERP Module (Dynamics 365) ───────┘
            └── HR ERP Module (Dynamics 365)
```

### 3.2 Risk Register

| # | Risk Area | Description | Severity | Likelihood |
|---|---|---|---|---|
| R1 | **Single Platform Dependency** | Both ERP modules (Sales & HR) run exclusively on Microsoft Dynamics 365. A platform outage, licensing change, or vendor issue would simultaneously disrupt both modules. | High | Medium |
| R2 | **Single Server Concentration** | All three Reporting System components (Extractor, Analyser, Generator) run on the same Linux Application Server. A hardware or OS failure would take down the entire reporting pipeline. | High | Medium |
| R3 | **Database Single Point of Failure** | The Oracle Database operates on a single Linux Database Server with no redundancy referenced in the architecture. Loss of this server means total loss of reporting capability. | High | Medium |
| R4 | **Homogeneous Technology Stack** | All reporting components are built in Java. A critical Java vulnerability (e.g., a zero-day exploit) could simultaneously compromise all three components. | Medium | Low |
| R5 | **REST API Exposure Risk** | The Existing Information System is accessible via a REST API, which, if insufficiently secured, may expose sensitive Sales and HR data to unauthorised access or injection attacks. | High | Medium |
| R6 | **Firewall as Sole Perimeter Control** | The architecture references a single Firewall as the boundary between the Corporate LAN and the Public Internet. There is no mention of additional controls (e.g., DMZ, IDS/IPS, MFA). | High | Medium |
| R7 | **Data Pipeline Integrity** | The direct extraction relationship between the Sales Data Extractor and the Sales ERP Module introduces risk of data inconsistency or corruption if extraction processes are not properly governed. | Medium | Medium |
| R8 | **Limited Role Segregation Visibility** | Only Top Management roles are defined in the current governance model. There is no visibility of access controls, role-based permissions, or audit trails for other system users. | Medium | High |

---

## 4. Recommendations

### Priority 1 — Immediate Actions (0–3 Months)

**R1 – ERP Platform Resilience**
Ensure a formal **Business Continuity Plan (BCP)** and **Disaster Recovery (DR)** agreement is in place with Microsoft for Dynamics 365. Evaluate whether a fallback or data export mechanism exists in the event of platform unavailability.

**R3 & R2 – Eliminate Single Points of Failure**
Commission an infrastructure review to implement:
- **Server redundancy or clustering** for the Linux Application Server hosting reporting components.
- **Database replication or backup** for the Oracle Database, with a tested recovery time objective (RTO) defined and documented.

**R6 – Strengthen Perimeter Security**
Conduct an immediate security architecture review to assess whether the single Firewall is sufficient. Consider implementing:
- A **Demilitarised Zone (DMZ)** for API-facing services.
- **Intrusion Detection/Prevention Systems (IDS/IPS)**.
- **Multi-Factor Authentication (MFA)** for all management and ERP access.

---

### Priority 2 — Short-Term Actions (3–6 Months)

**R5 – Secure the REST API**
Commission a formal **API security audit**, ensuring:
- All endpoints are authenticated and authorised using current standards (e.g., OAuth 2.0).
- Rate limiting and logging are enfor