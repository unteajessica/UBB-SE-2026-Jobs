using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Persistence.Configurations;

internal static class SkillGroupSeed
{
    // Verbatim port of PussyCatsApp/repositories/SkillGroupRepository.cs. The role-compatibility
    // weights were hand-tuned by the original team — names, JobRole assignments, and weights
    // must not drift here.
    public static readonly SkillGroup[] Groups =
    {
        // Frontend Developer
        new SkillGroup { SkillGroupId = 1, GroupName = "UI Markup", Weight = 20, JobRole = JobRole.FrontendDeveloper },
        new SkillGroup { SkillGroupId = 2, GroupName = "JavaScript", Weight = 20, JobRole = JobRole.FrontendDeveloper },
        new SkillGroup { SkillGroupId = 3, GroupName = "Frontend Framework", Weight = 25, JobRole = JobRole.FrontendDeveloper },
        new SkillGroup { SkillGroupId = 4, GroupName = "Version Control", Weight = 10, JobRole = JobRole.FrontendDeveloper },
        new SkillGroup { SkillGroupId = 5, GroupName = "Testing", Weight = 10, JobRole = JobRole.FrontendDeveloper },
        new SkillGroup { SkillGroupId = 6, GroupName = "Build Tools", Weight = 8, JobRole = JobRole.FrontendDeveloper },
        new SkillGroup { SkillGroupId = 7, GroupName = "Design Collaboration", Weight = 7, JobRole = JobRole.FrontendDeveloper },

        // Backend Developer
        new SkillGroup { SkillGroupId = 8, GroupName = "Backend Language", Weight = 25, JobRole = JobRole.BackendDeveloper },
        new SkillGroup { SkillGroupId = 9, GroupName = "Web Framework", Weight = 20, JobRole = JobRole.BackendDeveloper },
        new SkillGroup { SkillGroupId = 10, GroupName = "Database Management", Weight = 20, JobRole = JobRole.BackendDeveloper },
        new SkillGroup { SkillGroupId = 11, GroupName = "API Design", Weight = 15, JobRole = JobRole.BackendDeveloper },
        new SkillGroup { SkillGroupId = 12, GroupName = "Version Control", Weight = 10, JobRole = JobRole.BackendDeveloper },
        new SkillGroup { SkillGroupId = 13, GroupName = "Testing", Weight = 10, JobRole = JobRole.BackendDeveloper },

        // UI/UX Designer
        new SkillGroup { SkillGroupId = 14, GroupName = "Design Tools", Weight = 30, JobRole = JobRole.UiUxDesigner },
        new SkillGroup { SkillGroupId = 15, GroupName = "Prototyping", Weight = 20, JobRole = JobRole.UiUxDesigner },
        new SkillGroup { SkillGroupId = 16, GroupName = "User Research", Weight = 20, JobRole = JobRole.UiUxDesigner },
        new SkillGroup { SkillGroupId = 17, GroupName = "Visual Design", Weight = 15, JobRole = JobRole.UiUxDesigner },
        new SkillGroup { SkillGroupId = 18, GroupName = "Handoff", Weight = 10, JobRole = JobRole.UiUxDesigner },
        new SkillGroup { SkillGroupId = 19, GroupName = "Analytics", Weight = 5, JobRole = JobRole.UiUxDesigner },

        // DevOps Engineer
        new SkillGroup { SkillGroupId = 20, GroupName = "Containerization", Weight = 20, JobRole = JobRole.DevOpsEngineer },
        new SkillGroup { SkillGroupId = 21, GroupName = "Orchestration", Weight = 20, JobRole = JobRole.DevOpsEngineer },
        new SkillGroup { SkillGroupId = 22, GroupName = "CI/CD", Weight = 20, JobRole = JobRole.DevOpsEngineer },
        new SkillGroup { SkillGroupId = 23, GroupName = "Cloud Platform", Weight = 15, JobRole = JobRole.DevOpsEngineer },
        new SkillGroup { SkillGroupId = 24, GroupName = "Infrastructure as Code", Weight = 15, JobRole = JobRole.DevOpsEngineer },
        new SkillGroup { SkillGroupId = 25, GroupName = "Monitoring", Weight = 10, JobRole = JobRole.DevOpsEngineer },

        // Project Manager
        new SkillGroup { SkillGroupId = 26, GroupName = "Methodologies", Weight = 25, JobRole = JobRole.ProjectManager },
        new SkillGroup { SkillGroupId = 27, GroupName = "Project Tools", Weight = 20, JobRole = JobRole.ProjectManager },
        new SkillGroup { SkillGroupId = 28, GroupName = "Risk Management", Weight = 20, JobRole = JobRole.ProjectManager },
        new SkillGroup { SkillGroupId = 29, GroupName = "Communication", Weight = 20, JobRole = JobRole.ProjectManager },
        new SkillGroup { SkillGroupId = 30, GroupName = "Budgeting", Weight = 15, JobRole = JobRole.ProjectManager },

        // Data Analyst
        new SkillGroup { SkillGroupId = 31, GroupName = "Query Language", Weight = 25, JobRole = JobRole.DataAnalyst },
        new SkillGroup { SkillGroupId = 32, GroupName = "Data Visualization", Weight = 25, JobRole = JobRole.DataAnalyst },
        new SkillGroup { SkillGroupId = 33, GroupName = "Programming", Weight = 20, JobRole = JobRole.DataAnalyst },
        new SkillGroup { SkillGroupId = 34, GroupName = "Statistical Analysis", Weight = 15, JobRole = JobRole.DataAnalyst },
        new SkillGroup { SkillGroupId = 35, GroupName = "Spreadsheets", Weight = 10, JobRole = JobRole.DataAnalyst },
        new SkillGroup { SkillGroupId = 36, GroupName = "Data Cleaning", Weight = 5, JobRole = JobRole.DataAnalyst },

        // Cybersecurity Specialist
        new SkillGroup { SkillGroupId = 37, GroupName = "Network Security", Weight = 20, JobRole = JobRole.CybersecuritySpecialist },
        new SkillGroup { SkillGroupId = 38, GroupName = "Penetration Testing", Weight = 20, JobRole = JobRole.CybersecuritySpecialist },
        new SkillGroup { SkillGroupId = 39, GroupName = "SIEM & Monitoring", Weight = 15, JobRole = JobRole.CybersecuritySpecialist },
        new SkillGroup { SkillGroupId = 40, GroupName = "Cryptography", Weight = 15, JobRole = JobRole.CybersecuritySpecialist },
        new SkillGroup { SkillGroupId = 41, GroupName = "Compliance & Standards", Weight = 15, JobRole = JobRole.CybersecuritySpecialist },
        new SkillGroup { SkillGroupId = 42, GroupName = "Incident Response", Weight = 15, JobRole = JobRole.CybersecuritySpecialist },

        // AI/ML Engineer
        new SkillGroup { SkillGroupId = 43, GroupName = "ML Frameworks", Weight = 25, JobRole = JobRole.AiMlEngineer },
        new SkillGroup { SkillGroupId = 44, GroupName = "Programming", Weight = 20, JobRole = JobRole.AiMlEngineer },
        new SkillGroup { SkillGroupId = 45, GroupName = "Mathematics", Weight = 20, JobRole = JobRole.AiMlEngineer },
        new SkillGroup { SkillGroupId = 46, GroupName = "Data Engineering", Weight = 15, JobRole = JobRole.AiMlEngineer },
        new SkillGroup { SkillGroupId = 47, GroupName = "Model Deployment", Weight = 10, JobRole = JobRole.AiMlEngineer },
        new SkillGroup { SkillGroupId = 48, GroupName = "NLP / Computer Vision", Weight = 10, JobRole = JobRole.AiMlEngineer },
    };

