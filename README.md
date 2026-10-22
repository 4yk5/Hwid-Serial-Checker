# ⚡ OG COMMUNITY SERIAL CHECKER & HWID AUDITOR

<p align="center">
  <img src="app.ico" width="96" height="96" alt="OG Community Logo" />
</p>

<p align="center">
  <b>A modern, high-performance hardware identifier audit and anti-cheat spoof verification suite for Windows.</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Windows" />
  <img src="https://img.shields.io/badge/License-MIT-4ADE80?style=for-the-badge" alt="MIT License" />
  <img src="https://img.shields.io/badge/Powered%20By-OG%20COMMUNITY-FFFFFF?style=for-the-badge&color=08090E" alt="OG Community" />
</p>

---

## 🌟 Overview

**OG Community Serial Checker** is an open-source, hardware-accelerated desktop application engineered to inspect, capture, and compare deep system hardware identifiers across all major hardware layers. It allows users and administrators to verify whether their hardware identifiers (HWID) have been spoofed, altered, or remain intact.

---

## ✨ Features

- **Deep Hardware Identifier Audit**:
  - **Baseboard & Motherboard**: Manufacturer, product name, serial number, and version.
  - **Processor (CPU)**: Processor ID, socket, architecture, and core identifiers.
  - **BIOS & Firmware**: Serial number, release date, SMBIOS version, core isolation, and secure boot status.
  - **Storage & Disks**: Physical drive serials, model names, SCSI/NVMe storage query WWN.
  - **Graphics (GPU)**: Adapter name, device ID, driver version, and user mode driver GUID.
  - **Network Adapters**: Physical MAC addresses, permanent hardware MAC, and registry override addresses.
  - **Monitors & Displays**: Active display serial numbers extracted directly from raw monitor EDID tables.
  - **Security (TPM)**: TPM specification version, endorsement key presence, and cryptographic hashes (MD5, SHA1, SHA256).
- **Side-by-Side Spoof Comparison Engine**:
  - Automatically compares new hardware scans against the saved original baseline.
  - Generates standalone, responsive HTML matrix reports directly to the Desktop (`OG_OriginalSerials.html` & `OG_NewSerials.html`).
  - Highlights changed identifiers in **vibrant emerald green** and unchanged identifiers in neutral gray.
- **Ultra-Modern Dark UI**:
  - DirectX hardware-accelerated WPF interface with smooth rounded corners, native drop shadows, and anti-aliased subpixel ClearType rendering.
  - Real-time **Activity Terminal** tracking scanner events and query results.
  - Hardware Snapshot panel displaying active system identifiers at a glance.
  - Cinematic **OG Splash Intro** overlay on startup.
- **Built-in GitHub Auto-Updater**:
  - Automatically queries your GitHub repository in the background.
  - Surfaces a one-click glowing update pill whenever a newer version is released.
  - Automatically downloads, replaces, and restarts the application with zero manual file handling.

---

## 🛠️ Building from Source

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Windows 10 (19041+) or Windows 11

### Build Steps
```bash
# 1. Clone the repository
git clone https://github.com/4yk5/Hwid-Serial-Checker.git
cd Hwid-Serial-Checker

# 2. Build Release binary
dotnet build -c Release

# 3. Publish as standalone Single-File executable
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o "./Publish"
```

---

## 🔄 Configuring the Auto-Updater

To connect the application to your own GitHub repository:
1. Open [`UpdateManager.cs`](UpdateManager.cs).
2. Set your GitHub username and repository name:
```csharp
public static string GitHubOwner = "4yk5";
public static string GitHubRepo = "Hwid-Serial-Checker";
```
3. When releasing an update, simply create a new **GitHub Release** (e.g. tag `v1.1.0`) and attach the compiled `HwidSerialChecker.exe`. The app will automatically detect it and prompt users to update!

---

## 📄 License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.

Developed with ❤️ by **OG COMMUNITY**.
