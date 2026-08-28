# 🛡️ Nexus CSPM: Multi-Cloud Security & SOAR Engine

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![AWS](https://img.shields.io/badge/AWS-232F3E?style=for-the-badge&logo=amazon-aws&logoColor=white)
![TailwindCSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)
![Firebase](https://img.shields.io/badge/Firebase-FFCA28?style=for-the-badge&logo=firebase&logoColor=white)

**Nexus** is an enterprise-grade Cloud Security Posture Management (CSPM) and SOAR engine. Built on a strict C# Dependency Injection architecture, it abstracts cloud APIs to actively detect, prioritize, and automatically remediate critical infrastructure misconfigurations across **AWS, Azure, and Google Cloud Platform (GCP)**.

Unlike traditional security tools that simply generate alert fatigue, Nexus features a live **Security Orchestration, Automation, and Response (SOAR)** module. It actively reaches into the cloud and patches vulnerabilities like open SSH ports or wildcard IAM admins in real-time.

## ✨ Core Features

* **Multi-Cloud Interface Architecture:** Utilizes a scalable `ICloudScanner` C# interface registered via ASP.NET Core Dependency Injection. This normalizes telemetry from AWS (IAM/EC2), Azure (Resource Graph), and GCP (Asset Inventory) into a single, unified pipeline.
* **Automated SOAR Remediation:** One-click execution of SDK payloads to actively revoke over-permissive IAM policies and close vulnerable network security groups on live cloud environments.
* **Cyber Command Center UI:** A premium, dark-mode Single Page Application (SPA) built with Tailwind CSS. Features dynamic Bento Box grids, volumetric Chart.js data visualizations, and an animated live-terminal UI.
* **Firebase Google Authentication:** Secure user sessions and workspace rendering authenticated via Firebase Google Sign-In.
* **Immutable Audit Logging:** Historical findings and remediation events are permanently persisted locally using Entity Framework Core and SQLite for compliance readiness.
* **Webhook Routing:** Real-time JSON payloads designed to route critical alerts to Slack, Teams, or PagerDuty the millisecond a threat is detected.

## 🏗️ Technical Stack

* **Backend Engine:** C# / ASP.NET Core 8 (Minimal APIs)
* **Cloud Integration:** AWS SDK for .NET (Live execution) | Azure & GCP SDKs (Architectural simulation)
* **Database (ORM):** Entity Framework Core & SQLite
* **Frontend Authentication:** Firebase Auth
* **Frontend UI:** HTML5, Vanilla JavaScript, Tailwind CSS (CDN), Chart.js

## 🚀 Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) installed locally.
* [AWS CLI](https://aws.amazon.com/cli/) configured with an IAM User provisioned with `SecurityAudit` and `AmazonEC2FullAccess` permissions (for active remediation).

### Installation & Setup

1. **Clone the repository**
   ```bash
   git clone [https://github.com/AdithaGunawardhana/nexus-cspm.git](https://github.com/AdithaGunawardhana/nexus-cspm.git)
   cd nexus-cspm
