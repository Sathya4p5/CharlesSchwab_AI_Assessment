# Design Specification – Course Waitlist (single offering)

Stack: **C# / .NET 10, ASP.NET Core Web API with controllers, in-memory store, xUnit tests.**

## 1. Scope and service boundary
- **In scope:** enrollment, dropping, and FIFO waitlist promotion for **one course offering**.
- **Out of scope:** frontend, catalog, payments, identity, multi-term registration, deployment, a real database, a configurable policy engine, audit timestamps.
- **Starter project:** none was supplied, so this uses the assessment's fallback: a small service with an in-memory store, exposed as a Web API.

```
HTTP  ->  EnrollmentController  ->  IEnrollmentService  ->  IEnrollmentRepository
          (HTTP mapping only)       (all rules + the lock)   (plain in-memory storage)
```
The HTTP API is the only entry point. The controller never touches the repository, and the repository contains no rules.

## 2. Data model
| Model | Properties | Why |
|---|---|---|
| `Learner` | `LearnerId`, `CompletedCourseIds` | The prerequisite check reads `CompletedCourseIds`. The set is mutable so tests can revoke a prerequisite after a learner joins the waitlist. |
| `CourseOffering` | `OfferingId`, `CourseName`, `Capacity` (≥ 1), `RequiredPrerequisiteCourseId?` | The offering owns its capacity. Null prerequisite means none. The constructor throws on a blank id or capacity < 1. |
| `Enrollment` | `LearnerId`, `OfferingId` | A confirmed seat. |
| `WaitlistEntry` | `LearnerId`, `OfferingId` | A learner waiting for a seat. |

Left out on purpose: learner name, record IDs and timestamps (no rule reads them).

**Waitlist order = position in a `List<WaitlistEntry>`.** It is not a literal `Queue<T>`: skip-and-retain must look past ineligible learners without moving them, and a waitlisted learner who drops must be removed from the middle.

**Repository (`IEnrollmentRepository`)** is plain storage: find a learner, check `IsEnrolled` / `IsWaitlisted`, count enrollments, snapshot the enrollments and waitlist (copies, in order), add and remove enrollments and waitlist entries. Adding a learner who already has a record throws, as a second guard behind the service's checks. It is not thread-safe on its own.

## 3. Invariants
1. `enrollments.Count <= Capacity` at all times.
2. A learner is in at most one of {enrollments, waitlist}, at most once.
3. Waitlist order is list order; skipping a learner never reorders those who stay.
4. A learner who fails the eligibility check is never enrolled or promoted.
5. Every operation is atomic; a failed or invalid request changes nothing.
6. After a drop, if a seat is still free, no currently eligible learner remains on the waitlist.

## 4. Assumptions
- **Prerequisite rule:** a learner is eligible if the offering has no required prerequisite, or the learner's `CompletedCourseIds` contains it.
- Learner IDs are case-sensitive strings. Null, empty or whitespace is `InvalidRequest`; an ID not in the seeded learners is `LearnerNotFound`.
- **Eligibility is evaluated fresh on every enroll request.** A learner rejected as `NotEligible` is not queued or remembered, and must enroll again once eligible.
- **Eligibility is checked again at promotion time.** An ineligible learner cannot join the waitlist, but a waitlisted learner can become ineligible later (for example, a completed-course record is revoked). This is how an ineligible learner can reach the front of the queue.
- Seats are freed only by drops. Capacity is fixed.
- A learner's completed courses are fixed seed data; the running app has no way to change them. A learner can therefore only become ineligible after joining the waitlist in tests, which change the data directly (`Limitations.md` L5).

## 5. Waitlist promotion rule – skip and retain
After a drop frees a seat, make **one pass over the waitlist front to back**:
- Eligible and a seat is open → **promote** (remove the `WaitlistEntry`, add an `Enrollment`).
- Ineligible → **skip; they stay exactly where they are** (not moved to the back).
- Stop when no seat is open or the list ends.

**Example.** Waitlist A, B, C, D, E; A, D, E eligible; B, C ineligible.
- First seat frees: A is promoted. Waitlist: B, C, D, E.
- Second seat frees: B and C are skipped, D is promoted. Waitlist: **B, C, E**.
- Third seat frees: B and C are skipped, E is promoted. Waitlist: **B, C**.
- If nobody on the waitlist is eligible, nobody is promoted, the list is unchanged and the seat stays free (invariant 6).

Seats normally free one drop at a time. Several are open together only when every waitlisted learner was ineligible earlier; the same pass handles any number.

## 6. Operations
**Enroll(learnerId)**
1. Blank → `InvalidRequest`. Unknown learner → `LearnerNotFound`.
2. Already enrolled → `AlreadyEnrolled`. Already waitlisted → `AlreadyWaitlisted` (with position). No new record.
3. Not eligible → `NotEligible`. No record created.
4. Seat free → `Enrolled`. Otherwise append to the waitlist → `Waitlisted` (1-based position).

**Drop(learnerId)**
1. Blank → `InvalidRequest`. Unknown learner → `LearnerNotFound`.
2. On the waitlist → remove → `LeftWaitlist`. Seats unchanged.
3. Enrolled → remove, run the promotion pass → `Dropped` with the promoted IDs.
4. No record, including a repeated drop → `NotEnrolled`. No state change.

**GetStatus()** returns course name, capacity, seats available, enrolled IDs and the ordered waitlist.

