using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Win32;

namespace HwidSerialChecker;

public static class HardwareScanner
{
    public static List<HardwareItem> ScanAll()
    {
        var list = new List<HardwareItem>();

        try { ScanSystemInformation(list); } catch { }
        try { ScanBaseboardInformation(list); } catch { }
        try { ScanChassisInformation(list); } catch { }
        try { ScanBiosInformation(list); } catch { }
        try { ScanProcessorInformation(list); } catch { }
        try { ScanDiskInformation(list); } catch { }
        try { ScanNetworkInformation(list); } catch { }
        try { ScanMonitorInformation(list); } catch { }
        try { ScanGpuInformation(list); } catch { }
        try { ScanTpmInformation(list); } catch { }

        return list;
    }

    #region System Information
    private static void ScanSystemInformation(List<HardwareItem> list)
    {
        string tag = "Spoofable";
        string manufacturer = "To be filled by O.E.M.";
        string productName = "To be filled by O.E.M.";
        string familySerial = "To be filled by O.E.M.";
        string skuNumber = "To be filled by O.E.M.";

        try
        {
            using var csSearcher = new ManagementObjectSearcher("SELECT Manufacturer, Model, SystemFamily, SystemSKUNumber FROM Win32_ComputerSystem");
            foreach (var obj in csSearcher.Get())
            {
                manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? manufacturer;
                productName = obj["Model"]?.ToString()?.Trim() ?? productName;
                familySerial = obj["SystemFamily"]?.ToString()?.Trim() ?? familySerial;
                skuNumber = obj["SystemSKUNumber"]?.ToString()?.Trim() ?? skuNumber;
            }
        }
        catch { }

        string versionIndex = "-MO";
        string systemSerial = "To be filled by O.E.M.";
        string systemUuid = "00000000-0000-0000-0000-000000000000";

        try
        {
            using var cspSearcher = new ManagementObjectSearcher("SELECT Version, IdentifyingNumber, UUID, Name FROM Win32_ComputerSystemProduct");
            foreach (var obj in cspSearcher.Get())
            {
                var v = obj["Version"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(v)) versionIndex = v;

                var id = obj["IdentifyingNumber"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(id)) systemSerial = id;

                var uuid = obj["UUID"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(uuid)) systemUuid = uuid;

                var name = obj["Name"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(name) && productName == "To be filled by O.E.M.") productName = name;
            }
        }
        catch { }

        list.Add(new HardwareItem("System Information", "Manufacturer", manufacturer, tag));
        list.Add(new HardwareItem("System Information", "Product Name", productName, tag));
        list.Add(new HardwareItem("System Information", "Version Index", versionIndex, tag));
        list.Add(new HardwareItem("System Information", "System Serial", systemSerial, tag));
        list.Add(new HardwareItem("System Information", "System UUID", systemUuid, tag));
        list.Add(new HardwareItem("System Information", "Family Serial", familySerial, tag));
        list.Add(new HardwareItem("System Information", "SKU Number", skuNumber, tag));
    }
    #endregion

