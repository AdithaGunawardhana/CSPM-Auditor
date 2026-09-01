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
    // 2. THE AWS SCANNER PLUGIN (Real SDK)
    // ==========================================
    public class AwsScanner : ICloudScanner
    {
        public string CloudProvider => "AWS";

        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            var findings = new List<ScanFinding>();
            var iamClient = new AmazonIdentityManagementServiceClient();
            var ec2Client = new AmazonEC2Client(RegionEndpoint.EUNorth1); 

            Console.WriteLine($"[{CloudProvider}] Scanning Identity & Access Management...");
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

            Console.WriteLine($"[{CloudProvider}] Scanning EC2 Security Groups...");
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
            return findings;
        }
    }
    
    // ==========================================
    // 3. THE AZURE SCANNER PLUGIN (Simulated Data)
    // ==========================================
    public class AzureScanner : ICloudScanner
    {
        public string CloudProvider => "Azure";

        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            Console.WriteLine($"[{CloudProvider}] Scanning Azure Resource Graph...");
            await Task.Delay(850); 
            
            return new List<ScanFinding>
            {
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "nsg-frontend-prod", RuleCode = "AZURE_NSG_SSH_OPEN", Severity = "CRITICAL", Status = "FAIL", Evidence = "0.0.0.0/0 Port 22 Allow" },
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "sub-admin-role", RuleCode = "AZURE_RBAC_OVERPERMISSIVE", Severity = "HIGH", Status = "FAIL", Evidence = "Guest user assigned 'Owner' role" }
            };
        }
    }

    // ==========================================
    // 4. THE GCP SCANNER PLUGIN (Simulated Data)
    // ==========================================
    public class GcpScanner : ICloudScanner
    {
        public string CloudProvider => "GCP";

        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            Console.WriteLine($"[{CloudProvider}] Scanning GCP Asset Inventory...");
            await Task.Delay(650); 
            
            return new List<ScanFinding>
            {
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "fw-allow-all-ingress", RuleCode = "GCP_FW_SSH_OPEN", Severity = "CRITICAL", Status = "FAIL", Evidence = "Source ranges: 0.0.0.0/0, Port: 22" }
            };
        }
    }

    // ==========================================
    // 5. THE NEW KUBERNETES SCANNER PLUGIN
    // ==========================================
    public class KubernetesScanner : ICloudScanner
    {
        public string CloudProvider => "K8S";

        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            Console.WriteLine($"[{CloudProvider}] Scanning Kubernetes Cluster Configs...");
            await Task.Delay(750); // Simulate API call to K8s API Server
            
            // Simulating K8s vulnerabilities
            return new List<ScanFinding>
            {
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "pod/nginx-privileged", RuleCode = "K8S_PRIVILEGED_POD", Severity = "CRITICAL", Status = "FAIL", Evidence = "securityContext.privileged set to true" },
                new ScanFinding { ScanId = scanId, ResourceIdentifier = "clusterrolebinding/dev-admin", RuleCode = "K8S_CLUSTER_ADMIN_EXCESSIVE", Severity = "HIGH", Status = "FAIL", Evidence = "Subject 'dev-group' bound to 'cluster-admin'" }
            };
        }
    }

    // ==========================================
    // 6. THE CORE ENGINE
    // ==========================================
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure JSON options
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            // DI Registration: Inject all 4 Scanners
            builder.Services.AddTransient<ICloudScanner, AwsScanner>();
            builder.Services.AddTransient<ICloudScanner, AzureScanner>();
            builder.Services.AddTransient<ICloudScanner, GcpScanner>();
            builder.Services.AddTransient<ICloudScanner, KubernetesScanner>();

            // DB Context Registration with Docker Volume Paths
            builder.Services.AddDbContext<CspmDbContext>(options => 
                options.UseSqlite("Data Source=data/cspm_audit.db"));
            
            builder.Services.AddDbContext<DxoAdminContext>(options => 
                options.UseSqlite("Data Source=data/dxo_admins.db"));

            var app = builder.Build();
            app.UseDefaultFiles(); 
            app.UseStaticFiles();  

            app.MapGet("/api/scans", async (CspmDbContext db) =>
            {
                return await db.Scans.Include(s => s.Findings).ToListAsync();
            });

            app.MapPost("/api/scan/run", async (IEnumerable<ICloudScanner> scanners, CspmDbContext db) =>
            {
                var currentScan = new Scan();
                db.Scans.Add(currentScan);
                await db.SaveChangesAsync();

                string webhookUrl = "https://webhook.site/22ec14ed-fa58-4f7e-815a-95ad72a6e6fb"; 
                Console.WriteLine("\n[MULTI-CLOUD AUDIT] Starting global scan...");

                // The engine will automatically run scanners safely
                foreach (var scanner in scanners)
                {
                    try 
                    {
                        var newFindings = await scanner.RunScanAsync(currentScan.Id, webhookUrl);
                        
                        foreach(var finding in newFindings)
                        {
                            db.Findings.Add(finding);
                            await SendWebhookAlertAsync(finding, webhookUrl, scanner.CloudProvider);
                        }
                    }
                    catch (Exception ex)
                    {
                        // If AWS fails due to missing Docker credentials, log it and keep going!
                        Console.WriteLine($"[WARNING] {scanner.CloudProvider} scan bypassed: {ex.Message}");
                    }
                }

                currentScan.Status = "COMPLETED";
                currentScan.CompletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Multi-Cloud Scan completed", ScanId = currentScan.Id });
            });

            app.MapPost("/api/remediate", async (RemediateRequest request) =>
            {
                try
                {
                    if (request.RuleCode == "IAM_NO_WILDCARD_ADMIN")
                    {
                        Console.WriteLine($"[AWS] Executing live remediation on {request.ResourceIdentifier}...");
                        var iamClient = new AmazonIdentityManagementServiceClient();
                        await iamClient.DetachRolePolicyAsync(new DetachRolePolicyRequest { RoleName = request.ResourceIdentifier, PolicyArn = "arn:aws:iam::aws:policy/AdministratorAccess" });
                        return Results.Ok();
                    }
                    else if (request.RuleCode == "EC2_SSH_OPEN_TO_INTERNET")
                    {
                        Console.WriteLine($"[AWS] Executing live remediation on {request.ResourceIdentifier}...");
                        var ec2Client = new AmazonEC2Client(RegionEndpoint.EUNorth1);
                        await ec2Client.RevokeSecurityGroupIngressAsync(new RevokeSecurityGroupIngressRequest {
                            GroupName = request.ResourceIdentifier,
                            IpPermissions = new List<IpPermission> { new IpPermission { IpProtocol = "tcp", FromPort = 22, ToPort = 22, Ipv4Ranges = new List<IpRange> { new IpRange { CidrIp = "0.0.0.0/0" } } } }
                        });
                        return Results.Ok();
                    }
                    else if (request.RuleCode.StartsWith("AZURE_"))
                    {
                        Console.WriteLine($"[Azure] Executing simulated remediation on {request.ResourceIdentifier}...");
                        await Task.Delay(1200); 
                        return Results.Ok();
                    }
                    else if (request.RuleCode.StartsWith("GCP_"))
                    {
                        Console.WriteLine($"[GCP] Executing simulated remediation on {request.ResourceIdentifier}...");
                        await Task.Delay(900); 
                        return Results.Ok();
                    }
                    else if (request.RuleCode.StartsWith("K8S_")) 
                    {
                        Console.WriteLine($"[K8s] Executing simulated remediation on {request.ResourceIdentifier}...");
                        await Task.Delay(1000); 
                        return Results.Ok();
                    }

                    return Results.BadRequest();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Remediation Failed: {ex.Message}");
                    return Results.Problem();
                }
            });

            // --- IAC PRE-FLIGHT SCANNER ENDPOINT ---
            app.MapPost("/api/scan/iac", async (Microsoft.AspNetCore.Http.HttpContext ctx) =>
            {
                if (!ctx.Request.HasFormContentType || ctx.Request.Form.Files.Count == 0)
                    return Results.BadRequest("No file uploaded.");

                var file = ctx.Request.Form.Files[0];
                using var reader = new System.IO.StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();
                
                var findings = new List<ScanFinding>();

                // Simulated Static Analysis: Terraform SSH
                if (content.Contains("aws_security_group") && content.Contains("0.0.0.0/0") && content.Contains("22"))
                {
                    findings.Add(new ScanFinding { ResourceIdentifier = file.FileName, RuleCode = "IAC_TF_SSH_OPEN", Severity = "CRITICAL", Status = "FAIL", Evidence = "Hardcoded 0.0.0.0/0 ingress on port 22 in Terraform." });
                }
                
                // Simulated Static Analysis: IAM Administrator
                if (content.Contains("AdministratorAccess") || (content.Contains("Action") && content.Contains("\"*\"") && content.Contains("Resource")))
                {
                    findings.Add(new ScanFinding { ResourceIdentifier = file.FileName, RuleCode = "IAC_TF_WILDCARD_ADMIN", Severity = "CRITICAL", Status = "FAIL", Evidence = "Over-permissive IAM policy wildcard detected." });
                }

                // Simulated Static Analysis: K8s Privileged Pod
                if (content.Contains("privileged: true") || content.Contains("privileged = true"))
                {
                    findings.Add(new ScanFinding { ResourceIdentifier = file.FileName, RuleCode = "IAC_K8S_PRIVILEGED", Severity = "HIGH", Status = "FAIL", Evidence = "Privileged container configuration detected." });
                }

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
                    "K8S_PRIVILEGED_POD" => $"The Kubernetes pod '{req.ResourceIdentifier}' is running in privileged mode. This grants the container root-level privileges over the host node. An attacker escaping this container will gain total control over the underlying Kubernetes worker node.",
                    "K8S_CLUSTER_ADMIN_EXCESSIVE" => $"The Kubernetes ClusterRoleBinding '{req.ResourceIdentifier}' grants broad 'cluster-admin' access. This is equivalent to root on the entire cluster. A compromised subject can modify secrets, delete namespaces, and deploy malicious workloads.",
                    "IAC_TF_SSH_OPEN" => $"The uploaded Terraform file '{req.ResourceIdentifier}' contains a security group hardcoded to allow SSH (port 22) from the public internet (0.0.0.0/0). Deploying this will instantly expose the resulting EC2 instances to brute-force attacks.",
                    "IAC_TF_WILDCARD_ADMIN" => $"The uploaded IaC template '{req.ResourceIdentifier}' provisions an IAM policy with wildcard ('*') administrative privileges. This violates the principle of least privilege. You should scope this down to specific actions before deploying.",
                    "IAC_K8S_PRIVILEGED" => $"The uploaded Kubernetes manifest '{req.ResourceIdentifier}' attempts to deploy a pod with 'privileged: true'. This allows the container to bypass security boundaries and access the host node directly.",
                    _ => $"The resource '{req.ResourceIdentifier}' triggered the rule '{req.RuleCode}'. This indicates a potential security misconfiguration that deviates from cloud best practices.",
                };

                return Results.Ok(new { 
                    Explanation = explanation, 
                    RecommendedAction = "Use the Nexus SOAR Remediate button to autonomously patch this vulnerability." 
                });
            });

            Console.WriteLine("Starting Multi-Cloud CSPM Web API Server...");

            // --- DXO ADMIN APPLICATION ENDPOINT ---
            app.MapPost("/api/admin/apply", async (AdminApplicant newApplicant, DxoAdminContext adminDb) =>
            {
                Console.WriteLine($"\n[DXO PORTAL] Incoming application from: {newApplicant.Email}");
                
                try 
                {
                    adminDb.Database.EnsureCreated(); 
                    adminDb.AdminApplicants.Add(newApplicant);
                    await adminDb.SaveChangesAsync();
                    Console.WriteLine("[DXO PORTAL] Database save SUCCESS!");
                }
                catch (Exception dbEx)
                {
                    string exactError = dbEx.InnerException?.Message ?? dbEx.Message;
                    Console.WriteLine($"[CRITICAL ERROR] Database failure: {exactError}");
                    return Results.Problem("Database Error.");
                }

                try 
                {
                    using var smtpClient = new System.Net.Mail.SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new System.Net.NetworkCredential("YOUR_EMAIL@gmail.com", "YOUR_16_DIGIT_APP_PASSWORD"),
                        EnableSsl = true,
                    };

                    var mailMessage = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress("YOUR_EMAIL@gmail.com"),
                        Subject = $"🚨 Action Required: New DxO Admin Request from {newApplicant.Name}",
                        Body = $"A new user has requested SOAR Admin access for Nexus.\n\n" +
                               $"Name: {newApplicant.Name}\n" +
                               $"Email: {newApplicant.Email}\n" +
                               $"Department: {newApplicant.Department}\n" +
                               $"Justification: {newApplicant.Justification}\n\n" +
                               $"👉 REVIEW APPLICATION: http://localhost:5000/dxo-portal.html?id={newApplicant.Id}",
                        IsBodyHtml = false,
                    };
                    
                    mailMessage.To.Add("YOUR_EMAIL@gmail.com"); 

                    await smtpClient.SendMailAsync(mailMessage); 
                    Console.WriteLine("[DXO PORTAL] Email sent successfully!");
                } 
                catch (Exception emailEx) 
                { 
                    Console.WriteLine($"[WARNING] DB saved, but Email failed to send: {emailEx.Message}"); 
                }

                return Results.Ok(new { Message = "Application processed and saved to DxO database." });
            });

            // --- DXO GOVERNANCE PORTAL APIs ---
            app.MapGet("/api/dxo/requests", async (DxoAdminContext adminDb) =>
            {
                var pending = await adminDb.AdminApplicants
                                           .Where(a => a.Status == "PENDING")
                                           .OrderByDescending(a => a.RequestDate)
                                           .ToListAsync();
                return Results.Ok(pending);
            });

            app.MapPost("/api/dxo/review", async (ReviewRequest req, DxoAdminContext adminDb) =>
            {
                var applicant = await adminDb.AdminApplicants.FindAsync(req.ApplicantId);
                if (applicant == null) return Results.NotFound("Applicant not found.");

                applicant.Status = req.Action; // "APPROVED" or "REJECTED"
                await adminDb.SaveChangesAsync();

                Console.WriteLine($"[DXO PORTAL] Applicant {applicant.Email} was {req.Action}!");
                return Results.Ok(new { Message = $"Successfully {req.Action}" });
            });

            // ==========================================
            // DOCKER INITIALIZATION: AUTO-CREATE DATABASES
            // ==========================================
            using (var scope = app.Services.CreateScope())
            {
                // 1. Create the persistent 'data' folder for Docker
                Directory.CreateDirectory("data");

                // 2. Ensure both SQLite databases generate cleanly
                var auditDb = scope.ServiceProvider.GetRequiredService<CspmDbContext>();
                auditDb.Database.EnsureCreated();

                var adminDb = scope.ServiceProvider.GetRequiredService<DxoAdminContext>();
                adminDb.Database.EnsureCreated();
            }

            app.Run();
        }

        static async Task SendWebhookAlertAsync(ScanFinding finding, string webhookUrl, string provider)
        {
            using var client = new HttpClient();
            var payload = new { text = $"🚨 *{provider} SECURITY ALERT* 🚨\n*Resource:* `{finding.ResourceIdentifier}`\n*Rule Failed:* `{finding.RuleCode}`\n*Evidence:* {finding.Evidence}" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            try { await client.PostAsync(webhookUrl, content); } catch { }
        }
    }

    // ==========================================
    // DXO COMPANY PORTAL SEPARATE DATABASE
    // ==========================================
    public class DxoAdminContext : DbContext
    {
        // ---> ADD THIS CONSTRUCTOR <---
        public DxoAdminContext(DbContextOptions<DxoAdminContext> options) : base(options) { }

        public DbSet<AdminApplicant> AdminApplicants { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Only configure if not already configured in Program.cs AddDbContext
            if (!options.IsConfigured)
            {
                options.UseSqlite("Data Source=data/dxo_admins.db");
            }
        }
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