## 7. Concurrency and persistence
- **Persistence:** in-memory only; state is lost on restart.
- **Concurrency:** repository and service are singletons. The service holds one private `lock` around the whole validate → mutate → promote sequence of every public method that touches state, including `GetStatus`. Service methods are synchronous because `lock` cannot span an `await`.
- **Tests:** four parallel tests, 50 rounds each. Removing the lock from `Enroll` makes three of them fail.

## 8. Tradeoffs
- **Skip-and-retain** over removing or blocking: a temporary ineligibility doesn't cost a learner their place. Cost: a permanently ineligible learner stays listed.
- **A free seat with only ineligible learners waiting** goes to a new eligible learner. A retained learner who becomes eligible is promoted only at the next drop (`Limitations.md` L1, L2).
- **Separate `Enrollment` and `WaitlistEntry`** read like the problem statement and need no status flag or sequence counter. Cost: "one place only" is enforced by the service checks and the repository guard, not by the types.
- **`List` over `Queue`**, a single nullable prerequisite, and no eligibility interface: the simplest options that satisfy the rules.
- **One lock** over fine-grained locking: simplest correct option for one offering.
- **Seed data in code:** keeps the slice small; tests change learner data directly. There is deliberately no endpoint to edit learners (`Limitations.md` L5).
- **`NotEligible` is 422, not 403:** 403 means authenticated but not permitted (an identity concern; there is none here). The request is well-formed but fails a business rule.
- **Swagger/OpenAPI** is enabled in Development for API discovery; no configurable policy engine is included.

## 9. HTTP API
Base route `api/enrollment` (singular, matching `EnrollmentController`). Responses carry `outcome` (string) and `message`. Request and response examples are in the [root README](../README.md).

| Request | Outcomes and status |
|---|---|
| `POST /api/enrollment` `{ "learnerId": "..." }` | `Enrolled` 201 · `Waitlisted` 202 · `AlreadyEnrolled` / `AlreadyWaitlisted` 409 · `NotEligible` 422 · `LearnerNotFound` 404 · `InvalidRequest` 400 |
| `DELETE /api/enrollment/{learnerId}` | `Dropped` 200 (with `promoted`) · `LeftWaitlist` 200 · `NotEnrolled` 404 · `LearnerNotFound` 404 · `InvalidRequest` 400 |
| `GET /api/enrollment` | 200: `courseName`, `capacity`, `seatsAvailable`, `enrolled`, `waitlist` |

A missing, empty or whitespace `learnerId` in the POST body is rejected by `[ApiController]` model validation (`[Required]`) before the service runs: 400 with a standard problem-details body. The service's own `InvalidRequest` outcome (JSON body with `outcome`) is reached by a blank path segment on DELETE and by direct service calls, so the rule holds for every caller. The controller owns the outcome → status mapping (a private method); the service has no HTTP knowledge.

## 10. Tests (49)
| Acceptance criterion | Where |
|---|---|
| Capacity available, full, duplicates, prerequisite failure, invalid requests | `EnrollmentTests` |
| Drops, repeated drops, waitlisted learner drops | `DropTests` |
| FIFO promotion and skip-and-retain (including the example in §5) | `WaitListTests` |
| Concurrency (parallel enrolls, duplicate enrolls, repeated drops, mixed) | `ConcurrencyTests` |
| Offering validation and the repository's duplicate guard | `CourseOfferingTests` |
| HTTP status mapping and API behavior | `EnrollmentControllerTests` |

The suite contains 49 tests. Tests build fresh service and repository instances through `TestHelper`. Seed data (`CourseRegistration/Data/SeedData.cs`): offering `CS201-2026` "Advanced C#", capacity 2, prerequisite `CS101`; learners `L1`–`L5` eligible, `L6`–`L7` not. The folder layout is in §11.

## 11. Project structure
```
CharlesSchwab_AI_Assessment/
├── .gitignore
├── CourseRegistration.slnx
├── README.md
├── CourseRegistration/
│   ├── Controllers/
│   │   └── EnrollmentController.cs
│   ├── Data/
│   │   └── SeedData.cs
│   ├── DTOs/
│   │   ├── DropResponse.cs
│   │   ├── EnrollmentRequest.cs
│   │   ├── EnrollmentResponse.cs
│   │   ├── OfferingStatusResponse.cs
│   │   └── OrderingStatusResponse.cs (empty and unused)
│   ├── Interfaces/
│   │   ├── IEnrollmentRepository.cs
│   │   └── IEnrollmentService.cs
│   ├── Models/
│   │   ├── CourseOffering.cs
│   │   ├── Enrollment.cs
│   │   ├── EnrollmentOutcome.cs
│   │   ├── Learner.cs
│   │   └── WaitlistEntry.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Repositories/
│   │   └── InMemoryEnrollmentRepository.cs
│   ├── Services/
│   │   └── EnrollmentService.cs
│   ├── CourseRegistration.csproj
│   ├── Program.cs
│   ├── appsettings.Development.json
│   └── appsettings.json
├── CourseRegistration.Tests/
│   ├── ConcurrencyTests.cs
│   ├── CourseOfferingTests.cs
│   ├── DropTests.cs
│   ├── EnrollmentControllerTests.cs
│   ├── EnrollmentTests.cs
│   ├── TestHelper.cs
│   ├── WaitListTests.cs
│   └── CourseRegistration.Tests.csproj
└── ReadMeDocs/
    ├── AI-Prompt-Log.md
    ├── Design-Specification.md
    └── Limitations.md
```
