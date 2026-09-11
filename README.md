# 🛡️ Nexus CSPM (Cloud Security Posture Management)

![Version](https://img.shields.io/badge/version-4.0-amber)
![.NET Core](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?logo=docker)
![License](https://img.shields.io/badge/license-MIT-green)

Nexus is an enterprise-grade, **Multi-Tenant Cloud Security Posture Management (CSPM)** platform built with **ASP.NET Core 8**. It actively audits cloud infrastructure to detect dangerous misconfigurations and features a **SOAR** (Security Orchestration, Automation, and Response) module to actively patch vulnerabilities across AWS, Azure, GCP, Oracle Cloud (OCI), and Kubernetes.

## ✨ v4.0 Major Updates: The Enterprise SaaS Release
* **True Multi-Tenancy (Data Isolation):** Implemented strict `X-Tenant-ID` header injections. Entity Framework Core strictly isolates SQLite queries, ensuring users only see their own perfectly sandboxed cloud infrastructure, threats, and remediation histories.
* **Oracle Cloud Integration:** Expanded the `ICloudScanner` engine to support Oracle Cloud (OCI) scanning and remediation.
* **Self-Healing Database Initialization:** Docker containers now aggressively rebuild and migrate schema updates on startup to prevent volume caching conflicts.
* **Realistic Telemetry Simulation:** The dashboard now utilizes a "Random Walk" algorithm to generate realistic, plateauing historical threat data in real-time.

## 🏗️ Core Architecture & Tech Stack

* **Backend Engine:** C# ASP.NET Core 8 (Minimal APIs, Dependency Injection)
* **Database:** Entity Framework Core (SQLite) with multi-tenant query isolation
* **Identity Governance (IGA):** Firebase Authentication (Google SSO) & Serverless Firestore NoSQL
* **Frontend UI:** Vanilla JavaScript, Tailwind CSS, Chart.js
* **Deployment:** Docker & Docker Compose

## 🚀 Key Features

* **Multi-Cloud SOAR:** 1-click automated remediation (e.g., revoking open SSH ingress rules, removing wildcard IAM admins) across 5 distinct cloud layers.
* **Shift-Left IaC Pre-Flight:** Drag-and-drop static analyzer for Terraform (`.tf`) and Kubernetes (`.yaml`) manifests to catch flaws before deployment.
* **Generative AI Threat Intel:** Integrated LLM translates complex threat vectors into actionable context for junior security analysts.
* **Strict RBAC Approval Pipeline:** New users default to Viewer status. SOAR capabilities are locked behind an external Corporate Governance Portal (`dxo-portal.html`) requiring manual approval.

## 🛠️ Getting Started

### Prerequisites
* Docker Desktop
* A Firebase Project (with Google Auth and Firestore enabled)

### Local Deployment
1. Clone the repository.
2. Ensure Docker Desktop is running.
3. Start the engine and trigger the self-healing database initialization:
   ```bash
   docker compose up --build -d