    // Membership rows for the EF-generated SkillGroupSkills join table. Property names
    // (SkillGroupId, SkillId) match EF's default convention for shadow FKs in a many-to-many.
    public static readonly object[] Memberships = BuildMemberships();

    private static object[] BuildMemberships()
    {
        var list = new List<object>();

        void Add(int groupId, params int[] skillIds)
        {
            foreach (var skillId in skillIds)
            {
                list.Add(new { SkillGroupId = groupId, SkillId = skillId });
            }
        }

        // Frontend Developer
        Add(1, 33, 34, 35, 36);                 // UI Markup: HTML, CSS, SCSS, Tailwind
        Add(2, 37, 31);                         // JavaScript: JavaScript, TypeScript
        Add(3, 2, 29, 30, 38);                  // Frontend Framework: React, Angular, Vue.js, Svelte
        Add(4, 39, 40);                         // Version Control: Git, GitHub
        Add(5, 41, 42, 5);                      // Testing: Jest, Cypress, Selenium
        Add(6, 43, 44, 45);                     // Build Tools: Webpack, Vite, Parcel
        Add(7, 12, 46, 47);                     // Design Collaboration: Figma, Adobe XD, Zeplin

        // Backend Developer
        Add(8, 21, 1, 8, 48, 25);               // Backend Language: Java, C#, Python, Node.js, Go
        Add(9, 22, 49, 50);                     // Web Framework: Spring Boot, ASP.NET, Django
        Add(10, 3, 26, 51, 52, 53);             // Database Management: SQL, PostgreSQL, MySQL, MongoDB, Redis
        Add(11, 54, 55, 56);                    // API Design: REST, GraphQL, gRPC
        Add(12, 39, 40);                        // Version Control: Git, GitHub
        Add(13, 57, 58, 59, 60);                // Testing: JUnit, NUnit, pytest, Postman

