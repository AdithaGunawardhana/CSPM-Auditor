# 🛡️ Serverless Cloud Security Posture Management (CSPM) Engine

![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)
![AWS](https://img.shields.io/badge/AWS-SDK-orange.svg)
![SQLite](https://img.shields.io/badge/SQLite-Database-lightgrey.svg)
![TailwindCSS](https://img.shields.io/badge/Tailwind-CSS-06B6D4.svg)

A custom-built, lightweight Cloud Security Posture Management (CSPM) tool designed to programmatically audit AWS environments for critical security misconfigurations. 

Built with a **C# .NET Minimal API** backend and a **Tailwind CSS** frontend, this engine evaluates cloud resources against security best practices (such as IAM Least Privilege and S3 Public Access Blocks) and provides a real-time, interactive dashboard of organizational risk.

## ✨ Key Features

* **Automated AWS Auditing:** Programmatically scans AWS Identity and Access Management (IAM) and Simple Storage Service (S3) for common vulnerabilities.
* **Dynamic Region Detection:** Bypasses hardcoded endpoints by actively querying AWS to determine the geographical region of discovered resources before scanning them.
* **Secure Credential Management:** Utilizes the AWS Default Credentials Provider Chain. Secrets are never hardcoded into the application.
* **Database Persistence:** Audit histories and findings are permanently stored using Entity Framework Core and a local SQLite database.
* **Integrated Web Dashboard:** Serves a modern, dark-mode Single Page Application (SPA) directly from the .NET engine via `wwwroot`.

## 📸 Dashboard Preview

*(Note: Add the screenshot of your empty/completed dashboard here! You can name the file `dashboard.png`, place it in your folder, and uncomment the line below)*
<!-- ![CSPM Dashboard Screenshot](./dashboard.png) -->

## 🏗️ Architecture & Tech Stack

* **Backend Engine:** C# / .NET (Minimal APIs)
* **Cloud Integration:** AWS SDK for .NET (`AWSSDK.S3`, `AWSSDK.IdentityManagement`)
* **Database (ORM):** Entity Framework Core (SQLite)
* **Frontend UI:** HTML5, Vanilla JavaScript, Tailwind CSS (via CDN)

## 🚀 Getting Started

### Prerequisites
* [.NET SDK](https://dotnet.microsoft.com/download) installed locally.
* [AWS CLI](https://aws.amazon.com/cli/) installed.
* An active AWS Account with an IAM User provisioned with `SecurityAudit` permissions.

### Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/AdithaGunawardhana/cspm-auditor.git
   cd cspm-auditor


*Developed by [Aditha Gunawardhana](https://github.com/AdithaGunawardhana)*