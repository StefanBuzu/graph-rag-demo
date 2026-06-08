# Governance Report: Speedy – Systems & Infrastructure Overview

**Prepared for:** Top Management
**Prepared by:** Governance Consulting
**Classification:** Internal – Confidential

---

## 1. Executive Summary

This report provides a structured overview of the key information systems, infrastructure components, and associated governance risks relevant to Speedy's operating environment. Top Management currently relies on two primary systems — the **Reporting System** and the **Existing Information System** — to manage business operations and decision-making. While the architecture is functional and reasonably structured, this review identifies several **critical single points of failure, technology concentration risks, and network security considerations** that warrant immediate management attention. A set of prioritised recommendations is provided to strengthen resilience, security, and governance oversight.

---

## 2. Key Systems Overview

### 2.1 Reporting System
The Reporting System serves as the primary analytical and decision-support tool for Top Management. It comprises three tightly integrated components:

| Component | Technology | Infrastructure |
|---|---|---|
| Data Extraction Component | Java | Linux Server (Application) |
| Analysis Data Component | Java | Linux Server (Application) |
| Report Generation Component | Java | Linux Server (Application) |
| Underlying Database | Oracle Database | Linux Server (Database) |

All three processing components share a **single Linux Application Server** and depend on a **single Oracle Database instance** hosted on a dedicated Linux Database Server. Data is sourced directly from the Sales ERP Module within the Existing Information System.

---

### 2.2 Existing Information System
The Existing Information System supports core business operations through two ERP modules, both running on **Microsoft Dynamics 365**:

| Module | Platform | Access Methods |
|---|---|---|
| Sales ERP Module | Microsoft Dynamics 365 | Web-Based UI, REST API |
| HR ERP Module | Microsoft Dynamics 365 | Web-Based UI, REST API |

The system is accessible via a **Web-Based User Interface** for end users and a **REST API** for system integrations, providing flexibility but also broadening the potential attack surface.

---

### 2.3 Network & Security Infrastructure
A **Firewall** is in place to separate the **Corporate LAN** from the **Public Internet**, representing the primary boundary defence. No additional network segmentation layers are documented in the current knowledge graph.

---

## 3. Dependencies & Risks

### 3.1 Critical Single Points of Failure

> ⚠️ **HIGH RISK**

- **Linux Server (Application):** All three components of the Reporting System — Data Extraction, Analysis, and Report Generation — run on a single application server. Any unplanned outage, hardware failure, or security incident on this server would result in a **complete loss of reporting capability** for Top Management.
- **Oracle Database / Linux Server (Database):** The entire Reporting System depends on one database instance. There is no evidence of replication, clustering, or failover configuration in the current architecture.
- **Microsoft Dynamics 365:** Both the Sales and HR ERP modules share the same platform. A licensing issue, service outage, or platform-level failure would simultaneously impact both business-critical functions.

---

### 3.2 Technology Concentration Risk

> ⚠️ **MEDIUM–HIGH RISK**

- All reporting logic is implemented in **Java** across three separate components. While this provides consistency, it means a critical Java vulnerability (e.g., a zero-day exploit) could compromise the entire reporting pipeline simultaneously.
- **Microsoft Dynamics 365** represents a single-vendor dependency for all ERP functionality. Changes in licensing, support terms, or service availability directly impact core operations with limited short-term alternatives.

---

### 3.3 Network Security & Exposure

> ⚠️ **MEDIUM RISK**

- The **REST API** exposed by the Existing Information System provides programmatic access to both Sales and HR data. If insufficiently protected, this interface could be exploited by external or internal actors to extract sensitive business or personnel data.
- The architecture documents **only a single firewall** as the network boundary control. There is no indication of additional controls such as a **DMZ, intrusion detection/prevention systems (IDS/IPS), or network segmentation** between systems of different sensitivity levels (e.g., HR data vs. Sales data).
- The relationship between the Reporting System's Data Extraction Component and the Sales ERP Module represents a **cross-system data flow** that should be governed by formal data access controls and audit logging.

---

### 3.4 Governance & Accountability Gaps

> ⚠️ **MEDIUM RISK**

- Top Management is documented as both the **manager of Speedy** and a **direct user** of both key systems. While appropriate for oversight, this dual role suggests a potential lack of intermediate system ownership or delegated accountability for system governance, maintenance, and risk management.
- No **data owners, system custodians, or operational roles** are defined below the Top Management level in the current knowledge graph, creating an accountability gap for day-to-day system governance.

---

## 4. Recommendations

The following recommendations are prioritised by urgency and potential impact:

---

### 🔴 Priority 1 – Address Single Points of Failure *(Immediate)*

1. **Implement High Availability for the Linux Application Server.** Deploy a load-balanced or clustered application server environment to eliminate the single point of failure for all Reporting System components. Consider containerisation (e.g., Docker/Kubernetes) for improved resilience and deployment flexibility.

2. **Establish Oracle Database Redundancy.** Implement Oracle Data Guard or an equivalent replication/failover solution for the database server. Define and test a Recovery Time Objective (RTO) and Recovery Point Objective (RPO) appropriate to management reporting needs.

3. **Define a Business Continuity Plan (BCP) for Microsoft Dynamics 365.** Document and test contingency procedures in the event of an extended Dynamics 365 outage, including manual fallback processes for Sales and HR operations.

---

### 🟠 Priority 2 – Strengthen Network Security *(Short-term: 1–3 months)*

4. **Conduct a REST API Security Audit.** Review authentication mechanisms (e.g., OAuth 2.0), rate limiting, and access controls on the REST API. Ensure all API access is logged and monitored, particularly for HR data access which may carry regulatory obligations (e.g., GDPR).

5. **Introduce Network Segmentation.** Implement a DMZ or tiered network architecture to isolate the Reporting