        // UI/UX Designer
        Add(14, 12, 46, 61, 62);                // Design Tools: Figma, Adobe XD, Sketch, InVision
        Add(15, 63, 64, 65);                    // Prototyping: Figma Prototyping, Marvel, Axure
        Add(16, 66, 67, 68);                    // User Research: Interviews, Surveys, Usability Testing
        Add(17, 69, 70, 71);                    // Visual Design: Typography, Color Theory, Grid Systems
        Add(18, 47, 12, 72);                    // Handoff: Zeplin, Figma, Storybook
        Add(19, 73, 74, 75);                    // Analytics: Google Analytics, Hotjar, Mixpanel

        // DevOps Engineer
        Add(20, 6, 76);                         // Containerization: Docker, Podman
        Add(21, 7, 77, 78);                     // Orchestration: Kubernetes, Docker Swarm, OpenShift
        Add(22, 79, 80, 81, 82);                // CI/CD: Jenkins, GitHub Actions, GitLab CI, CircleCI
        Add(23, 32, 83, 84);                    // Cloud Platform: AWS, Azure, Google Cloud
        Add(24, 85, 86, 87);                    // Infrastructure as Code: Terraform, Ansible, Pulumi
        Add(25, 88, 89, 90);                    // Monitoring: Prometheus, Grafana, Datadog

        // Project Manager
        Add(26, 91, 92, 23, 93);                // Methodologies: Scrum, Kanban, Agile, Waterfall
        Add(27, 94, 95, 96);                    // Project Tools: Jira, Trello, Asana
        Add(28, 97, 98);                        // Risk Management: Risk Assessment, Mitigation Planning
        Add(29, 99, 100, 101);                  // Communication: Stakeholder Management, Reporting, Presentations
        Add(30, 102, 103, 104);                 // Budgeting: Cost Estimation, Budget Tracking, MS Project

        // Data Analyst
        Add(31, 3, 26, 105);                    // Query Language: SQL, PostgreSQL, BigQuery
        Add(32, 106, 107, 108);                 // Data Visualization: Power BI, Tableau, Looker
        Add(33, 8, 109);                        // Programming: Python, R
        Add(34, 110, 111, 112);                 // Statistical Analysis
        Add(35, 113, 114);                      // Spreadsheets: Excel, Google Sheets
        Add(36, 9, 115);                        // Data Cleaning: Pandas, OpenRefine

        // Cybersecurity Specialist
        Add(37, 116, 117, 118, 119);            // Network Security: Firewalls, VPN, IDS/IPS, TCP/IP
        Add(38, 120, 121, 122);                 // Penetration Testing: Metasploit, Burp Suite, Nmap
        Add(39, 123, 124, 125);                 // SIEM: Splunk, IBM QRadar, Microsoft Sentinel
        Add(40, 126, 127, 128, 129);            // Cryptography: AES, RSA, PKI, TLS/SSL
        Add(41, 130, 131, 132, 133);            // Compliance: ISO 27001, GDPR, NIST, SOC 2
        Add(42, 134, 135, 136);                 // Incident Response: Forensics, Malware Analysis, DFIR

        // AI/ML Engineer
        Add(43, 137, 28, 138, 139);             // ML Frameworks: TensorFlow, PyTorch, scikit-learn, Keras
        Add(44, 8, 109, 140);                   // Programming: Python, R, Julia
        Add(45, 141, 142, 143, 144);            // Mathematics: Linear Algebra, Calculus, Probability, Statistics
        Add(46, 9, 145, 146, 3);                // Data Engineering: Pandas, NumPy, Apache Spark, SQL
        Add(47, 6, 147, 148);                   // Model Deployment: Docker, FastAPI, MLflow
        Add(48, 149, 150, 151, 152);            // NLP / Computer Vision: Hugging Face, OpenCV, NLTK, spaCy

        return list.ToArray();
    }
}
