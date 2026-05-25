function addSkill() {

    const input = document.getElementById("skill-input");

    const value = input.value.trim();

    if (!value) {
        return;
    }

    const container = document.getElementById("skills-container");

    const existingSkills = Array.from(
        container.querySelectorAll("input[type='hidden']")
    ).map(x => x.value.toLowerCase());

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
}

function addProject() {

    const container = document.getElementById("projects-container");

    const index = container.children.length;

    container.insertAdjacentHTML(
        "beforeend",
        `
        <div class="card p-3 mb-3">

            <input name="Projects[${index}].Name"
                   class="form-control mb-2"
                   placeholder="Project Name" />

            <textarea name="Projects[${index}].Description"
                      class="form-control"></textarea>

        </div>
        `
    );
}

function addWorkExperience() {

    const container = document.getElementById("work-container");

    const index = container.querySelectorAll(".work-item").length;

    const html = `
    
    <div class="card p-3 mb-3 work-item">

        <div class="row mb-2">

            <div class="col-md-5">
                <input name="WorkExperiences[${index}].Company"
                       class="form-control"
                       placeholder="Company" />
            </div>

            <div class="col-md-5">
                <input name="WorkExperiences[${index}].JobTitle"
                       class="form-control"
                       placeholder="Job Title" />
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
                       class="form-control" />
            </div>

            <div class="col-md-6">
                <label>End Date</label>

                <input type="date"
                       name="WorkExperiences[${index}].EndDate"
                       class="form-control end-date" />
            </div>

        </div>

        <div class="form-check mb-2">

            <input type="checkbox"
                   class="form-check-input current-work-checkbox"
                   onchange="toggleEndDate(this)" />

            <label class="form-check-label">
                Currently Working Here
            </label>

        </div>

        <textarea name="WorkExperiences[${index}].Description"
                  class="form-control"
                  placeholder="Description"></textarea>

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

