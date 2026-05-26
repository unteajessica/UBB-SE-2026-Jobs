function addSkill() {

    const input = document.getElementById("skill-input");

    const value = input.value.trim();

    if (!value) {
        return;
    }

    const container = document.getElementById("skills-container");

    const existingSkills = Array.from(
        container.querySelectorAll("input[type='hidden']")
    ).map(skill => skill.value.toLowerCase());

    if (existingSkills.includes(value.toLowerCase())) {

        alert("Skill already added.");

        return;
    }

    const index = existingSkills.length;

    const html = `
    
    <div class="badge bg-primary p-2 me-2 mb-2 skill-item">

        ${value}

        <button type="button"
                class="btn-close btn-close-white ms-2"
                onclick="removeSkill(this)">
        </button>

        <input type="hidden"
               name="Skills[${index}].Skill.Name"
               value="${value}" />

    </div>
    `;

    container.insertAdjacentHTML("beforeend", html);

    input.value = "";
}

function removeSkill(button) {

    button.closest(".skill-item").remove();

    const container = document.getElementById("skills-container");

    Array.from(container.querySelectorAll(".skill-item")).forEach((item, index) => {

        const hidden = item.querySelector("input[type='hidden']");

        if (hidden) {
            hidden.name = `Skills[${index}].Skill.Name`;
        }
    });
}

    function addProject(project = null) {

        const container =
    document.getElementById("projectsContainer");

    const index =
    container.querySelectorAll(".project-item").length;

    const name =
    project?.name ?? "";

    const description =
    project?.description ?? "";

    const html = `

    <div class="card p-3 mb-3 project-item">

        <div class="d-flex justify-content-end mb-2">
            <button type="button"
                class="btn btn-danger"
                onclick="removeItem(this)">
                Remove
            </button>
        </div>

        <input name="Projects[${index}].Name"
            class="form-control mb-2"
            placeholder="Project Name"
            value="${escapeHtml(name)}" />

        <textarea name="Projects[${index}].Description"
            class="form-control"
            placeholder="Description">${escapeHtml(description)}</textarea>

    </div>
        `;

        container.insertAdjacentHTML("beforeend", html);
    }

function addWorkExperience(experience = null) {

    const container =
        document.getElementById("workExperienceContainer");

    const index =
        container.querySelectorAll(".work-item").length;

    const company =
        experience?.company ?? "";

    const jobTitle =
        experience?.jobTitle ?? "";

    const description =
        experience?.description ?? "";

    const currentlyWorking =
        experience?.currentlyWorking ?? false;

    const startDate =
        formatDateForInput(experience?.startDate);

    const endDate =
        formatDateForInput(experience?.endDate);

    const html = `
        
        <div class="card p-3 mb-3 work-item">

            <div class="row mb-2">

                <div class="col-md-5">
                    <input name="WorkExperiences[${index}].Company"
                           class="form-control"
                           placeholder="Company"
                           value="${escapeHtml(company)}" />
                </div>

                <div class="col-md-5">
                    <input name="WorkExperiences[${index}].JobTitle"
                           class="form-control"
                           placeholder="Job Title"
                           value="${escapeHtml(jobTitle)}" />
                </div>

                <div class="col-md-2">
                    <button type="button"
                            class="btn btn-danger w-100"
                            onclick="removeItem(this)">
                        Remove
                    </button>
                </div>

            </div>

            <div class="row mb-2">

                <div class="col-md-6">

                    <label>Start Date</label>

                    <input type="date"
                           name="WorkExperiences[${index}].StartDate"
                           class="form-control"
                           value="${startDate}" />

                </div>

                <div class="col-md-6">

                    <label>End Date</label>

                    <input type="date"
                           name="WorkExperiences[${index}].EndDate"
                           class="form-control end-date"
                           value="${endDate}"
                           ${currentlyWorking ? "disabled" : ""} />

                </div>

            </div>

            <div class="form-check mb-2">

                <input type="checkbox"
                       class="form-check-input current-work-checkbox"
                       name="WorkExperiences[${index}].CurrentlyWorking"
                       value="true"
                       onchange="toggleEndDate(this)"
                       ${currentlyWorking ? "checked" : ""} />

                <label class="form-check-label">
                    Currently Working Here
                </label>

            </div>

            <textarea name="WorkExperiences[${index}].Description"
                      class="form-control"
                      placeholder="Description">${escapeHtml(description)}</textarea>

        </div>
        `;

    container.insertAdjacentHTML("beforeend", html);
}

function toggleEndDate(checkbox) {

    const card = checkbox.closest(".work-item");

    const endDateInput = card.querySelector(".end-date");

    endDateInput.disabled = checkbox.checked;

    if (checkbox.checked) {
        endDateInput.value = "";
    }
}

function removeItem(button) {

    const item = button.closest(".work-item");

    item.remove();
}

async function uploadCv(file) {
    const formData = new FormData();
    formData.append("file", file);

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append("__RequestVerificationToken", token);
    }

    const res = await fetch(`/UserProfile/UploadCv`, {
        method: "POST",
        body: formData
    });

    if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.detail || err.title || "Upload failed");
    }

    return await res.json();
}

