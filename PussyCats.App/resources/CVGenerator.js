/**
 * ─────────────────────────────────────────────────────────────────
 * Called by PdfExportService via ExecuteScriptAsync after the
 * template page has fully loaded in WebView2.
 *
 * Strategy: inject data directly into the live DOM rather than
 * rewriting the document. This is reliable with WebView2 +
 * PrintToPdfAsync because the DOM is already stable when the
 * script runs.
 * ─────────────────────────────────────────────────────────────────
 */

const CVGenerator = (() => {

    const SKILL_GROUPS = {
        Languages: ['JavaScript', 'TypeScript', 'Python', 'Java', 'C#', 'C++', 'Go', 'Rust',
            'PHP', 'Ruby', 'Swift', 'Kotlin', 'Scala', 'R', 'Julia',
            'HTML', 'CSS', 'SCSS'],
        Frameworks: [
            'React', 'Angular', 'Vue.js', 'Next.js', 'Svelte',
            'Node.js', 'Spring Boot', 'ASP.NET Core', 'Django', 'Flask', 'FastAPI'
        ],
        'DevOps / Cloud': [
            'Docker', 'Kubernetes', 'Docker Swarm', 'OpenShift', 'Podman',
            'Git', 'GitHub', 'CI/CD', 'Jenkins', 'GitHub Actions',
            'AWS', 'Azure', 'Google Cloud',
            'Terraform', 'Ansible', 'Pulumi',
            'Linux', 'Bash',
            'Prometheus', 'Grafana', 'Datadog'
        ],
        Databases: [
            'SQL Server', 'PostgreSQL', 'MySQL', 'MongoDB', 'Redis', 'Oracle', 'SQL', 'BigQuery'
        ],
        'Data & AI': [
            'ML', 'Deep Learning',
            'TensorFlow', 'PyTorch', 'scikit-learn', 'Keras',
            'Pandas', 'NumPy',
            'Apache Spark', 'MLflow', 'Hugging Face',
            'OpenCV', 'NLTK', 'spaCy',
            'Descriptive Statistics', 'Regression', 'Hypothesis Testing',
            'Linear Algebra', 'Calculus', 'Probability', 'Statistics'
        ],
        Design: [
            'Figma', 'Adobe XD', 'Zeplin', 'Sketch', 'InVision', 'Marvel', 'Axure',
            'UI/UX', 'Wireframing', 'Prototyping', 'Figma Prototyping',
            'Typography', 'Color Theory', 'Grid Systems',
            'Storybook'
        ],
        'Soft Skills': [
            'Teamwork', 'Communication', 'Problem Solving',
            'Leadership', 'Time Management'
        ],
        Other: [],
    };

    // ── Helpers ───────────────────────────────────────────────────

    function escapeSpecialCharactersInString(stringToBeChecked) {
        if (!stringToBeChecked) return '';
        return String(stringToBeChecked)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function hasValue(valueToBeChecked) {
        if (valueToBeChecked === null || valueToBeChecked === undefined) return false;
        if (Array.isArray(valueToBeChecked)) return valueToBeChecked.length > 0;
        if (typeof valueToBeChecked === 'string') return valueToBeChecked.trim() !== '';
        if (typeof valueToBeChecked === 'number') return valueToBeChecked !== 0;
        return Boolean(valueToBeChecked);
    }

    function formatDate(dateValue) {
        if (!dateValue) return '';
        let delimiterCharacter = 'T';
        // extract just the date part
        if (typeof dateValue === 'string') {
            return dateValue.split(delimiterCharacter)[0];
        }
        // i it's a Date object, format it
        if (dateValue instanceof Date) {
            return dateValue.toISOString().split(delimiterCharacter)[0];
        }
        return String(dateValue);
    }

    // ── Renderers ─────────────────────────────────────────────────

    function renderWorkExperience(workExperienceEntries = []) {
        return workExperienceEntries.map(workExperience => `
        <div class="work-entry">
            <div class="work-line">
                <span class="work-role">${escapeSpecialCharactersInString(workExperience.jobTitle)}</span>
                <span class="work-company">${escapeSpecialCharactersInString(workExperience.company)}</span>
                <span class="work-divider"></span>
                <span class="work-period">${formatDate(workExperience.startDate)} – ${workExperience.currentlyWorking || !workExperience.endDate ? 'Present' : formatDate(workExperience.endDate)}</span>
            </div>
            ${workExperience.description ? `<p class="work-description">${escapeSpecialCharactersInString(workExperience.description)}</p>` : ''}
        </div>`).join('');
    }

    function renderProjects(projectEntries = []) {
        return projectEntries.map(project => `
        <div class="project-entry-custom">
            <div class="project-line">
                <span class="project-title">${escapeSpecialCharactersInString(project.name)}</span>
                <span class="project-divider"></span>
                ${project.url ? `<span class="project-period"><a href="${escapeSpecialCharactersInString(project.url)}" style="color:#2d6a9f;font-size:7.5pt">${escapeSpecialCharactersInString(project.url)}</a></span>` : ''}
            </div>
            ${project.technologies?.length ? `<div class="project-tech">${project.technologies.map(escapeSpecialCharactersInString).join(' · ')}</div>` : ''}
            ${project.description ? `<p class="project-description">${escapeSpecialCharactersInString(project.description)}</p>` : ''}
        </div>`).join('');
    }

    function renderExtraCurricular(extraCurricularEntries = []) {
        return extraCurricularEntries.map(extraCurricularActivity => `
        <div class="extra-entry">
            <div class="extra-title">${escapeSpecialCharactersInString(extraCurricularActivity.activityName)}</div>
            <div class="extra-meta">${[extraCurricularActivity.organization, extraCurricularActivity.role, extraCurricularActivity.period].filter(Boolean).map(escapeSpecialCharactersInString).join(' · ')}</div>
            ${extraCurricularActivity.description ? `<div class="extra-desc">${escapeSpecialCharactersInString(extraCurricularActivity.description)}</div>` : ''}
        </div>`).join('');
    }

    function renderSkills(skills = []) {
        const buckets = {};
        skills
            .map(untrimmedSkill => typeof untrimmedSkill === 'string'
                ? untrimmedSkill
                : (untrimmedSkill.skill?.name || untrimmedSkill.name || untrimmedSkill.skillName || ''))
            .filter(Boolean)
            .map(skill => skill.trim())
            .forEach(skill => {
            let placed = false;
            for (const [group, keywords] of Object.entries(SKILL_GROUPS)) {
                if (group === 'Other') continue;
                if (keywords.some(SkillToBeMatched => SkillToBeMatched.toLowerCase() === skill.toLowerCase())) {
                    buckets[group] = buckets[group] || [];
                    buckets[group].push(skill);
                    placed = true;
                    break;
                }
            }
            if (!placed) {
                buckets['Other'] = buckets['Other'] || [];
                buckets['Other'].push(skill);
            }
        });

        return Object.entries(buckets).map(([label, tags]) => `
            <div class="skill-group">
                <div class="skill-group-label">${escapeSpecialCharactersInString(label)}</div>
                <div class="skill-tags">${tags.map(tag => `<span class="skill-tag">${escapeSpecialCharactersInString(tag)}</span>`).join('')}</div>
            </div>`).join('');
    }

    // ── DOM injection helpers ─────────────────────────────────────

    /** Replace text content of all elements matching selector */
    function setText(selector, value) {
        let defaultValue = '';
        document.querySelectorAll(selector).forEach(element => {
            element.textContent = value || defaultValue;
        });
    }

    /** Show or hide a section based on whether data has a value */
    function toggleSection(sectionId, showOrHideOption) {
        const element = document.getElementById(sectionId);
        if (element) element.style.display = showOrHideOption ? '' : 'none';
    }

    /** Set inner HTML of an element */
    function setHtml(selector, htmlContent) {
        const element = document.querySelector(selector);
        if (element) element.innerHTML = htmlContent;
    }

    /** Set href and text of a link element */
    function setLink(selector, linkValue) {
        const element = document.querySelector(selector);
        let defaultHref = '#'; 
        let defaultTextContent = '';
        if (element) {
            element.href = linkValue || defaultHref;
            element.textContent = linkValue || defaultTextContent;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────

    function generateProfile(profile) {

        let defaultFirstName = '';
        let defaultLastName = '';
        let defaultCity = '';
        let defaultCountry = '';
        // ── Header ────────────────────────────────────────────────
        setText('#cv-name', `${profile.firstName || defaultFirstName} ${profile.lastName || defaultLastName}`);
        setText('#cv-email', profile.email);
        setText('#cv-phone', profile.phone);
        setText('#cv-location', `${profile.city || defaultCity} ${profile.country || defaultCountry}`);

        const githubElement = document.getElementById('cv-github');
        if (githubElement) {
            if (hasValue(profile.gitHub)) {
                githubElement.style.display = '';
                githubElement.querySelector('a').href = profile.gitHub;
                githubElement.querySelector('a').textContent = profile.gitHub;
            } else {
                githubElement.style.display = 'none';
            }
        }

        const linkedInElement = document.getElementById('cv-linkedin');
        if (linkedInElement) {
            if (hasValue(profile.linkedIn)) {
                linkedInElement.style.display = '';
                linkedInElement.querySelector('a').href = profile.linkedIn;
                linkedInElement.querySelector('a').textContent = profile.linkedIn;
            } else {
                linkedInElement.style.display = 'none';
            }
        }

        // ── Education ─────────────────────────────────────────────
        const educationSection = document.getElementById('section-education');
        if (educationSection) {
            if (hasValue(profile.university)) {
                educationSection.style.display = '';
                setText('#cv-university', profile.university);
                setText('#cv-degree', profile.degree || '');
                setText('#cv-graduation',
                    profile.expectedGraduationYear ? profile.expectedGraduationYear : '');
                setText('#cv-university-start',
                    profile.universityStartYear ? profile.universityStartYear : '');
            } else {
                educationSection.style.display = 'none';
            }
        }

        // ── Work Experience ───────────────────────────────────────
        const workSection = document.getElementById('section-work');
        if (workSection) {
            if (hasValue(profile.workExperiences)) {
                workSection.style.display = '';
                setHtml('#work-list', renderWorkExperience(profile.workExperiences));
            } else {
                workSection.style.display = 'none';
            }
        }

        // ── Projects ──────────────────────────────────────────────
        const projectsSection = document.getElementById('section-projects');
        if (projectsSection) {
            if (hasValue(profile.projects)) {
                projectsSection.style.display = '';
                setHtml('#projects-list', renderProjects(profile.projects));
            } else {
                projectsSection.style.display = 'none';
            }
        }

        // ── Extracurricular ───────────────────────────────────────
        const extraCurricularSection = document.getElementById('section-extra');
        if (extraCurricularSection) {
            if (hasValue(profile.extraCurricularActivities)) {
                extraCurricularSection.style.display = '';
                setHtml('#extra-list', renderExtraCurricular(profile.extraCurricularActivities));
            } else {
                extraCurricularSection.style.display = 'none';
            }
        }


        // ── Skills ────────────────────────────────────────────────
        const skillsSection = document.getElementById('section-skills');
        if (skillsSection) {
            if (hasValue(profile.skills)) {
                skillsSection.style.display = '';
                setHtml('#skills-list', renderSkills(profile.skills));
            } else {
                skillsSection.style.display = 'none';
            }
        }
    }

    return { generateProfile };

})();

window.CVGenerator = CVGenerator;  // Expose globally for WebView2 access
