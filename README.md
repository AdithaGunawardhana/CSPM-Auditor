# 🛡️ Nexus CSPM: Multi-Cloud Security & SOAR Engine

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![AWS](https://img.shields.io/badge/AWS-232F3E?style=for-the-badge&logo=amazon-aws&logoColor=white)
![Kubernetes](https://img.shields.io/badge/kubernetes-%23326ce5.svg?style=for-the-badge&logo=kubernetes&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Firebase](https://img.shields.io/badge/Firebase-FFCA28?style=for-the-badge&logo=firebase&logoColor=white)

**Nexus** is an enterprise-grade Cloud Security Posture Management (CSPM) and SOAR engine. Built on a strict C# Dependency Injection architecture, it abstracts cloud APIs to actively detect, prioritize, and automatically remediate critical infrastructure misconfigurations across **AWS, Azure, Google Cloud Platform (GCP), and Kubernetes**.

Unlike traditional security tools that generate alert fatigue, Nexus features a live **Security Orchestration, Automation, and Response (SOAR)** module. It actively reaches into the cloud and patches vulnerabilities in real-time. It also features a **Generative AI Analysis Engine** and a **Shift-Left IaC Pre-Flight Scanner** to catch vulnerabilities before they reach production.

## ✨ Core Features

* **Fault-Tolerant Multi-Cloud Scanner:** Utilizes a scalable `ICloudScanner` C# interface. The engine is built with graceful degradation—if specific cloud credentials (like AWS) are missing from the Docker environment, the engine safely logs the error and continues scanning Azure, GCP, and K8s without interrupting the audit pipeline.
* **Identity Governance & Administration (IGA):** Decoupled, serverless RBAC powered by **Firebase Firestore**. Default sessions are strictly gated to Viewer Mode. SOAR capabilities are dynamically unlocked only after a corporate security officer approves an access request via the dedicated **DxO Governance Portal**.
* **Generative AI Threat Intel:** Built-in LLM integration that contextualizes threat vectors and generates prescriptive remediation snippets directly within the dashboard.
* **Shift-Left IaC Pre-Flight Scanning:** A drag-and-drop static analyzer that parses raw Terraform (`.tf`) and Kubernetes (`.yaml`) manifests in-memory to prevent misconfigurations from entering deployment pipelines.
* **Automated SOAR Remediation:** One-click execution of SDK payloads to actively revoke over-permissive IAM policies and close vulnerable network security groups on live cloud environments.
* **Premium Cyber UI/UX:** A custom, dark-mode SPA built with Tailwind CSS. Native browser alerts have been replaced with immersive glass-morphic modals and dynamic toast notifications. Features smart avatar handling that seamlessly adapts between Google SSO and standard Email/Password authentication.
* **Containerized Deployment:** Fully packaged via Docker and Docker Compose, bundling the .NET 8 backend, static frontend, and mapped persistent volumes for immutable SQLite audit logging.

## 🏗️ Technical Stack

* **Backend Engine:** C# / ASP.NET Core 8 (Minimal APIs)
* **Database (Logging):** Entity Framework Core 8 & SQLite (Persistent Docker Volumes)
* **Deployment:** Multi-stage Docker & Docker Compose
* **Identity & Governance:** Firebase Auth + Firebase Firestore (NoSQL)
* **Frontend UI:** HTML5, Vanilla JavaScript, Tailwind CSS, Chart.js

## 🚀 Getting Started

### Prerequisites
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed locally.
* A [Firebase](https://firebase.google.com/) Project with Auth and Firestore enabled.

### Installation & Setup

1. **Clone the repository**
   ```bash
   git clone [https://github.com/AdithaGunawardhana/CSPM-Auditor.git](https://github.com/AdithaGunawardhana/CSPM-Auditor.git)
   cd CSPM-Auditor