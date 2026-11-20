# Course Registration – Enrollment and FIFO Waitlist

## Overview

This repository contains a small ASP.NET Core Web API for enrolling learners in one course offering. It enforces a prerequisite, prevents duplicate enrollment, maintains a FIFO waitlist, and promotes the earliest eligible waiting learner when a seat is freed. Ineligible learners on the waitlist are skipped without losing their place.

The service uses an in-memory repository, so all state resets when the application restarts.

## Repository layout

```text
.
├── CourseRegistration.slnx
├── CourseRegistration/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Data/
│   ├── Interfaces/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Program.cs
│   └── CourseRegistration.csproj
├── CourseRegistration.Tests/
│   ├── ConcurrencyTests.cs
│   ├── CourseOfferingTests.cs
│   ├── DropTests.cs
│   ├── EnrollmentControllerTests.cs
│   ├── EnrollmentTests.cs
│   ├── TestHelper.cs
│   └── WaitListTests.cs
└── ReadMeDocs/
    ├── AI-Prompt-Log.md
    ├── Design-Specification.md
    └── Limitations.md
```

See the [design specification](ReadMeDocs/Design-Specification.md) for the detailed architecture and complete source layout, the [AI prompt and use record](ReadMeDocs/AI-Prompt-Log.md) for the development history, and [limitations](ReadMeDocs/Limitations.md) for known risks and follow-ups.

## Requirements

- .NET 10 SDK and runtime
- Internet access for the initial NuGet restore

No database, secrets, environment variables, or external services are required.

## Build and test

Open PowerShell and navigate to the repository root (the folder containing `CourseRegistration.slnx`):

```powershell
Set-Location 'C:\CharlesSchwab_AI_Assessment'
```

Run the build and test commands from that directory:

```powershell
dotnet build CourseRegistration.slnx
dotnet test CourseRegistration.slnx
```

The test suite contains 49 tests, including HTTP controller tests and concurrency tests.

For the assessment's independent verification, run the build and tests yourself, then complete the API smoke-test steps below (or exercise the endpoints through Swagger UI). Review the AI prompt log to confirm it accurately reflects your decisions. These are candidate verification steps; no additional implementation task is outstanding.

## Run the API

From the same repository root, run:

```powershell
dotnet run --project CourseRegistration/CourseRegistration.csproj --launch-profile http
```

The HTTP launch profile listens at `http://localhost:5224`. In Development, open `http://localhost:5224/swagger` to browse the API and send requests interactively through Swagger UI. Stop the process with `Ctrl+C`. Data is reset after restarting.

## Seed data

- Offering: `CS201-2026`, **Advanced C#**
- Capacity: **2**
- Required prerequisite: `CS101`
- Eligible learners: `L1`–`L5`
- Ineligible learners: `L6` and `L7`
- Other learner IDs are unknown

## API

Base URL: `http://localhost:5224/api/enrollment`

### Get offering status

```http
GET /api/enrollment
```

Returns `200 OK`, for example:

```json
{
  "courseName": "Advanced C#",
  "capacity": 2,
  "seatsAvailable": 2,
  "enrolled": [],
  "waitlist": []
}
```

### Enroll or join the waitlist

```http
POST /api/enrollment
Content-Type: application/json

{ "learnerId": "L1" }
```

| Situation | Status | Outcome |
|---|---:|---|
| Eligible learner takes an open seat | 201 | `Enrolled` |
| Eligible learner requests a full offering | 202 | `Waitlisted` |
| Already enrolled or waitlisted | 409 | `AlreadyEnrolled` or `AlreadyWaitlisted` |
| Prerequisite not met (for example, `L6`) | 422 | `NotEligible` |
| Unknown learner | 404 | `LearnerNotFound` |
| Missing or blank learner ID | 400 | Model validation error |

### Drop enrollment or leave the waitlist

```http
DELETE /api/enrollment/L1
```

| Situation | Status | Outcome |
|---|---:|---|
| Enrolled learner is dropped | 200 | `Dropped` (includes promoted learner IDs) |
| Waitlisted learner leaves the queue | 200 | `LeftWaitlist` |
| Learner has no enrollment or waitlist entry | 404 | `NotEnrolled` |
| Unknown learner | 404 | `LearnerNotFound` |
| Blank learner ID | 400 | `InvalidRequest` |

Promotion scans the waitlist from front to back, promoting eligible learners while seats are available. Ineligible learners are skipped and retain their positions. The running API does not include an endpoint to change learner prerequisites; skip-and-retain is covered by the automated tests.

### PowerShell smoke-test sequence

With the API running, open a second PowerShell terminal and navigate to the same repository root:

```powershell
Set-Location 'C:\CharlesSchwab_AI_Assessment'
```

Then run each step in order; each command prints its response before you continue.

```powershell
$base = "http://localhost:5224/api/enrollment"
$json = "application/json"
```

1. Check the initial offering status:

   ```powershell
   Invoke-RestMethod -Uri $base | ConvertTo-Json -Depth 4
   ```

   Expected:

   ```json
   {
     "courseName": "Advanced C#",
     "capacity": 2,
     "seatsAvailable": 2,
     "enrolled": [],
     "waitlist": []
   }
   ```

2. Enroll `L1`:

   ```powershell
   Invoke-RestMethod -Method Post -Uri $base -ContentType $json -Body '{"learnerId":"L1"}' | ConvertTo-Json
   ```

   Expected:

   ```json
   {"outcome":"Enrolled","message":"Learner enrolled.","learnerId":"L1","waitlistPosition":null}
   ```

3. Enroll `L2`:

   ```powershell
   Invoke-RestMethod -Method Post -Uri $base -ContentType $json -Body '{"learnerId":"L2"}' | ConvertTo-Json
   ```

   Expected:

   ```json
   {"outcome":"Enrolled","message":"Learner enrolled.","learnerId":"L2","waitlistPosition":null}
   ```

4. Enroll `L3` after the course is full:

   ```powershell
   Invoke-RestMethod -Method Post -Uri $base -ContentType $json -Body '{"learnerId":"L3"}' | ConvertTo-Json
   ```

   Expected:

   ```json
   {"outcome":"Waitlisted","message":"Course is full; learner added to the waitlist.","learnerId":"L3","waitlistPosition":1}
   ```

5. Check the enrollment and waitlist:

   ```powershell
   Invoke-RestMethod -Uri $base | ConvertTo-Json -Depth 4
   ```

   Expected:

   ```json
   {
     "courseName": "Advanced C#",
     "capacity": 2,
     "seatsAvailable": 0,
     "enrolled": ["L1", "L2"],
     "waitlist": ["L3"]
   }
   ```

6. Drop `L1`:

   ```powershell
   Invoke-RestMethod -Method Delete -Uri "$base/L1" | ConvertTo-Json -Depth 4
   ```

   Expected:

   ```json
   {"outcome":"Dropped","message":"Learner dropped; seat released.","learnerId":"L1","promoted":["L3"]}
   ```

7. Verify the final status:

   ```powershell
   Invoke-RestMethod -Uri $base | ConvertTo-Json -Depth 4
   ```

   Expected:

   ```json
   {
     "courseName": "Advanced C#",
     "capacity": 2,
     "seatsAvailable": 0,
     "enrolled": ["L2", "L3"],
     "waitlist": []
   }
   ```
