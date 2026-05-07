# Demo Dry Run Checklist

Run this on a clean Windows machine after applying migrations and creating
`PussyCats.Api/appsettings.local.json` from the example file. If the API runs
on a non-default URL, also copy `PussyCats.App/appsettings.local.json.example`
to `PussyCats.App/appsettings.local.json` and update `Api.BaseUrl`.

## Candidate Mode

- Browse Jobs loads recommendations without errors.
- My Applications loads cards and Skill Gap Analysis without `RPC_E_WRONG_THREAD`.
- My Profile loads profile details and avatar preview.
- Edit Profile saves profile changes.
- Skill Tests shows seeded demo tests.
- Personality Test opens, can be completed, and persists the result.
- Compatibility loads without route or binding errors.
- Documents uploads, views, and deletes a PDF.
- Preferences loads, saves, and reloads roles/work mode/location.

## Company Mode

- Review Applicants loads candidates for the current company.
- Applicant Status loads reviewed applicants and can submit a decision.

## Developer Mode

- Developer Hub opens from the mode selector.
- Chat remains reachable as the shared page.

## Screenshots

Place final demo screenshots in `docs/demo-screenshots/`:

- `candidate-browse-jobs.png`
- `candidate-applications.png`
- `candidate-profile.png`
- `candidate-documents.png`
- `candidate-preferences.png`
- `company-review-applicants.png`
- `company-applicant-status.png`
- `developer-hub.png`
