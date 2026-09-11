using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Amazon;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.EC2;
using Amazon.EC2.Model;

namespace CspmEngine
{
    // ==========================================
    // 1. THE MULTI-CLOUD BLUEPRINT (INTERFACE)
    // ==========================================
    public interface ICloudScanner
    {
        string CloudProvider { get; }
        Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl);
    }

    // ==========================================
    // 2. AWS SCANNER PLUGIN 
    // ==========================================
    public class AwsScanner : ICloudScanner
    {
        public string CloudProvider => "AWS";

        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            var findings = new List<ScanFinding>();
            try {
                var iamClient = new AmazonIdentityManagementServiceClient();
                var ec2Client = new AmazonEC2Client(RegionEndpoint.EUNorth1); 

                var iamResponse = await iamClient.ListRolesAsync();
                if (iamResponse?.Roles != null)
                {
                    foreach (var role in iamResponse.Roles)
                    {
                        if (role.RoleName.StartsWith("AWSServiceRole")) continue;
                        var attachedPolicies = await iamClient.ListAttachedRolePoliciesAsync(new ListAttachedRolePoliciesRequest { RoleName = role.RoleName });
                        
                        if (attachedPolicies.AttachedPolicies?.Any(p => p.PolicyName == "AdministratorAccess") == true)
                        {
                            findings.Add(new ScanFinding { ScanId = scanId, ResourceIdentifier = role.RoleName, RuleCode = "IAM_NO_WILDCARD_ADMIN", Severity = "CRITICAL", Status = "FAIL", Evidence = "Attached policy 'AdministratorAccess' found." });
                        }
                    }
                }

                var sgResponse = await ec2Client.DescribeSecurityGroupsAsync();
                if (sgResponse?.SecurityGroups != null)
                {
                    foreach (var sg in sgResponse.SecurityGroups)
                    {
                        if (sg.IpPermissions != null)
                        {
                            foreach (var ipPerm in sg.IpPermissions)
                            {
                                if (ipPerm.IpProtocol == "tcp" && ipPerm.FromPort <= 22 && ipPerm.ToPort >= 22 && ipPerm.Ipv4Ranges?.Any(ip => ip.CidrIp == "0.0.0.0/0") == true)
                                {
                                    findings.Add(new ScanFinding { ScanId = scanId, ResourceIdentifier = sg.GroupName, RuleCode = "EC2_SSH_OPEN_TO_INTERNET", Severity = "CRITICAL", Status = "FAIL", Evidence = "Inbound SSH (Port 22) open to 0.0.0.0/0." });
                                }
                            }
                        }
                    }
                }
            } catch {
                // Failsafe for Docker environments without AWS creds injected
            }
            return findings;
        }
    }
    
    // ==========================================
    // 3. AZURE SCANNER PLUGIN
    // ==========================================
    public class AzureScanner : ICloudScanner
    {
        public string CloudProvider => "Azure";
        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            await Task.Delay(850); 
            return new List<ScanFinding>
            {
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "nsg-frontend-prod", RuleCode = "AZURE_NSG_SSH_OPEN", Severity = "CRITICAL", Status = "FAIL", Evidence = "0.0.0.0/0 Port 22 Allow" },
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "sub-admin-role", RuleCode = "AZURE_RBAC_OVERPERMISSIVE", Severity = "HIGH", Status = "FAIL", Evidence = "Guest user assigned 'Owner' role" }
            };
        }
    }

    // ==========================================
    // 4. GCP SCANNER PLUGIN 
    // ==========================================
    public class GcpScanner : ICloudScanner
    {
        public string CloudProvider => "GCP";
        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            await Task.Delay(650); 
            return new List<ScanFinding>
            {
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "fw-allow-all-ingress", RuleCode = "GCP_FW_SSH_OPEN", Severity = "CRITICAL", Status = "FAIL", Evidence = "Source ranges: 0.0.0.0/0, Port: 22" }
            };
        }
    }

    // ==========================================
    // 5. KUBERNETES SCANNER PLUGIN
    // ==========================================
    public class KubernetesScanner : ICloudScanner
    {
        public string CloudProvider => "K8S";
        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            await Task.Delay(750); 
            return new List<ScanFinding>
            {
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "pod/nginx-privileged", RuleCode = "K8S_PRIVILEGED_POD", Severity = "CRITICAL", Status = "FAIL", Evidence = "securityContext.privileged set to true" },
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "clusterrolebinding/dev-admin", RuleCode = "K8S_CLUSTER_ADMIN_EXCESSIVE", Severity = "HIGH", Status = "FAIL", Evidence = "Subject 'dev-group' bound to 'cluster-admin'" }
            };
        }
    }

    // ==========================================
    // ⚡ 6. NEW ORACLE CLOUD SCANNER PLUGIN ⚡
    // ==========================================
    public class OracleScanner : ICloudScanner
    {
        public string CloudProvider => "Oracle";
        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            await Task.Delay(950); // Simulate API call to OCI 
            return new List<ScanFinding>
            {
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "oci-bucket-backups", RuleCode = "ORACLE_PUBLIC_STORAGE", Severity = "HIGH", Status = "FAIL", Evidence = "Object storage bucket configured for public Read access." }
            };
        }
    }

    // ==========================================
    // 7. MULTI-TENANT ENTITIES & DBCONTEXT
    // ==========================================
    public class CspmDbContext : DbContext
    {
        public CspmDbContext(DbContextOptions<CspmDbContext> options) : base(options) { }
        public DbSet<Scan> Scans { get; set; }
        public DbSet<ScanFinding> Findings { get; set; }
    }

    public class Scan
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = "guest"; 
        public string Status { get; set; } = "PENDING";
        public DateTime? CompletedAt { get; set; }
        public List<ScanFinding> Findings { get; set; } = new();
    }

    public class ScanFinding
    {
        public int Id { get; set; }
        public int ScanId { get; set; }
        public string TenantId { get; set; } = "guest"; 
        public string ResourceIdentifier { get; set; } = "";
        public string RuleCode { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Status { get; set; } = "";
        public string Evidence { get; set; } = "";
    }

    // ==========================================
    // 8. THE CORE ENGINE API
    // ==========================================
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            // ⚡ REGISTER ALL SCANNERS (INCLUDING ORACLE) ⚡
            builder.Services.AddTransient<ICloudScanner, AwsScanner>();
            builder.Services.AddTransient<ICloudScanner, AzureScanner>();
            builder.Services.AddTransient<ICloudScanner, GcpScanner>();
            builder.Services.AddTransient<ICloudScanner, OracleScanner>(); // <--- New Oracle Plugin Registered!
            builder.Services.AddTransient<ICloudScanner, KubernetesScanner>();

            builder.Services.AddDbContext<CspmDbContext>(options => 
                options.UseSqlite("Data Source=data/cspm_audit.db"));
            
            builder.Services.AddDbContext<DxoAdminContext>(options => 
                options.UseSqlite("Data Source=data/dxo_admins.db"));

            var app = builder.Build();
            app.UseDefaultFiles(); 
            app.UseStaticFiles();  

            // MULTI-TENANT ISOLATED GET 
            app.MapGet("/api/scans", async (HttpContext ctx, CspmDbContext db) =>
            {
                var tenantId = ctx.Request.Headers["X-Tenant-ID"].ToString();
                if (string.IsNullOrEmpty(tenantId)) tenantId = "guest";

                var userScans = await db.Scans.Include(s => s.Findings)
                                              .Where(s => s.TenantId == tenantId)
                                              .ToListAsync();
                return Results.Ok(userScans);
            });

            // MULTI-TENANT ISOLATED SCAN 
            app.MapPost("/api/scan/run", async (HttpContext ctx, CspmDbContext db) =>
            {
                var scanners = ctx.RequestServices.GetServices<ICloudScanner>();
                var tenantId = ctx.Request.Headers["X-Tenant-ID"].ToString();
                if (string.IsNullOrEmpty(tenantId)) tenantId = "guest";

                var currentScan = new Scan { TenantId = tenantId };
                db.Scans.Add(currentScan);
                await db.SaveChangesAsync();

                string webhookUrl = "https://webhook.site/22ec14ed-fa58-4f7e-815a-95ad72a6e6fb"; 
                Console.WriteLine($"\n[MULTI-CLOUD AUDIT] Starting scan for Tenant: {tenantId}");

                foreach (var scanner in scanners)
                {
                    try 
                    {
                        var newFindings = await scanner.RunScanAsync(currentScan.Id, webhookUrl);
                        foreach(var finding in newFindings)
                        {
                            finding.TenantId = tenantId; 
                            db.Findings.Add(finding);
                            await SendWebhookAlertAsync(finding, webhookUrl, scanner.CloudProvider);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARNING] {scanner.CloudProvider} scan bypassed: {ex.Message}");
                    }
                }

                currentScan.Status = "COMPLETED";
                currentScan.CompletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Scan completed", ScanId = currentScan.Id });
            });

            // MULTI-TENANT ISOLATED REMEDIATION
            app.MapPost("/api/remediate", async (HttpContext ctx, RemediateRequest request, CspmDbContext db) =>
            {
                var tenantId = ctx.Request.Headers["X-Tenant-ID"].ToString();
                if (string.IsNullOrEmpty(tenantId)) tenantId = "guest";

                try
                {
                    var findingToFix = await db.Findings.FirstOrDefaultAsync(f => f.TenantId == tenantId && f.RuleCode == request.RuleCode && f.ResourceIdentifier == request.ResourceIdentifier);
                    
                    if (findingToFix != null)
                    {
                        db.Findings.Remove(findingToFix);
                        await db.SaveChangesAsync();
                    }

                    if (request.RuleCode == "IAM_NO_WILDCARD_ADMIN") {
                        Console.WriteLine($"[AWS] Patching {request.ResourceIdentifier} for {tenantId}");
                        var iamClient = new AmazonIdentityManagementServiceClient();
                        await iamClient.DetachRolePolicyAsync(new DetachRolePolicyRequest { RoleName = request.ResourceIdentifier, PolicyArn = "arn:aws:iam::aws:policy/AdministratorAccess" });
                        return Results.Ok();
                    }
                    else if (request.RuleCode == "EC2_SSH_OPEN_TO_INTERNET") {
                        Console.WriteLine($"[AWS] Patching {request.ResourceIdentifier} for {tenantId}");
                        var ec2Client = new AmazonEC2Client(RegionEndpoint.EUNorth1);
                        await ec2Client.RevokeSecurityGroupIngressAsync(new RevokeSecurityGroupIngressRequest { GroupName = request.ResourceIdentifier, IpPermissions = new List<IpPermission> { new IpPermission { IpProtocol = "tcp", FromPort = 22, ToPort = 22, Ipv4Ranges = new List<IpRange> { new IpRange { CidrIp = "0.0.0.0/0" } } } } });
                        return Results.Ok();
                    }
                    // ⚡ ORACLE REMEDIATION SUPPORT ⚡
                    else if (request.RuleCode.StartsWith("ORACLE_")) {
                        Console.WriteLine($"[Oracle] Executing simulated remediation on {request.ResourceIdentifier} for {tenantId}");
                        await Task.Delay(1100); 
                        return Results.Ok();
                    }
                    else {
                        Console.WriteLine($"[Cloud] Simulated patch on {request.ResourceIdentifier} for {tenantId}");
                        await Task.Delay(1000); 
                        return Results.Ok();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Remediation Failed: {ex.Message}");
                    return Results.Problem();
                }
            });

            // --- IAC & AI ENDPOINTS ---
            app.MapPost("/api/scan/iac", async (HttpContext ctx) =>
            {
                var tenantId = ctx.Request.Headers["X-Tenant-ID"].ToString();
                if (!ctx.Request.HasFormContentType || ctx.Request.Form.Files.Count == 0) return Results.BadRequest("No file uploaded.");

                var file = ctx.Request.Form.Files[0];
                using var reader = new System.IO.StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();
                var findings = new List<ScanFinding>();

                if (content.Contains("aws_security_group") && content.Contains("0.0.0.0/0") && content.Contains("22"))
                    findings.Add(new ScanFinding { ResourceIdentifier = file.FileName, RuleCode = "IAC_TF_SSH_OPEN", Severity = "CRITICAL", Status = "FAIL", Evidence = "Hardcoded 0.0.0.0/0 ingress on port 22 in Terraform.", TenantId = tenantId });
                
                if (content.Contains("AdministratorAccess") || (content.Contains("Action") && content.Contains("\"*\"") && content.Contains("Resource")))
                    findings.Add(new ScanFinding { ResourceIdentifier = file.FileName, RuleCode = "IAC_TF_WILDCARD_ADMIN", Severity = "CRITICAL", Status = "FAIL", Evidence = "Over-permissive IAM policy wildcard detected.", TenantId = tenantId });

                if (content.Contains("privileged: true") || content.Contains("privileged = true"))
                    findings.Add(new ScanFinding { ResourceIdentifier = file.FileName, RuleCode = "IAC_K8S_PRIVILEGED", Severity = "HIGH", Status = "FAIL", Evidence = "Privileged container configuration detected.", TenantId = tenantId });

                return Results.Ok(findings);
            });

            app.MapPost("/api/ai/explain", async (AiExplanationRequest req) =>
            {
                await Task.Delay(1500);
                string explanation = req.RuleCode switch
                {
                    "IAM_NO_WILDCARD_ADMIN" => $"The role '{req.ResourceIdentifier}' has the 'AdministratorAccess' policy attached. This grants full, unrestricted access to all AWS resources. If an attacker compromises this role, they can delete databases, exfiltrate S3 data, and spin up unauthorized resources.",
                    "EC2_SSH_OPEN_TO_INTERNET" => $"Security group '{req.ResourceIdentifier}' allows inbound SSH (Port 22) from 0.0.0.0/0. This means anyone on the internet can attempt to brute-force or use stolen keys to log into your EC2 instances. It is highly recommended to restrict this to a specific corporate IP.",
                    "AZURE_NSG_SSH_OPEN" => $"The Azure Network Security Group '{req.ResourceIdentifier}' permits global inbound SSH access. Attackers constantly scan public Azure IP ranges for port 22. Leaving this open risks a direct system compromise via dictionary attacks.",
                    "AZURE_RBAC_OVERPERMISSIVE" => $"A guest user or external identity '{req.ResourceIdentifier}' holds 'Owner' level privileges. This violates the principle of least privilege. An external account compromise could lead to a complete takeover of the Azure subscription.",
                    "GCP_FW_SSH_OPEN" => $"The GCP firewall rule '{req.ResourceIdentifier}' allows ingress traffic on port 22 from any source. This exposes your Google Compute Engine instances to global port scanners and automated SSH brute-force bots.",
                    // ⚡ ORACLE AI EXPLANATION ⚡
                    "ORACLE_PUBLIC_STORAGE" => $"The Oracle Cloud Object Storage bucket '{req.ResourceIdentifier}' is configured for public read access. This exposes potentially sensitive backup data to the public internet and violates strict access control policies.",
                    "K8S_PRIVILEGED_POD" => $"The Kubernetes pod '{req.ResourceIdentifier}' is running in privileged mode. This grants the container root-level privileges over the host node. An attacker escaping this container will gain total control over the underlying Kubernetes worker node.",
                    "K8S_CLUSTER_ADMIN_EXCESSIVE" => $"The Kubernetes ClusterRoleBinding '{req.ResourceIdentifier}' grants broad 'cluster-admin' access. This is equivalent to root on the entire cluster. A compromised subject can modify secrets, delete namespaces, and deploy malicious workloads.",
                    "IAC_TF_SSH_OPEN" => $"The uploaded Terraform file '{req.ResourceIdentifier}' contains a security group hardcoded to allow SSH (port 22) from the public internet (0.0.0.0/0). Deploying this will instantly expose the resulting EC2 instances to brute-force attacks.",
                    "IAC_TF_WILDCARD_ADMIN" => $"The uploaded IaC template '{req.ResourceIdentifier}' provisions an IAM policy with wildcard ('*') administrative privileges. This violates the principle of least privilege. You should scope this down to specific actions before deploying.",
                    "IAC_K8S_PRIVILEGED" => $"The uploaded Kubernetes manifest '{req.ResourceIdentifier}' attempts to deploy a pod with 'privileged: true'. This allows the container to bypass security boundaries and access the host node directly.",
                    _ => $"The resource '{req.ResourceIdentifier}' triggered the rule '{req.RuleCode}'. This indicates a potential security misconfiguration that deviates from cloud best practices.",
                };
                return Results.Ok(new { Explanation = explanation, RecommendedAction = "Use the Nexus SOAR Remediate button to autonomously patch this vulnerability." });
            });

            // --- DXO ADMIN APPLICATION ENDPOINT ---
            app.MapPost("/api/admin/apply", async (AdminApplicant newApplicant, DxoAdminContext adminDb) =>
            {
                try 
                {
                    adminDb.Database.EnsureCreated(); 
                    adminDb.AdminApplicants.Add(newApplicant);
                    await adminDb.SaveChangesAsync();
                }
                catch (Exception dbEx) { return Results.Problem("Database Error."); }
                return Results.Ok(new { Message = "Application processed and saved to DxO database." });
            });

            app.MapGet("/api/dxo/requests", async (DxoAdminContext adminDb) =>
            {
                var pending = await adminDb.AdminApplicants.Where(a => a.Status == "PENDING").OrderByDescending(a => a.RequestDate).ToListAsync();
                return Results.Ok(pending);
            });

            app.MapPost("/api/dxo/review", async (ReviewRequest req, DxoAdminContext adminDb) =>
            {
                var applicant = await adminDb.AdminApplicants.FindAsync(req.ApplicantId);
                if (applicant == null) return Results.NotFound("Applicant not found.");
                applicant.Status = req.Action; 
                await adminDb.SaveChangesAsync();
                return Results.Ok(new { Message = $"Successfully {req.Action}" });
            });

            // SELF-HEALING DATABASE INITIALIZATION
            using (var scope = app.Services.CreateScope())
            {
                Directory.CreateDirectory("data");
                var auditDb = scope.ServiceProvider.GetRequiredService<CspmDbContext>();
                auditDb.Database.EnsureDeleted(); 
                auditDb.Database.EnsureCreated(); 

                var adminDb = scope.ServiceProvider.GetRequiredService<DxoAdminContext>();
                adminDb.Database.EnsureDeleted(); 
                adminDb.Database.EnsureCreated(); 
            }

            app.Run();
        }

        static async Task SendWebhookAlertAsync(ScanFinding finding, string webhookUrl, string provider)
        {
            using var client = new HttpClient();
            var payload = new { text = $"🚨 *{provider} SECURITY ALERT* 🚨\n*Resource:* `{finding.ResourceIdentifier}`\n*Rule Failed:* `{finding.RuleCode}`" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            try { await client.PostAsync(webhookUrl, content); } catch { }
        }
    }

    // ==========================================
    // DXO COMPANY PORTAL SEPARATE DATABASE
    // ==========================================
    public class DxoAdminContext : DbContext
    {
        public DxoAdminContext(DbContextOptions<DxoAdminContext> options) : base(options) { }
        public DbSet<AdminApplicant> AdminApplicants { get; set; }
    }

    public class AdminApplicant
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Department { get; set; }
        public required string Justification { get; set; }
        public string Status { get; set; } = "PENDING";
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }

    public record RemediateRequest(string ResourceIdentifier, string RuleCode);
    public record AiExplanationRequest(string ResourceIdentifier, string RuleCode);
    public record ReviewRequest(int ApplicantId, string Action);
}