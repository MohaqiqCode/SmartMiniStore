# 🛒 SmartMiniStore

A modern, fast, and secure **Smart Mini-Store Management System** built with **Pure C#**, **WPF (Windows Presentation Foundation)**, and **MongoDB**. The application follows the strict **MVVM (Model-View-ViewModel)** architectural pattern to ensure clean separation of concerns, high scalability, and maintainability.

[![NET Version](https://shields.io)](https://microsoft.com)
[![Database](https://shields.io)](https://mongodb.com)
[![Architecture](https://shields.io)]()
[![License](https://shields.io)](LICENSE)


---

## 📂 Project Architecture

The workspace is organized into a highly structured, enterprise-ready MVVM layout:

*   **`Views/`**: Contains the XAML UI layouts and design elements (Windows, UserControls, Pages).
*   **`ViewModels/`**: Houses the application logic, data binding properties, and commands linkable to the views.
*   **`Models/`**: Defines data structures, business entities, and MongoDB schema representations.
*   **`Services/`**: Dedicated layer for business operations, including MongoDB database connections, API calls, and authentication mechanics.
*   **`Helpers/`**: Contains utility classes, value converters, and shared configuration tools.

---

## 📋 Core Features

*   **🔒 Secure Authentication:** Multi-tier user access control for Store Admins, Managers, and Cashiers.
*   **📦 Inventory & Stock Control:** Real-time tracking of store stock levels, expiry alerts, and supplier chains.
*   **💼 Point of Sale (POS) Hub:** Highly interactive, rapid checkout interface optimized for lightning-fast invoice generation.
*   **📊 NoSQL Integration:** Backed by MongoDB for flexible schema updates, ultra-low latency reads, and structured document storage.
*   **📈 Sales Analytics:** Interactive analytics reporting metrics on high-performing products, gross revenues, and daily transactional margins.

---

## 🔧 Technical Stack

*   **Frontend UI:** WPF (XAML) using asynchronous data-binding and MVVM architecture.
*   **Backend Runtime:** Pure C# code written on the top of native .NET environment.
*   **Database Integration:** MongoDB official .NET Driver via secure asynchronously triggered Services.

---

## 🚀 Setup & Installation

### Prerequisites
*   Visual Studio 2022 (with **.NET Desktop Development** workload enabled).
*   .NET SDK installed locally.
*   MongoDB Instance (Local server running or a cloud MongoDB Atlas connection cluster).

### Step-by-Step Guide

1. **Clone the Repository**
   ```bash
   git clone https://github.com
   cd SmartMiniStore
   ```

2. **Configure Database Connection String**
   * Make sure you have created your hidden `appsettings.json` file based on the `.gitignore` policy.
   * Add your connection parameters securely without committing them to the open repository:
   ```json
   {
     "MongoSettings": {
       "ConnectionString": "mongodb://localhost:27017",
       "DatabaseName": "SmartMiniStoreDB"
     }
   }
   ```

3. **Build & Run**
   * Open the `SmartMiniStore.sln` solution file inside Visual Studio.
   * Press `F5` or click **Start** to build and run the retail environment interface locally!

---

## 📄 License
This store ecosystem project is distributed under the open-source **MIT License**. Review the `LICENSE` file for extended governance conditions.