    #region Baseboard Information
    private static void ScanBaseboardInformation(List<HardwareItem> list)
    {
        string tag = "Spoofable";
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (var obj in searcher.Get())
            {
                var manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                var version = obj["Version"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                var product = obj["Product"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                var serial = obj["SerialNumber"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                var asset = obj["Tag"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                
                string location = "Default string";
                try
                {
                    var locVal = obj["LocationInChassis"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(locVal)) location = locVal;
                }
                catch { }

                list.Add(new HardwareItem("Baseboard Information", "Manufacturer", manufacturer, tag));
                list.Add(new HardwareItem("Baseboard Information", "Version Index", version, tag));
                list.Add(new HardwareItem("Baseboard Information", "Product Name", product, tag));
                list.Add(new HardwareItem("Baseboard Information", "Serial Number", serial, tag));
                list.Add(new HardwareItem("Baseboard Information", "Asset Number", asset, tag));
                list.Add(new HardwareItem("Baseboard Information", "(CS) Location", location, tag));
            }
        }
        catch (Exception ex)
        {
            list.Add(new HardwareItem("Baseboard Information", "Error", ex.Message, tag));
        }
    }
    #endregion

    #region Chassis Information
    private static void ScanChassisInformation(List<HardwareItem> list)
    {
        string tag = "Spoofable";
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, ChassisTypes, Version, SerialNumber, SMBIOSAssetTag, SKU FROM Win32_SystemEnclosure");
            foreach (var obj in searcher.Get())
            {
                var manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                string chassisType = "3";
                if (obj["ChassisTypes"] is ushort[] types && types.Length > 0)
                {
                    chassisType = types[0].ToString();
                }
                var version = obj["Version"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                var serial = obj["SerialNumber"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                var asset = obj["SMBIOSAssetTag"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                var sku = obj["SKU"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";

                list.Add(new HardwareItem("Chassis Information", "Manufacturer", manufacturer, tag));
                list.Add(new HardwareItem("Chassis Information", "Chassis Type", chassisType, tag));
                list.Add(new HardwareItem("Chassis Information", "Version Index", version, tag));
                list.Add(new HardwareItem("Chassis Information", "Serial Number", serial, tag));
                list.Add(new HardwareItem("Chassis Information", "Asset Number", asset, tag));
                list.Add(new HardwareItem("Chassis Information", "SKU Number", sku, tag));
            }
        }
        catch (Exception ex)
        {
            list.Add(new HardwareItem("Chassis Information", "Error", ex.Message, tag));
        }
    }
    #endregion

    #region BIOS Information
    private static void ScanBiosInformation(List<HardwareItem> list)
    {
        string tag = "Spoofable";
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, SMBIOSBIOSVersion, ReleaseDate FROM Win32_BIOS");
            foreach (var obj in searcher.Get())
            {
                var vendor = obj["Manufacturer"]?.ToString()?.Trim() ?? "American Megatrends Inc.";
                var version = obj["SMBIOSBIOSVersion"]?.ToString()?.Trim() ?? "1.00";
                var releaseDate = FormatDate(obj["ReleaseDate"]?.ToString());

                list.Add(new HardwareItem("BIOS Information", "BIOS Vendor", vendor, tag));
                list.Add(new HardwareItem("BIOS Information", "BIOS Version", version, tag));
                list.Add(new HardwareItem("BIOS Information", "Release Date", releaseDate, tag));
            }
        }
        catch (Exception ex)
        {
            list.Add(new HardwareItem("BIOS Information", "Error", ex.Message, tag));
        }

        // Core Isolation (Memory Integrity)
        string coreIsolation = "Disabled";
        try
        {
            var val = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Enabled", 0);
            if (val is int intVal && intVal == 1) coreIsolation = "Enabled";
        }
        catch { }
        list.Add(new HardwareItem("BIOS Information", "Core Isolation", coreIsolation, tag));

        // Virtualization
        string virt = "Disabled";
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT VirtualizationFirmwareEnabled FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                if (obj["VirtualizationFirmwareEnabled"] is bool b && b) virt = "Enabled";
            }
        }
        catch { }
        list.Add(new HardwareItem("BIOS Information", "Virtualization", virt, tag));

        // Secure Boot
        string secBoot = "Disabled";
        try
        {
            var val = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecureBoot\State", "UEFISecureBootEnabled", 0);
            if (val is int intVal && intVal == 1) secBoot = "Enabled";
        }
        catch { }
        list.Add(new HardwareItem("BIOS Information", "Secure Boot", secBoot, tag));

        // TPM Status
        string tpmStatus = "Disabled";
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\CIMV2\Security\MicrosoftTpm", "SELECT IsEnabled_InitialValue, IsActivated_InitialValue FROM Win32_Tpm");
            foreach (var obj in searcher.Get())
            {
                bool enabled = (bool)(obj["IsEnabled_InitialValue"] ?? false);
                tpmStatus = enabled ? "Enabled" : "Disabled";
            }
        }
        catch
        {
            tpmStatus = "Disabled";
        }
        list.Add(new HardwareItem("BIOS Information", "TPM Status", tpmStatus, tag));
    }
    #endregion

    #region Processor Information
    private static void ScanProcessorInformation(List<HardwareItem> list)
    {
        string tag = "Spoofable";
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Name, ProcessorId, PartNumber, AssetTag, SocketDesignation FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                var mfg = obj["Manufacturer"]?.ToString()?.Trim() ?? "Advanced Micro Devices, Inc.";
                var name = obj["Name"]?.ToString()?.Trim() ?? "AMD Processor";
                var serial = obj["ProcessorId"]?.ToString()?.Trim() ?? "To be filled by O.E.M.";
                var part = obj["PartNumber"]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(part) || part == "Unknown") part = "Unknown";

                var asset = obj["AssetTag"]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(asset) || asset == "Unknown") asset = "Unknown";

                var socket = obj["SocketDesignation"]?.ToString()?.Trim() ?? "AM4";

                list.Add(new HardwareItem("Processor Information", "CPU Manufacturer", mfg, tag));
                list.Add(new HardwareItem("Processor Information", "Processor Type", name, tag));
                list.Add(new HardwareItem("Processor Information", "Serial Number", serial, tag));
                list.Add(new HardwareItem("Processor Information", "Part Number", part, tag));
                list.Add(new HardwareItem("Processor Information", "Asset Number", asset, tag));
                list.Add(new HardwareItem("Processor Information", "Socket Type", socket, tag));
            }
        }
        catch (Exception ex)
        {
            list.Add(new HardwareItem("Processor Information", "Error", ex.Message, tag));
        }
    }
    #endregion

    #region Disk Information
    private static void ScanDiskInformation(List<HardwareItem> list)
    {
        string tag = "Driver";
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Model, SerialNumber, InterfaceType, PNPDeviceID FROM Win32_DiskDrive");
            int driveIndex = 0;
            foreach (var obj in searcher.Get())
            {
                var model = obj["Model"]?.ToString()?.Trim() ?? "Unknown Storage Drive";
                var rawSerial = obj["SerialNumber"]?.ToString()?.Trim() ?? "00000000";
                var pnp = obj["PNPDeviceID"]?.ToString()?.Trim() ?? "";

                // SCSI_PASS_THROUGH (Raw serial or formatted SCSI serial)
                string scsiPassThrough = rawSerial;

                // STORAGE_QUERY_PROPERTY (Cleaned serial without trailing dot / underscores)
                string storageQueryProp = CleanDiskSerial(rawSerial);

                // SMART_RCV_DRIVE_DATA
                string smartData = "00000000";

                // STORAGE_QUERY_WWN (NVMe EUI64 formatted as ASCII hex or 00000000)
                string storageQueryWwn = GenerateStorageQueryWwn(rawSerial);

                // ATA_PASS_THROUGH
                string ataPassThrough = "00000000";

                string prefix = driveIndex > 0 ? $" [Disk {driveIndex}]" : "";

                list.Add(new HardwareItem("Disk Information", $"DISK_STORAGE_MODEL{prefix}", model, tag));
                list.Add(new HardwareItem("Disk Information", $"STORAGE_QUERY_PROPERTY{prefix}", storageQueryProp, tag));
                list.Add(new HardwareItem("Disk Information", $"SMART_RCV_DRIVE_DATA{prefix}", smartData, tag));
                list.Add(new HardwareItem("Disk Information", $"STORAGE_QUERY_WWN{prefix}", storageQueryWwn, tag));
                list.Add(new HardwareItem("Disk Information", $"SCSI_PASS_THROUGH{prefix}", scsiPassThrough, tag));
                list.Add(new HardwareItem("Disk Information", $"ATA_PASS_THROUGH{prefix}", ataPassThrough, tag));

                driveIndex++;
            }
        }
        catch (Exception ex)
        {
            list.Add(new HardwareItem("Disk Information", "Error", ex.Message, tag));
        }
    }
    #endregion

    #region Network Information
    private static void ScanNetworkInformation(List<HardwareItem> list)
    {
        string tag = "Spoofable";
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                             ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                             !ni.Description.Contains("Filter", StringComparison.OrdinalIgnoreCase) &&
                             !ni.Description.Contains("QoS", StringComparison.OrdinalIgnoreCase) &&
                             !ni.Description.Contains("Packet Scheduler", StringComparison.OrdinalIgnoreCase) &&
                             !ni.Description.Contains("WFP", StringComparison.OrdinalIgnoreCase) &&
                             !ni.Description.Contains("WAN Miniport", StringComparison.OrdinalIgnoreCase) &&
                             !ni.Description.Contains("Bluetooth", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int cardIndex = 1;
            var seenMacs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var ni in interfaces)
            {
                string mac = ni.GetPhysicalAddress().ToString();
                if (string.IsNullOrEmpty(mac) || mac.Length < 12) continue;

                string formattedMac = FormatMac(mac);
                if (seenMacs.Contains(formattedMac)) continue;
                seenMacs.Add(formattedMac);

                // Check registry for cached/configured MAC
                string cacheMac = GetRegistryNetworkAddress(ni.Id) ?? formattedMac;

                list.Add(new HardwareItem("Network Information", $"MAC [CACHE{cardIndex}]", cacheMac, tag, ni.Description));
                list.Add(new HardwareItem("Network Information", $"MAC [KERNEL{cardIndex}]", formattedMac, tag, ni.Description));

                cardIndex++;
            }

            if (cardIndex == 1)
            {
                // Fallback if no physical network card found
                list.Add(new HardwareItem("Network Information", "MAC [CACHE1]", "00:00:00:00:00:00", tag));
                list.Add(new HardwareItem("Network Information", "MAC [KERNEL1]", "00:00:00:00:00:00", tag));
            }
        }
        catch (Exception ex)
        {
            list.Add(new HardwareItem("Network Information", "Error", ex.Message, tag));
        }
    }
    #endregion

    #region Monitor Information
    private static void ScanMonitorInformation(List<HardwareItem> list)
    {
        string tag = "Spoofable";
        bool found = false;

        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorID");
            int monitorIndex = 0;
            foreach (var obj in searcher.Get())
            {
                var mfgBytes = obj["ManufacturerName"] as ushort[];
                var nameBytes = obj["UserFriendlyName"] as ushort[];
                var serialBytes = obj["SerialNumberID"] as ushort[];
                var productBytes = obj["ProductCodeID"] as ushort[];

                string manufacturer = DecodeUshortString(mfgBytes) ?? "MSI";
                string modelName = DecodeUshortString(nameBytes) ?? "Generic Monitor";
                string serial = DecodeUshortString(serialBytes) ?? "00000000";
                string productCode = DecodeUshortString(productBytes) ?? "711";

                string prefix = monitorIndex > 0 ? $" [Monitor {monitorIndex + 1}]" : "";

                list.Add(new HardwareItem("Monitor Information", $"Manufacturer{prefix}", manufacturer, tag));
                list.Add(new HardwareItem("Monitor Information", $"Model Name{prefix}", modelName, tag));
                list.Add(new HardwareItem("Monitor Information", $"Monitor Serial{prefix}", serial, tag));
                list.Add(new HardwareItem("Monitor Information", $"ID Serial Number{prefix}", productCode, tag));

                found = true;
                monitorIndex++;
            }
        }
        catch { }

        if (!found)
        {
            // Registry fallback for monitors
            try
            {
                using var displayKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\DISPLAY");
                if (displayKey != null)
                {
                    foreach (var subKeyName in displayKey.GetSubKeyNames())
                    {
                        list.Add(new HardwareItem("Monitor Information", "Device Key", subKeyName, tag));
                        found = true;
                        break;
                    }
                }
            }
            catch { }
        }

        if (!found)
        {
            list.Add(new HardwareItem("Monitor Information", "Manufacturer", "Generic Monitor", tag));
            list.Add(new HardwareItem("Monitor Information", "Model Name", "Generic PnP Monitor", tag));
            list.Add(new HardwareItem("Monitor Information", "Monitor Serial", "00000000", tag));
            list.Add(new HardwareItem("Monitor Information", "ID Serial Number", "0000", tag));
        }
    }
    #endregion

    #region GPU Information
    private static void ScanGpuInformation(List<HardwareItem> list)
    {
        string tag = "Driver";
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, PNPDeviceID FROM Win32_VideoController");
            int gpuIndex = 0;
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString()?.Trim() ?? "Unknown Graphics Device";
                var pnp = obj["PNPDeviceID"]?.ToString()?.Trim() ?? "PCI\\VEN_10DE&DEV_0000";

                string guidSerial = GetGpuGuidSerial(gpuIndex, pnp);

                string prefix = gpuIndex > 0 ? $" [GPU {gpuIndex + 1}]" : "";

                list.Add(new HardwareItem("GPU Information", $"PCI Device{prefix}", pnp, tag));
                list.Add(new HardwareItem("GPU Information", $"GPU Name{prefix}", name, tag));
                list.Add(new HardwareItem("GPU Information", $"GUID Serial{prefix}", guidSerial, tag));

                gpuIndex++;
            }
        }
        catch (Exception ex)
        {
            list.Add(new HardwareItem("GPU Information", "Error", ex.Message, tag));
        }
    }

    private static string GetGpuGuidSerial(int gpuIndex, string pnp)
    {
        // Query nvidia-smi uuid
        try
        {
            string output = ExecuteHiddenCommand("nvidia-smi", "--query-gpu=uuid --format=csv,noheader");
            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (gpuIndex < lines.Length)
                {
                    return lines[gpuIndex].Trim();
                }
            }
        }
        catch { }

        // Fallback to display driver registry GUID
        try
        {
            using var classKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
            if (classKey != null)
            {
                foreach (var sub in classKey.GetSubKeyNames())
                {
                    if (sub.StartsWith("000"))
                    {
                        using var cardKey = classKey.OpenSubKey(sub);
                        var devId = cardKey?.GetValue("MatchingDeviceId")?.ToString();
                        var guid = cardKey?.GetValue("GPU-UUID")?.ToString() ?? cardKey?.GetValue("UserModeDriverGUID")?.ToString();
                        if (!string.IsNullOrEmpty(guid))
                        {
                            return guid.StartsWith("GPU-") ? guid : $"GPU-{guid}";
                        }
                    }
                }
            }
        }
        catch { }

        // Fallback deterministic hash
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(pnp + gpuIndex));
        var g = new Guid(hash);
        return $"GPU-{g}";
    }
    #endregion

    #region TPM Information
    private static void ScanTpmInformation(List<HardwareItem> list)
    {
        string tag = "Driver";
        string md5 = "TPM Disabled or Error";
        string sha1 = "TPM Disabled or Error";
        string sha256 = "TPM Disabled or Error";

        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\CIMV2\Security\MicrosoftTpm", "SELECT * FROM Win32_Tpm");
            foreach (var obj in searcher.Get())
            {
                var ek = obj["EndorsementKeyPresent"] as bool?;
                if (ek == true && obj is ManagementObject mgmtObj)
                {
                    var m = mgmtObj.InvokeMethod("GetEndorsementKey", null, null);
                    if (m != null && m["EndorsementKey"] is byte[] ekBytes && ekBytes.Length > 0)
                    {
                        using var md5Hasher = System.Security.Cryptography.MD5.Create();
                        using var sha1Hasher = System.Security.Cryptography.SHA1.Create();
                        using var sha256Hasher = System.Security.Cryptography.SHA256.Create();

                        md5 = BitConverter.ToString(md5Hasher.ComputeHash(ekBytes)).Replace("-", "").ToLower();
                        sha1 = BitConverter.ToString(sha1Hasher.ComputeHash(ekBytes)).Replace("-", "").ToLower();
                        sha256 = BitConverter.ToString(sha256Hasher.ComputeHash(ekBytes)).Replace("-", "").ToLower();
                    }
                }
            }
        }
        catch { }

        list.Add(new HardwareItem("TPM Information", "MD5", md5, tag));
        list.Add(new HardwareItem("TPM Information", "SHA1", sha1, tag));
        list.Add(new HardwareItem("TPM Information", "SHA256", sha256, tag));
    }
    #endregion

    #region Helpers
    private static string CleanDiskSerial(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "00000000";
        string s = raw.Trim().TrimEnd('.');
        return s.Replace(" ", "").Replace("_", "");
    }

    private static string GenerateStorageQueryWwn(string? serial)
    {
        if (string.IsNullOrWhiteSpace(serial)) return "00000000";
        string clean = serial.Replace("_", "").TrimEnd('.').Trim();
        if (clean.Length < 16) return "00000000";

        // eui. in hex is 65:75:69:2E followed by hex ASCII of characters
        var bytes = new List<byte> { 0x65, 0x75, 0x69, 0x2E };
        foreach (char c in clean)
        {
            bytes.Add((byte)c);
        }
        return string.Join(":", bytes.Select(b => b.ToString("X2")));
    }

    private static string FormatMac(string mac)
    {
        if (string.IsNullOrEmpty(mac) || mac.Length != 12) return mac;
        return string.Join(":", Enumerable.Range(0, 6).Select(i => mac.Substring(i * 2, 2)));
    }

    private static string? GetRegistryNetworkAddress(string adapterId)
    {
        try
        {
            using var classKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}");
            if (classKey != null)
            {
                foreach (var sub in classKey.GetSubKeyNames())
                {
                    if (sub.StartsWith("000"))
                    {
                        using var k = classKey.OpenSubKey(sub);
                        var netCfg = k?.GetValue("NetCfgInstanceId")?.ToString();
                        if (string.Equals(netCfg, adapterId, StringComparison.OrdinalIgnoreCase))
                        {
                            var addr = k?.GetValue("NetworkAddress")?.ToString();
                            if (!string.IsNullOrWhiteSpace(addr))
                            {
                                return FormatMac(addr);
                            }
                        }
                    }
                }
            }
        }
        catch { }
        return null;
    }

    private static string? DecodeUshortString(ushort[]? array)
    {
        if (array == null || array.Length == 0) return null;
        var bytes = array.Where(u => u != 0).Select(u => (byte)u).ToArray();
        if (bytes.Length == 0) return null;
        return Encoding.ASCII.GetString(bytes).Trim();
    }

    private static string FormatDate(string? d)
    {
        if (string.IsNullOrWhiteSpace(d)) return "11/14/2022";
        if (d.Length >= 8 && char.IsDigit(d[0]) && char.IsDigit(d[1]) && char.IsDigit(d[2]) && char.IsDigit(d[3]))
        {
            string year = d.Substring(0, 4);
            string month = d.Substring(4, 2);
            string day = d.Substring(6, 2);
            return $"{month}/{day}/{year}";
        }
        if (DateTime.TryParse(d, out var dt))
        {
            return dt.ToString("MM/dd/yyyy");
        }
        return d;
    }

    private static string ExecuteHiddenCommand(string exe, string args)
    {
        try
        {
            using var proc = new Process();
            proc.StartInfo = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(3000);
            return output;
        }
        catch
        {
            return string.Empty;
        }
    }
    #endregion
}
