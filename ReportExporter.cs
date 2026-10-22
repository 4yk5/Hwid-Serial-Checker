using System.Text;

namespace HwidSerialChecker;

public static class ReportExporter
{
    private const string CommonStyles = @"
*{margin:0;padding:0;box-sizing:border-box}
body{font-family:'Space Grotesk',sans-serif;background:#050508;color:#e2e8f0;line-height:1.6;min-height:100vh;padding:30px 15px}
body::before{content:'';position:fixed;top:0;left:0;right:0;bottom:0;background:radial-gradient(circle at 50% 0%,rgba(255,255,255,0.06) 0%,transparent 60%),radial-gradient(circle at 80% 90%,rgba(34,197,94,0.04) 0%,transparent 40%);z-index:-1;pointer-events:none}
.container{max-width:1380px;margin:0 auto}
.header{background:rgba(13,14,20,0.7);backdrop-filter:blur(20px);border:1px solid rgba(255,255,255,0.08);border-radius:18px;padding:25px 35px;margin-bottom:25px;display:flex;align-items:center;justify-content:space-between;flex-wrap:wrap;gap:20px;box-shadow:0 20px 40px rgba(0,0,0,0.6)}
.brand{display:flex;align-items:center;gap:18px}
.og-logo{width:56px;height:56px;border-radius:14px;background:#ffffff;display:flex;align-items:center;justify-content:center;box-shadow:0 0 25px rgba(255,255,255,0.35);border:1px solid rgba(255,255,255,0.8);flex-shrink:0}
.og-logo span{font-family:'Space Grotesk',sans-serif;font-weight:900;font-size:26px;color:#050508;letter-spacing:-1px}
.brand-text h1{font-size:1.6em;font-weight:800;letter-spacing:3px;color:#ffffff;text-transform:uppercase}
.brand-text p{font-size:0.85em;color:#94a3b8;letter-spacing:1px;text-transform:uppercase}
.metrics{display:flex;gap:12px;flex-wrap:wrap}
.metric-card{background:rgba(20,21,30,0.8);border:1px solid rgba(255,255,255,0.06);padding:10px 18px;border-radius:12px;display:flex;flex-direction:column;align-items:center;min-width:110px}
.metric-card .lbl{font-size:0.7em;color:#64748b;text-transform:uppercase;letter-spacing:0.8px;margin-bottom:3px}
.metric-card .val{font-size:1.15em;font-weight:600;color:#ffffff;font-family:'JetBrains Mono',monospace}
.section{background:rgba(12,13,18,0.7);backdrop-filter:blur(15px);border:1px solid rgba(255,255,255,0.07);border-radius:16px;margin-bottom:20px;overflow:hidden;box-shadow:0 10px 30px rgba(0,0,0,0.4)}
.section-header{background:rgba(18,20,28,0.8);padding:14px 25px;border-bottom:1px solid rgba(255,255,255,0.07);display:flex;align-items:center;justify-content:space-between}
.section-title{font-size:0.95em;font-weight:700;letter-spacing:1.5px;color:#ffffff;text-transform:uppercase;display:flex;align-items:center;gap:10px}
.section-title::before{content:'';width:4px;height:16px;background:#ffffff;border-radius:2px;display:inline-block}
.section-content{padding:10px 20px}
.prop-info{display:flex;flex-direction:column;gap:3px}
.prop-name{font-size:0.88em;font-weight:600;color:#cbd5e1}
.prop-detail{font-size:0.75em;color:#64748b;font-family:'JetBrains Mono',monospace}
.serial-box{font-family:'JetBrains Mono',monospace;font-size:0.84em;padding:9px 14px;border-radius:8px;word-break:break-all}
.footer{margin-top:35px;text-align:center;color:#475569;font-size:0.8em;letter-spacing:1px}
.footer span{color:#ffffff;font-weight:700}
";

    public static string GenerateComparisonHtml(List<ComparisonRow> rows)
    {
        var sb = new StringBuilder();
        int changedCount = rows.Count(r => r.IsChanged);
        int sameCount = rows.Count(r => !r.IsChanged);
        string overallStatus = changedCount > 0 ? "SPOOF DETECTED" : "NO CHANGES DETECTED";
        string statusColor = changedCount > 0 ? "#4ade80" : "#f87171";
        string statusBg = changedCount > 0 ? "rgba(34, 197, 94, 0.15)" : "rgba(239, 68, 68, 0.15)";
        string statusBorder = changedCount > 0 ? "rgba(34, 197, 94, 0.3)" : "rgba(239, 68, 68, 0.3)";

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("<title>OG COMMUNITY - SERIAL COMPARISON REPORT</title>");
        sb.AppendLine("<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">");
        sb.AppendLine("<link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin>");
        sb.AppendLine("<link href=\"https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500;600&display=swap\" rel=\"stylesheet\">");
        sb.AppendLine("<style>");
        sb.Append(CommonStyles);
        sb.AppendLine($".metric-card.status-card{{background:{statusBg};border-color:{statusBorder}}}");
        sb.AppendLine($".metric-card.status-card .val{{color:{statusColor};font-weight:700}}");
        sb.AppendLine(".metric-card.changed-card{background:rgba(34,197,94,0.1);border-color:rgba(34,197,94,0.3)}");
        sb.AppendLine(".metric-card.changed-card .val{color:#4ade80;font-weight:700}");
        sb.AppendLine(".column-bar{display:grid;grid-template-columns:260px 1fr 40px 1fr;gap:15px;padding:12px 25px;background:rgba(255,255,255,0.02);border:1px solid rgba(255,255,255,0.05);border-radius:12px;margin-bottom:15px;font-size:0.8em;font-weight:600;letter-spacing:1px;color:#64748b;text-transform:uppercase}");
        sb.AppendLine(".column-bar .mid-arrow{text-align:center}");
        sb.AppendLine(".comp-row{display:grid;grid-template-columns:260px 1fr 40px 1fr;align-items:center;gap:15px;padding:12px 8px;border-bottom:1px solid rgba(255,255,255,0.04);transition:background 0.2s ease}");
        sb.AppendLine(".comp-row:last-child{border-bottom:none}");
        sb.AppendLine(".comp-row:hover{background:rgba(255,255,255,0.02);border-radius:8px}");
        sb.AppendLine(".serial-old{background:rgba(0,0,0,0.4);border:1px solid rgba(255,255,255,0.08);color:#94a3b8}");
        sb.AppendLine(".comp-arrow{text-align:center;font-size:1.1em;font-weight:700}");
        sb.AppendLine(".arrow-changed{color:#4ade80}");
        sb.AppendLine(".arrow-same{color:#334155}");
        sb.AppendLine(".serial-new-changed{background:rgba(34,197,94,0.12);border:1px solid rgba(74,222,128,0.45);color:#4ade80;font-weight:600;box-shadow:0 0 16px rgba(34,197,94,0.15);display:flex;align-items:center;justify-content:space-between;gap:10px}");
        sb.AppendLine(".serial-new-same{background:rgba(255,255,255,0.02);border:1px solid rgba(255,255,255,0.05);color:#64748b;display:flex;align-items:center;justify-content:space-between;gap:10px}");
        sb.AppendLine(".diff-badge{font-size:0.7em;padding:2px 8px;border-radius:4px;font-weight:700;letter-spacing:0.5px;text-transform:uppercase;flex-shrink:0}");
        sb.AppendLine(".diff-badge-changed{background:rgba(34,197,94,0.25);color:#4ade80;border:1px solid rgba(74,222,128,0.4)}");
        sb.AppendLine(".diff-badge-same{background:rgba(255,255,255,0.05);color:#64748b}");
        sb.AppendLine("@media(max-width:900px){.column-bar{display:none}.comp-row{grid-template-columns:1fr;gap:8px}.comp-arrow{transform:rotate(90deg);margin:4px 0}}");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"container\">");

        sb.AppendLine("<div class=\"header\">");
        sb.AppendLine("  <div class=\"brand\">");
        sb.AppendLine("    <div class=\"og-logo\"><span>OG</span></div>");
        sb.AppendLine("    <div class=\"brand-text\">");
        sb.AppendLine("      <h1>OG COMMUNITY</h1>");
        sb.AppendLine("      <p>HARDWARE ID & SERIAL COMPARISON MATRIX</p>");
        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <div class=\"metrics\">");
        sb.AppendLine($"    <div class=\"metric-card status-card\"><span class=\"lbl\">STATUS</span><span class=\"val\">{overallStatus}</span></div>");
        sb.AppendLine($"    <div class=\"metric-card changed-card\"><span class=\"lbl\">CHANGED</span><span class=\"val\">{changedCount}</span></div>");
        sb.AppendLine($"    <div class=\"metric-card\"><span class=\"lbl\">IDENTICAL</span><span class=\"val\">{sameCount}</span></div>");
        sb.AppendLine($"    <div class=\"metric-card\"><span class=\"lbl\">TIME</span><span class=\"val\">{DateTime.Now:HH:mm:ss}</span></div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class=\"column-bar\">");
        sb.AppendLine("  <div>HARDWARE & PROPERTY</div>");
        sb.AppendLine("  <div>ORIGINAL SERIAL (PREVIOUS)</div>");
        sb.AppendLine("  <div class=\"mid-arrow\">⇌</div>");
        sb.AppendLine("  <div>CURRENT SERIAL (SPOOFED)</div>");
        sb.AppendLine("</div>");

        var groups = rows.GroupBy(r => r.Category);
        foreach (var grp in groups)
        {
            sb.AppendLine("<div class=\"section\">");
            sb.AppendLine("  <div class=\"section-header\">");
            sb.AppendLine($"    <div class=\"section-title\">{grp.Key}</div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("  <div class=\"section-content\">");

            foreach (var item in grp)
            {
                string arrowClass = item.IsChanged ? "arrow-changed" : "arrow-same";
                string newBoxClass = item.IsChanged ? "serial-new-changed" : "serial-new-same";
                string badgeHtml = item.IsChanged
                    ? "<span class=\"diff-badge diff-badge-changed\">CHANGED</span>"
                    : "<span class=\"diff-badge diff-badge-same\">SAME</span>";

                sb.AppendLine("    <div class=\"comp-row\">");
                sb.AppendLine("      <div class=\"prop-info\">");
                sb.AppendLine($"        <span class=\"prop-name\">{System.Net.WebUtility.HtmlEncode(item.Property)}</span>");
                if (!string.IsNullOrEmpty(item.Details))
                {
                    sb.AppendLine($"        <span class=\"prop-detail\">{System.Net.WebUtility.HtmlEncode(item.Details)}</span>");
                }
                sb.AppendLine("      </div>");

                sb.AppendLine($"      <div class=\"serial-box serial-old\"><span>{System.Net.WebUtility.HtmlEncode(item.OldValue)}</span></div>");
                sb.AppendLine($"      <div class=\"comp-arrow {arrowClass}\">→</div>");
                sb.AppendLine($"      <div class=\"serial-box {newBoxClass}\">");
                sb.AppendLine($"        <span>{System.Net.WebUtility.HtmlEncode(item.NewValue)}</span>");
                sb.AppendLine($"        {badgeHtml}");
                sb.AppendLine("      </div>");
                sb.AppendLine("    </div>");
            }

            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("<div class=\"footer\">");
        sb.AppendLine("  <p>POWERED BY <span>OG COMMUNITY</span></p>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div></body></html>");
        return sb.ToString();
    }

    public static string GenerateSingleScanHtml(List<HardwareItem> items)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("<title>OG COMMUNITY - ORIGINAL SERIALS REPORT</title>");
        sb.AppendLine("<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">");
        sb.AppendLine("<link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin>");
        sb.AppendLine("<link href=\"https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500;600&display=swap\" rel=\"stylesheet\">");
        sb.AppendLine("<style>");
        sb.Append(CommonStyles);
        sb.AppendLine(".metric-card.status-card{background:rgba(34,197,94,0.15);border-color:rgba(34,197,94,0.3)}");
        sb.AppendLine(".metric-card.status-card .val{color:#4ade80;font-weight:700}");
        sb.AppendLine(".column-bar{display:grid;grid-template-columns:300px 1fr;gap:15px;padding:12px 25px;background:rgba(255,255,255,0.02);border:1px solid rgba(255,255,255,0.05);border-radius:12px;margin-bottom:15px;font-size:0.8em;font-weight:600;letter-spacing:1px;color:#64748b;text-transform:uppercase}");
        sb.AppendLine(".comp-row{display:grid;grid-template-columns:300px 1fr;align-items:center;gap:15px;padding:12px 8px;border-bottom:1px solid rgba(255,255,255,0.04);transition:background 0.2s ease}");
        sb.AppendLine(".comp-row:last-child{border-bottom:none}");
        sb.AppendLine(".comp-row:hover{background:rgba(255,255,255,0.02);border-radius:8px}");
        sb.AppendLine(".serial-box{background:rgba(0,0,0,0.45);border:1px solid rgba(255,255,255,0.08);color:#94a3b8}");
        sb.AppendLine("@media(max-width:900px){.column-bar{display:none}.comp-row{grid-template-columns:1fr;gap:8px}}");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"container\">");

        sb.AppendLine("<div class=\"header\">");
        sb.AppendLine("  <div class=\"brand\">");
        sb.AppendLine("    <div class=\"og-logo\"><span>OG</span></div>");
        sb.AppendLine("    <div class=\"brand-text\">");
        sb.AppendLine("      <h1>OG COMMUNITY</h1>");
        sb.AppendLine("      <p>ORIGINAL HARDWARE IDENTIFIERS BASELINE</p>");
        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <div class=\"metrics\">");
        sb.AppendLine("    <div class=\"metric-card status-card\"><span class=\"lbl\">STATUS</span><span class=\"val\">CHECKED</span></div>");
        sb.AppendLine($"    <div class=\"metric-card\"><span class=\"lbl\">TOTAL IDENTIFIERS</span><span class=\"val\">{items.Count}</span></div>");
        sb.AppendLine($"    <div class=\"metric-card\"><span class=\"lbl\">RECORDED AT</span><span class=\"val\">{DateTime.Now:HH:mm:ss}</span></div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class=\"column-bar\">");
        sb.AppendLine("  <div>HARDWARE & PROPERTY</div>");
        sb.AppendLine("  <div>ORIGINAL SERIAL NUMBER / IDENTIFIER</div>");
        sb.AppendLine("</div>");

        var groups = items.GroupBy(i => i.Category);
        foreach (var grp in groups)
        {
            sb.AppendLine("<div class=\"section\">");
            sb.AppendLine("  <div class=\"section-header\">");
            sb.AppendLine($"    <div class=\"section-title\">{grp.Key}</div>");
            sb.AppendLine("  </div>");
            sb.AppendLine("  <div class=\"section-content\">");

            foreach (var item in grp)
            {
                sb.AppendLine("    <div class=\"comp-row\">");
                sb.AppendLine("      <div class=\"prop-info\">");
                sb.AppendLine($"        <span class=\"prop-name\">{System.Net.WebUtility.HtmlEncode(item.Property)}</span>");
                if (!string.IsNullOrEmpty(item.Details))
                {
                    sb.AppendLine($"        <span class=\"prop-detail\">{System.Net.WebUtility.HtmlEncode(item.Details)}</span>");
                }
                sb.AppendLine("      </div>");
                sb.AppendLine($"      <div class=\"serial-box\"><span>{System.Net.WebUtility.HtmlEncode(item.Value)}</span></div>");
                sb.AppendLine("    </div>");
            }

            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("<div class=\"footer\">");
        sb.AppendLine("  <p>POWERED BY <span>OG COMMUNITY</span></p>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div></body></html>");
        return sb.ToString();
    }
}
