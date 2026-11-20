# AI Prompt and Use Record – Course Waitlist Assessment

This file records how AI (an AI assistant in VS Code, Copilot SDK) was used on this assessment. It satisfies the "AI prompt and use record" deliverable and acceptance criterion 7.

- Current files are `ReadMeDocs/Design-Specification.md`, `ReadMeDocs/AI-Prompt-Log.md`, and `ReadMeDocs/Limitations.md`. Earlier entries below preserve historical project names and paths where they describe prior decisions.
- Prompts below are **rephrased** from the originals. They contain no credentials, secrets, or personal data.
- Responses are **summaries or excerpts**, not full transcripts.
- Each suggestion is marked **Accepted**, **Edited**, or **Rejected**, with the files or decisions it informed.
- Verification entries are only filled in for work that has actually been done. A *Pending* mark means not yet done; in Entries 1–5 these were closed by Entry 7.
- Pending/open labels in older entries record their historical state; Entry 20 records the current candidate-verification status.

---

## Entry 1 – Understanding the assignment

### Activity
Requirement analysis (reading the assessment email and PDF).

### Prompt (rephrased)
> I have an assessment with an email body and an attached PDF describing it. No starter kit or rubric file was provided. The implementation must be in C# on .NET 10 and shared via a GitHub link along with any other required docs. Read both very carefully and tell me exactly what is being asked.

### Context supplied
- The email: 24-hour submission window, GitHub link, C# and .NET 10, and a note that the assessment evaluates AI-assisted engineering, prompt engineering, problem solving and modern engineering practices.
- The PDF: *Focused AI-Assisted Engineering Assessment: Course Waitlist* (objective, problem statement, 7 acceptance criteria, scope boundaries, deliverables, candidate expectations, suggested timebox).

### AI response (summary)
1. **Core ask.** Build a small, correct enroll / drop / FIFO-waitlist service for **one** course offering in C#/.NET 10. Use AI as an engineering assistant and document the use honestly. Grading emphasises correct capacity and queue behaviour, focused tests, and evidence of independent review over feature count.
2. **Acceptance criteria mapped to implications.**
   - AC1: enrollment needs a defined prerequisite rule.
   - AC2: a duplicate request, whether already enrolled or already waitlisted, creates no new record.
   - AC3: a drop promotes the earliest eligible waitlisted learner.
   - AC4: an ineligible learner is never enrolled or promoted, and the skip/retain policy is applied consistently.
   - AC5: invalid requests and repeated drops return clear results and change no state.
   - AC6: tests for capacity available, full, duplicates, FIFO, prerequisite failure, and invalid/repeated drops, plus a focused concurrency test.
   - AC7: this record.
3. **Deliverables.** Code and tests; a design spec; an AI prompt and use record; setup and run instructions; and limitations and risks.
4. **Scope limits.** No frontend, catalog, payments, identity, multi-term, deployment, new database, configurable policy engine, or multiple scenario variants.
5. **Gaps found in the materials.**
   - The PDF says "the provided starter project", but none was supplied. The stated fallback is a small service or CLI with an in-memory store.
   - The PDF links `Course-Registration-Rubric-Score.md`, which was not attached. Work from the PDF's stated priorities and consider asking the recruiter.
6. **Hidden design trap.** AC2 lets only *eligible* learners join the waitlist. An ineligible learner can therefore reach the front of the queue only if eligibility *changes after joining*. The design must re-check eligibility at promotion time and make the eligibility source swappable so tests can simulate the change.
7. **Ambiguities to decide and document.**
   - Skip vs. retain for ineligible learners (AI recommended skip-and-retain-position).
   - Whether a waitlisted learner can drop (leave the queue).
   - Handling of invalid input (null/empty IDs) and invalid capacity.
   - The concrete prerequisite rule.
8. **Invariants proposed.**
   - Enrolled count never exceeds capacity.
   - A learner is in at most one of enrolled or waitlist, at most once.
   - The waitlist is strictly FIFO.
   - Every operation is atomic (a single lock per offering).
   - A failed or invalid operation changes nothing.
9. **Suggested structure.**
   - `src/Waitlist.Core` and `tests/Waitlist.Tests` (xUnit).
   - A `Parallel.For` concurrency test.
   - `README.md`, `Design-Specification.md`, `AI-Prompt-Log.md`.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Use the "no starter" fallback (in-memory service) | **Accepted** | `Design-Specification.md` §1 |
| Re-check eligibility at promotion time via a swappable checker | **Edited in Entry 3**: re-check kept, but the swappable `IEligibilityChecker` was dropped as unnecessary | `Design-Specification.md` §2, §4–5 |
| Skip-and-retain policy | **Accepted, then clarified by the candidate** (Entry 2) | `Design-Specification.md` §5 |
| Waitlisted learner dropping = leaving the queue | **Accepted** | `Design-Specification.md` §6 |
| Optional thin CLI/API | **Edited in Entry 3**: the candidate chose an ASP.NET Core Web API (controllers) | `Design-Specification.md` §9 |

### Verification
- The AI's analysis was cross-checked against the PDF text (acceptance criteria, deliverables and scope boundaries) by the AI itself; this was not independently confirmed by the candidate at the time. The two gaps (no starter, no rubric) were confirmed against the attachments actually supplied. A full compliance audit against the PDF was later done in Entry 8.
- No code was produced in this step.

---

## Entry 2 – Policy decision, worked example and documentation request

### Activity
Policy definition and documentation scaffolding.

### Prompt (rephrased)
> I agree with skip-and-retain. For example, with waitlist A, B, C, D, E where A, D and E are eligible and B and C are not: if 2 seats open, A and D are promoted and B, C, E stay in that order; if 3 seats open, A, D and E are promoted and B, C stay. Ineligible learners must not be moved to the back. I created a folder `C:\EnrollCourse`. Create a markdown file named for the AI prompts, using rephrased prompts, and record our conversation in detail with prompts and responses. Also create the other markdown files the PDF requires.

### AI response (summary)
- Formalised the candidate's examples as the **promotion algorithm** (`Design-Specification.md` §5): scan the waitlist front to back, promote each eligible learner while a seat is free, and leave ineligible learners untouched in their original relative positions. Both examples are included as worked scenarios and as planned test cases.
- Noted a corollary: a seat that remains free while the waitlist is non-empty means every waitlisted learner is currently ineligible.
- Created the documentation set: this log, a design spec, `README.md`, and a limitations file (renamed and moved to `docs/` in Entry 3).
- Called out edge cases not covered by the examples and chose defaults (see decisions below).

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Candidate's skip-and-retain examples become the canonical promotion scenarios | **Accepted (candidate-authored)** | `Design-Specification.md` §5, tests `TwoSeatsFreed_SkipsIneligibleAndKeepsTheirOrder` and `ThreeSeatsFreed_PromotesEligibleAndRetainsIneligibleInOrder` (originally planned as T-FIFO-2 and T-FIFO-3) |
| Promotion fills **all** open seats in one pass (loop until no seat or no eligible learner remains) | **Accepted** | `Design-Specification.md` §5. This makes the "2 seats" and "3 seats" examples a single algorithm rather than special cases. |
| A new eligible enroll request when a seat is free but the waitlist holds only ineligible learners enrolls directly | **Accepted, flagged as a tradeoff** | `Design-Specification.md` §8, `Limitations.md` L2 |
| A retained learner who later becomes eligible is promoted only at the next promotion pass (no background re-check) | **Accepted as a known limitation** | `Limitations.md` L1 |
| Planned code layout, to be created later | **Accepted** | `README.md` marks commands as planned until the code exists |
| A configurable policy engine for skip/retain | **Rejected**: out of scope per the PDF | `Design-Specification.md` §8 |

### Verification
- The candidate's two examples were traced by hand against the algorithm in `Design-Specification.md` §5; both produce the expected queues (see the worked example in §5).
- Pending: automated tests that encode the same two scenarios. (Closed in Entry 7: `WaitlistTests` encodes both.)

---

## Entry 3 – System design and project structure

### Activity
Architecture and design (no code written).

### Prompt (rephrased)
> Before coding, let's design the system as simply as possible, with no code changes yet. Since there is no starter kit, implement it as an ASP.NET Core Web API using controllers, not minimal API. Here is my proposed folder structure (solution, `src/<Project>.API` with Controllers, Models, DTOs, Interfaces, Services, Repositories and Program.cs, `tests/<Project>.Tests` with Enrollment, Drop, Waitlist and Concurrency test files, a `docs/` folder with the design spec, AI prompt log and limitations, and a root README). Use `EnrollCourse` instead of `CourseRegistration` as the name. Tell me if the folder structure needs any changes, then update the README files to match.

### AI response (summary)
1. **Architecture.** Three thin layers: controller (HTTP mapping only), service (all rules and the lock), repository (plain in-memory storage). One-way dependencies.
2. **Data model.** `Learner` (with completed courses), `CourseOffering` (capacity, prerequisites), and one `Enrollment` record per learner with a `Status` and an increasing `Sequence`. Keying enrollments by learner ID makes duplicates structurally impossible. The waitlist is the waitlisted records ordered by `Sequence`.
3. **Eligibility.** A private service method (all prerequisites completed). The earlier `IEligibilityChecker` idea is dropped for simplicity. Tests change eligibility by editing the seeded `Learner`.
4. **Concurrency.** Singleton service and repository, one `lock` around every public operation including reads. Synchronous service methods.
5. **API contract.** `POST`, `DELETE` and `GET` on `api/enrollments`, with an outcome → HTTP status table (201, 202, 400, 404, 409, 422, 200).
6. **Folder structure review.** The proposed tree is accepted with these changes:
   - Rename to `EnrollCourse.sln`, `EnrollCourse.API`, `EnrollCourse.Tests`; the repo root is `C:\EnrollCourse`, so there is no extra nested folder.
   - Add `DTOs/DropResponse.cs` and `DTOs/OfferingStatusResponse.cs`.
   - Add `Models/EnrollmentStatus.cs` and `Models/EnrollmentOutcome.cs`.
   - Add `Data/SeedData.cs` so `Program.cs` stays small.
   - Add `tests/.../Helpers/` (shared test setup, to avoid duplication) and an optional small `EnrollmentApiTests.cs`.
   - Make `Program` a `public partial class` so API tests can use `WebApplicationFactory`.
   - Add a root `.gitignore` (`bin/`, `obj/`).
   - Note: the .NET 10 SDK creates `.slnx` solutions by default; create a classic `.sln` with `dotnet new sln --format sln` if the `.sln` file name is wanted.
   - Keep one root `README.md` (no duplicate READMEs).

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| ASP.NET Core Web API with controllers | **Accepted (candidate-directed)** | `Design-Specification.md` §1, §9 |
| Candidate's proposed folder structure | **Accepted with the edits above** | `README.md` (folder structure) |
| Drop `IEligibilityChecker`, use a private eligibility method | **Accepted** | `Design-Specification.md` §2, §8. Fewer moving parts. |
| One enrollment record per learner, keyed by learner ID | **Accepted** | `Design-Specification.md` §2. Enforces invariant 2 by construction. |
| `Sequence` counter instead of a timestamp for ordering | **Accepted** | `Design-Specification.md` §2. Timestamps can tie. |
| Lock inside the singleton service, including `GetStatus` | **Accepted** | `Design-Specification.md` §7 |
| Outcome → HTTP status mapping, with 422 for `NotEligible` | **Accepted** | `Design-Specification.md` §9. Implemented and covered by API tests; candidate review remains part of final verification. |
| Re-run the promotion pass on every enroll so a newly eligible waitlisted learner is never overtaken | **Rejected for now**: adds a hidden side effect to enroll and goes beyond the assessment's scope | Kept as a documented limitation (`Limitations.md` L1, L2) |
| Dev-only endpoint to change a learner's completed courses, to demo skip-and-retain over HTTP | **Rejected**: extra surface area; the behaviour is covered by tests | `Design-Specification.md` §8 |

### Verification
- The design was checked against each acceptance criterion in the PDF (AC1–AC7); each maps to a section of the specification or a planned test (the planned test IDs were later replaced by the test table in `Design-Specification.md` §10).
- The folder tree on disk was listed after creation and matches the structure in `README.md`.
- Pending: the HTTP status mapping and the lock approach are verified only once code and tests exist. (Closed in Entry 7: API tests and the smoke test cover the status mapping; mutation checks cover the lock.)

---

## Entry 4 – Design decisions follow-up

### Activity
Design review: HTTP status choice, eligibility behaviour on enroll, dynamic learner data.

### Prompt (rephrased)
> 1) Isn't 403 "forbidden"? What other status code is best for a not-eligible learner? 2) Agreed: an ineligible learner is rejected on enroll, and only a later enroll request, made when they are eligible, should get a seat. 3) Seed data is fine for a simple service, but check whether data can be changed dynamically to support point 2.

### AI response (summary)
1. **Status code.** 403 means authenticated but not authorized, which is an identity concern; this slice has no identity. Options considered: 400 (reserved for malformed input), 409 (already used for duplicates; reusing it would blur the two cases), 403 (wrong meaning), 422 (well-formed request that fails a business rule). Recommendation: keep 422, with the `outcome` field in the body for precision.
2. **Point 2 confirmed.** Eligibility is evaluated fresh on each enroll request. Rejected learners are not remembered or queued. This matches the "no re-check on enroll" decision from Entry 3.
3. **Dynamic data.** Yes, it is possible. Learners live in an in-memory singleton repository as mutable objects, so completed courses can change at runtime. Tests can edit them directly. An HTTP endpoint would also work, but the change must go through the service lock to avoid a data race with running requests. Not added, to keep the slice simple.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Keep 422 for `NotEligible`; reject 403 | **Implemented; candidate should confirm during final self-review** | `Design-Specification.md` §8, §9; verified by API tests |
| Eligibility evaluated per request, no memory of rejected requests | **Accepted (candidate-directed)** | `Design-Specification.md` §4 |
| Keep seed data; tests change learner data directly | **Accepted (candidate-directed)** | `Design-Specification.md` §8, `Limitations.md` L5 |
| Learner-management HTTP endpoint (changes made inside the lock) | **Deferred**: not needed for the assessment | `Design-Specification.md` §8 |

### Verification
- Reasoning check only; no code yet. The claim that learner data can change at runtime was later proven by the `WaitlistTests` that revoke eligibility after a learner has joined the waitlist (originally planned as T-FIFO-5).

---

## Entry 5 – Low-level design and start of implementation

### Activity
Low-level design, toolchain setup, test-first scaffolding.

### Prompt (rephrased)
> Before implementing, give me the low-level design: every class, its properties, and why each exists. Then start the implementation. (Also: update the README/docs with each prompt and response from now on. Forget the PUT endpoint; we will refactor later.)

### AI response (summary)
1. Wrote the low-level design into the design spec (models, DTOs, interfaces, implementations, test support). That section was later condensed in Entry 9 because the code and README now carry the detail.
2. **Toolchain finding.** The machine had no .NET SDK (only the 8.0 runtime). A .NET 10 SDK (10.0.404) was installed to a user-local folder, with no admin rights and no change to the system `dotnet`.
3. Scaffolded the solution (`dotnet new sln --format sln`), a controllers-based Web API, an xUnit test project and a `.gitignore`. Removed the template weather files.
4. **Package issue.** `dotnet add package Microsoft.AspNetCore.Mvc.Testing` picked a version (11.0.0) that does not support net10.0. It was pinned to 10.0.15 instead.
5. Wrote contracts first (models, DTOs, interfaces), then all tests, then ran the build to confirm the **red** state (compile errors: implementation types missing).
6. Wrote the first implementation (repository, service, controller, seed data, `Program.cs`). It has **not yet been built or run**.
7. **Design finding while writing the waitlist tests.** Seats free up one at a time through drops, so the candidate's "2 seats" and "3 seats" examples are reproduced by *sequential* drops (A then D; A, D then E), ending in the same final queue. Several seats are free at once only when every waitlisted learner is ineligible. The pass is still written for any number of seats.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Install the .NET 10 SDK user-locally | **Accepted** | Needed to build at all; matches the assessment email. |
| Pin `Microsoft.AspNetCore.Mvc.Testing` to 10.0.15 | **Edited** (AI tooling default of 11.0.0 rejected) | `EnrollCourse.Tests.csproj` |
| Write tests before implementation | **Accepted** | Org TDD rule; red build confirmed. |
| Scenario A/B tested through sequential drops | **Accepted** | `WaitlistTests.cs`; documented as a design finding. |
| Test-only `AddLearner` on the concrete repository (not on the interface) | **Accepted** | `TestHarness.cs`, repository |

### Verification
- Red phase: `dotnet build` failed with missing-type errors only, as expected.
- Green build and tests: see Entry 7 (the code in this entry was superseded by the simpler models).

---

## Entry 6 – Are the simpler four models enough?

### Activity
Design analysis only (no code changes).

### Prompt (rephrased)
> Why do we need .NET 10 if 8 is available? I think the service is getting complex. Are these four models enough: `Learner` (LearnerId, Name, CompletedCourseIds), `CourseOffering` (OfferingId, CourseName, Capacity, RequiredPrerequisiteCourseId), `Enrollment` (EnrollmentId, LearnerId, OfferingId, EnrolledAtUtc) and `WaitlistEntry` (WaitlistEntryId, LearnerId, OfferingId, AddedAtUtc), with FIFO order kept by the waitlist storage as a queue instead of timestamps? (Clarification: I did not ask for a change. Just analyse whether this is simpler, then we decide.)

### AI response (summary)
- **Verdict:** simpler to *understand*, not smaller by field count. Separate `Enrollment` and `WaitlistEntry` types match the problem statement, and the status flag, sequence counter and promote method disappear. Promotion becomes "move a learner from the waitlist to the enrollments".
- **Caveat 1:** a literal `Queue<T>` cannot do skip-and-retain, because dequeuing B and C and re-enqueuing them would move them to the back, and it cannot remove a learner from the middle when a waitlisted learner drops. A `List<WaitlistEntry>` kept in insertion order gives the same FIFO behaviour.
- **Caveat 2:** a single nullable prerequisite is simpler than a set, with the limit of one prerequisite per offering.
- **Caveat 3:** "a learner is in at most one place" is no longer structural, so the service checks both collections and the repository can reject duplicates.
- **Trims suggested:** `Name`, `EnrollmentId` and `WaitlistEntryId` are never read by any rule. The timestamps are informational and need only an injected `TimeProvider`.
- **.NET version:** the email requires .NET 10, and the machine has no SDK and no ASP.NET Core 8 runtime, so net8.0 would need extra installs.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Adopt the four-model design | **Accepted by the candidate in Entry 7**, with `List` and the trims below | Models, repository, service and tests rewritten. |
| Use `List<WaitlistEntry>` instead of a literal `Queue<T>` | **Accepted** (candidate agreed) | Needed for skip-and-retain. |
| Drop `Name`, `EnrollmentId`, `WaitlistEntryId` | **Accepted** | Unused by any rule. |
| Stay on .NET 10 | **Accepted** | Stated in the assessment email. |

### Verification
- Reasoning check against the acceptance criteria. No code changed in this step.

---

## Entry 7 – Adopting the simpler models and implementing

### Activity
Implementation, test-first verification and mutation checks.

### Prompt (rephrased)
> Use .NET 10. Timestamps/audit entries are not required; make it as simple as possible. A `List` instead of a `Queue` is fine. Use my four models with the trims.

### AI response (summary)
1. Replaced the earlier models (`Learner`, `CourseOffering`, `Enrollment` with status and sequence) with the candidate's design: `Learner(LearnerId, CompletedCourseIds)`, `CourseOffering(OfferingId, CourseName, Capacity, RequiredPrerequisiteCourseId?)`, `Enrollment(LearnerId, OfferingId)`, `WaitlistEntry(LearnerId, OfferingId)`. Removed `Name`, the two entry IDs, the timestamps, the `EnrollmentStatus` enum, the sequence counter and `Promote()`.
2. Rewrote the repository (two lists plus a learner dictionary), the service, the seed data and the test helper. Updated the affected tests. The controller, DTOs and `EnrollmentOutcome` were unchanged, except for the field rename in item 5.
3. First run: **49 of 49 tests passed**, including both skip-and-retain scenarios.
4. Because a first-run green is weak evidence, the review added three **mutation checks** (each reverted afterwards; see Verification).
5. A live smoke test over HTTP found one **defect**: `OfferingStatusResponse.CourseCode` now carried the course *name*. It was renamed to `CourseName` and an assertion added.
6. Removed the template's OpenAPI registration and package. It was not requested, and the source-generated code it adds distorted the coverage figure (34% overall). Swagger/OpenAPI was subsequently enabled in Development; see Entry 20.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Four-model design with the trims | **Accepted (candidate-directed)** | `Models/*`, `Repositories/*`, `Services/EnrollmentService.cs` |
| Repository rejects a learner who already has a record, as a second guard | **Accepted** | `InMemoryEnrollmentRepository.cs`; tested in `CourseOfferingTests` |
| First concurrency tests (one race attempt each) | **Edited**: mutation check 1 showed only one of four tests caught a missing lock, so the other three were repeated for 50 rounds each | `ConcurrencyTests.cs` |
| Rename `CourseCode` to `CourseName` in the status DTO | **Accepted** (found in review) | `OfferingStatusResponse.cs`, `EnrollmentApiTests.cs` |
| Remove OpenAPI | **Accepted at that stage; later reversed when Swagger/OpenAPI was enabled in Development** | Historical files: `Program.cs`, `EnrollCourse.API.csproj`; current state is documented in Entry 20 |
| Pin `Mvc.Testing` to 10.0.15 (tool default 11.0.0 does not support net10.0) | **Edited** | `EnrollCourse.Tests.csproj` |

### Verification
Commands run from `C:\EnrollCourse` using the user-local .NET 10 SDK (10.0.404).

| Check | Command or method | Result |
|---|---|---|
| Build | `dotnet build` | Succeeded, 0 warnings, 0 errors |
| Tests | `dotnet test` | **49 passed, 0 failed** |
| Coverage | `dotnet test --collect:"XPlat Code Coverage"` | **99.0% lines, 94.1% branches**; service and repository 100% lines; controller 90% lines |
| Mutation 1: remove the lock from `Enroll` | edit, `dotnet test`, revert | **3 tests failed** (after strengthening; only 1 failed before) |
| Mutation 2: `continue` changed to `break` in the promotion pass (block instead of skip) | edit, `dotnet test`, revert | **2 tests failed** (both skip-and-retain scenarios) |
| Mutation 3: eligibility always true | edit, `dotnet test`, revert | **7+ tests failed** |
| All mutations reverted | `grep` confirmed three locks, `continue`, original `IsEligible` | Done |
| Live smoke test | built DLL on `localhost:5099`, requests with `Invoke-WebRequest` | Enroll 201 ×2, waitlist 202, duplicate 409, ineligible 422, unknown 404, blank 400, drop 200 with `promoted: ["L3"]`, repeated drop 404, GET snapshot correct. Found the `courseCode` defect (fixed; tests re-run, 49 passed). |

---

## Entry 8 – Compliance audit against the assessment PDF

### Activity
Review of the finished work against the PDF's acceptance criteria, deliverables, scope boundaries and candidate expectations (no code changes).

### Prompt (rephrased)
> Before going further, compare the PDF with what we have built so far and tell me whether we are in sync, mainly against the deliverables listed in the PDF.

### AI response (summary)
- **Result:** all five deliverables and all seven acceptance criteria are met in substance. Mapping and the gaps found are in the table below.
- **Defect found in this record:** Entry 1 claimed the candidate had cross-checked the analysis against the PDF. That was not true as written, and it was corrected.
- **Remaining gaps:** no git repository / GitHub link yet; the rubric file the PDF mentions was never supplied; the record shows AI-run verification only, with no entry yet for the candidate's own review; prompts are rephrased rather than verbatim; the limitations list (17 items) and design spec are longer than "concise" and "proportionate" suggest.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Correct the false verification claim in Entry 1 | **Accepted** (applied) | `AI-Prompt-Log.md` Entry 1 |
| Candidate to run tests and review the code personally, then add their own review notes | **Open (candidate action)** | Needed for the "independent review" expectation |
| Trim `Limitations.md` and tidy its numbering | **Done in Entry 9** | Deliverable asks for a concise summary |
| Ask the recruiter for the rubric file and starter | **Open (candidate action)** | Not supplied with the assessment |

### Verification
- Evidence gathered from the files on disk (document list and sizes, test method names, text search for secrets or personal data in the `.md`, `.cs`, `.json` and `.csproj` files, git status). No secrets were found in the submitted sources; the only personal data was a user-profile path in generated `bin/` and `obj/` files, which `.gitignore` excludes.

---

## Entry 9 – Trimming the limitations and design spec

### Activity
Documentation edit (no code changes, no git setup).

### Prompt (rephrased)
> Just trim the documents; don't initialise git. Also, what does "candid" mean?

### AI response (summary)
- **Limitations:** reduced from 17 rows to 7 by merging related items (in-memory and single-process locking; the out-of-scope extras) and moving two by-design behaviours into a short note. IDs L1 and L2 were kept so existing references stay valid; the seed-data item is now L5.
- **Design spec:** reduced from about 13,300 to about 8,600 characters. The planned test-ID matrix and the class-by-class reference were replaced with a short test table and a pointer to the README folder tree. The three promotion scenarios were merged into one worked example. No rule, invariant or API contract was changed.
- **Error found afterwards (Entry 10):** the trimmed spec, like the earlier one, wrongly said a blank POST `learnerId` reaches the service; it is stopped by model validation first. Corrected in Entry 10.
- **"Candid"** means open and honest, including about unflattering facts. For this assessment it means the record must say plainly what AI wrote, what the candidate changed or rejected, and where the checks were weak.
- Updated stale section and ID references in this log. Git was not initialised, as instructed.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Merge and shorten the limitations | **Accepted (candidate-directed)** | `Limitations.md` |
| Condense the design spec, keeping every rule and contract | **Accepted (candidate-directed)** | `Design-Specification.md` |
| Initialise git | **Deferred at the candidate's request** | Still needed before submission |

### Verification
- Searched the docs for references to the old layout and fixed those that were stale (README, and several log entries). A final search found two leftover references to planned test IDs, which were then fixed. Earlier entries still describe the earlier designs (status flag, sequence counter) as they were at the time; Entry 7 records the change.
- No code touched; tests were not re-run for this step.

---

## Entry 10 – Walkthrough of the initial data and API scenarios

### Activity
Explanation for the candidate's own code review (no code changes).

### Prompt (rephrased)
> I'm about to review the work myself. Don't run anything; explain what data exists when the app starts, and then the flow of scenarios for every API call, with request data and expected results.

### AI response (summary)
- Described the seeded starting state (one offering, capacity 2, prerequisite `CS101`, learners `L1`–`L5` eligible and `L6`–`L7` not, no enrollments).
- Walked through the API scenarios in order: enroll, fill the seats, waitlist, duplicates, ineligible, unknown learner, blank input, drop with promotion, repeated drop, waitlisted learner leaving, and the status read. Each states the request, response and resulting state.
- Stated honestly that skip-and-retain cannot be shown over HTTP with the seed data: `L6`/`L7` are never eligible, so they can never join the waitlist, and nothing changes a waitlisted learner's eligibility. It is covered by `WaitlistTests` only.
- **Defect found while writing it:** the design spec said a blank POST `learnerId` reaches the service and returns `InvalidRequest`. In fact model validation rejects it first (400, problem-details body), as the earlier live test had shown. The spec was corrected.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Correct the blank-ID statement in the API section | **Accepted** (applied) | `Design-Specification.md` §9 |
| Add a test asserting the two different 400 bodies | **Not done**: left for the candidate to decide | `EnrollmentApiTests.cs` |

### Verification
- Expected results come from the live runs recorded in Entry 7 (enroll, waitlist, duplicate, ineligible, unknown, blank, drop, repeated drop, status) and from the passing API tests for the rest (duplicate enrolled 409, drop of a waitlisted learner, blank DELETE). Nothing new was run in this step, and a stray server start I attempted was interrupted; I confirmed that nothing is listening on the port.

---

## Entry 11 – Clarification: an eligible learner posting after the seats are full

### Activity
Explanation (no code changes, nothing run).

### Prompt (rephrased)
> After step 7 in the walkthrough, if a learner who does have the prerequisite (say `L7`, if added to the learners list) posts an enroll request, they will get enrolled, right?

### AI response (summary)
- **Corrected the assumption.** An eligible learner is enrolled only if a seat is free. After step 7 both seats are taken and `L3`, `L4` are waiting, so the result is 202 `Waitlisted` at position 3.
- An eligible learner enrolls directly only when a seat is free; if only ineligible learners are waiting, a newcomer takes the free seat (`Limitations.md` L2).
- With the current seed data `L7` has no `CS101`, so the answer is 422 `NotEligible`. Changing that requires editing `SeedData.cs` and restarting, because learner data cannot be changed over HTTP (`Limitations.md` L5).

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| None (explanation only) | n/a | Consistent with `Design-Specification.md` §4 and §6 |

### Verification
- Reasoned from `EnrollmentService.Enroll` and covered by the existing tests `Enroll_WhenFull_JoinsWaitlistInOrderWithPosition` and `Enroll_WhenSeatFreeAndOnlyIneligibleLearnersWait_EnrollsNewcomerDirectly`. Not run live in this step.

---

## Entry 12 – Duplicate-request scenarios

### Activity
Explanation for the candidate's own review (no code changes, nothing run).

### Prompt (rephrased)
> Help me with a scenario related to duplicates.

### AI response (summary)
- Walked through a fresh-start sequence: duplicate enroll while enrolled (409 `AlreadyEnrolled`), duplicate enroll while waitlisted (409 `AlreadyWaitlisted`, position unchanged), repeated drop (404 `NotEnrolled`, nobody extra promoted), repeated drop of a waitlisted learner (200 `LeftWaitlist`, then 404), re-enrolling after a drop (a new request, goes to the back of the waitlist), simultaneous duplicate requests (exactly one 201), and the repository's duplicate guard as a backstop.
- Stated that the sequence was read from the code and tests, not run end to end.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| None (explanation only) | n/a | Mirrors `EnrollmentTests`, `DropTests`, `ConcurrencyTests`, `CourseOfferingTests` |

### Verification
- Matches the existing passing tests `Enroll_Twice_…`, `Enroll_WhenAlreadyWaitlisted_…`, `Drop_Twice_…`, `Drop_WaitlistedLearner_…`, `Drop_ThenEnrollAgain_GoesToBackOfWaitlist`, `ParallelDuplicateEnrollsOfSameLearner_CreateExactlyOneRecord`, `Repository_AddingSameLearnerTwice_IsRejected`, and the API tests `Post_Duplicate_Returns409` and `Delete_Repeated_Returns404`. The exact HTTP sequence was not run live in this step.

---

## Entry 13 – Lock and concurrency scenario

### Activity
Explanation for the candidate's own review (no code changes, nothing run).

### Prompt (rephrased)
> I want to understand a scenario related to the lock and concurrency.

### AI response (summary)
- Explained the race with a timeline: two learners enrolling at the same instant with one seat left, where both pass the "is a seat free?" check before either adds, so capacity is exceeded.
- Showed the same timeline with the lock: the second request waits, then sees no seat and joins the waitlist.
- Highlighted the design points: one shared lock because the service is a singleton; validate, check, change and promote all under one hold; reads (`GetStatus`) also locked; blank-ID checks before the lock because they read no state; synchronous methods.
- Mapped the four concurrency tests to what each must preserve, cited the mutation result (lock removed: 3 tests fail; one attempt per test caught only 1), and restated the limits (tests are probabilistic, single process only).

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| None (explanation only) | n/a | Mirrors `Design-Specification.md` §7, `Limitations.md` L3 and L6, `ConcurrencyTests.cs` |

### Verification
- The mutation result and test behaviour are from the runs recorded in Entry 7. The race timeline is a reasoned explanation, not a captured trace, and nothing new was run in this step.

---

## Entry 14 – Endpoint to change a learner's prerequisites (skip-and-retain over HTTP)

> **Reversed in Entry 18.** The candidate asked for this endpoint to be removed. This entry is kept as history; the code, tests and doc references it describes no longer exist.

### Activity
Feature change, test-first, with mutation and live verification.

### Prompt (rephrased)
> `L6` has no prerequisite in our data. Can we have an API to update the prerequisite so we can try the skip-and-retain scenario?

### AI response (summary)
1. **Interpretation stated before building.** Changing the *offering's* prerequisite would flip every learner at once and cannot make "B and C ineligible while A, D, E are eligible". The endpoint therefore changes a *learner's* completed courses: `PUT /api/learners/{learnerId}/completed-courses`, body `{"completedCourseIds":[...]}`, which **replaces** the list (`[]` revokes everything).
2. This reverses the candidate's earlier "forget the PUT endpoint, we will refactor later" (Entry 5); the candidate has now asked for it. The Entry 4 option to put it inside the service lock was followed.
3. Wrote the tests first (10 service tests, 5 API tests, and the A–E example end to end over HTTP), saw the build fail, then implemented: `IEnrollmentService.UpdateCompletedCourses`, a new `LearnersController`, `LearnerResponse` and `UpdateCompletedCoursesRequest` DTOs, `Learner.ReplaceCompletedCourses`, and a new `LearnerUpdated` outcome.
4. **Design decision:** updating eligibility never enrolls or promotes anyone, consistent with Limitations L1. Granting a prerequisite to a waitlisted learner does not move them up.
5. **Refactor for DRY:** the outcome → HTTP status table moved from `EnrollmentController` into one `OutcomeStatusCodes` class so both controllers share it.
6. **Unexpected finding (not caused by AI or by me today):** the red build failed with "`IEnrollmentRepository` not found". `Interfaces\IEnrollmentRepository.cs` had been **blanked to 1 byte at 11:19 on 20 Nov**, before this prompt, and it was not modified by any tool call in this session that day. The cause is unknown (possibly an accidental edit in the editor during review). It was restored with the original 799-character contents, and a search found no other damaged file. Because the file was empty, anyone who ran the tests between 11:19 and this restoration would have seen a build failure.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Change a learner's completed courses, not the offering's prerequisite | **Accepted** (candidate asked for a way to change the prerequisite; this is the form that supports skip-and-retain) | `LearnersController.cs`, `EnrollmentService.cs` |
| Replace, not merge | **Accepted** | `Learner.cs`; mutation test below |
| Updating eligibility triggers no promotion | **Accepted**, documented as Limitation L1 | `Design-Specification.md` §4 |
| Leave the endpoint unauthenticated and always on | **Accepted for the demo; flagged as a risk** | `Limitations.md` L5 |
| Restrict the endpoint to the Development environment | **Not done** (adds complexity; `dotnet run` and test hosts both default to Development, so it would not have changed how this was tested) | `Limitations.md` L5 |
| Add a dedicated concurrency test for the PUT lock | **Not done** | Honest gap, recorded in `Limitations.md` L5 |
| Restore the blanked `IEnrollmentRepository.cs` | **Accepted** | Verified by the build and the 68 passing tests |

### Verification
| Check | Result |
|---|---|
| Red phase | Build failed before implementation, as expected (after restoring the blanked file) |
| `dotnet build` | 0 warnings, 0 errors |
| `dotnet test` | **68 passed, 0 failed** (19 new) |
| Coverage | **99.6% lines, 96.7% branches** |
| Mutation: update merges instead of replaces (removed `Clear()`) | **5 tests failed**, including the full HTTP skip-and-retain scenario; reverted and confirmed by search |
| Live run with `dotnet run --project src\EnrollCourse.API --urls http://localhost:5099` | PUT granted `L6`, `L7` the prerequisite (200); `L1`–`L7` enrolled (2 × 201, 5 × 202); PUT `[]` on `L4`, `L5` (200); drop `L1` promoted `L3`; drop `L2` promoted `L6`, skipping `L4`, `L5`; waitlist `L4, L5, L7`; drop `L3` promoted `L7`; final enrolled `L6, L7`, waitlist `L4, L5`. PUT on an unknown learner gave 404; a PUT with no list gave 400 with a problem-details body. |

---

## Entry 15 – Catch-up: two short questions logged late

### Activity
Explanations only (no code changes). These prompts were answered before Entry 14 but not recorded at the time; they are added here, so the entry number is out of chronological order.

### Prompts (rephrased) and responses (summary)
1. *"In which location did you make the changes? Give me the full paths."* Listed every file under `C:\EnrollCourse` (source, tests, docs, solution) from an actual directory listing, and noted the only thing outside the project: the user-local .NET 10 SDK at `C:\Users\mramayanam\.dotnet10\sdk`.
2. *"I did not see any GET endpoint for enrollments."* Checked the controller: the GET exists as `GetStatus` (`[HttpGet]`, inheriting the class route `api/enrollments`, returning course name, capacity, seats available, enrolled and waitlist). It was easy to miss because of the name and the inherited route. Noted there is no per-learner GET; offered to add one or rename the method (not done).

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Per-learner GET or rename `GetStatus` | **Not implemented; outside the single-offering assessment scope, so no action is pending** | `EnrollmentController.cs` exposes the offering-level GET |

### Verification
- Path list taken from a directory listing. The GET endpoint confirmed by searching the controller for `[Http…]` attributes.

---

## Entry 16 – Move the status mapping and PUT action into `EnrollmentController` (incomplete)

### Activity
Code refactor requested by the candidate; the work was **interrupted and left half-done**.

### Prompt (rephrased)
> Don't add `OutcomeStatusCodes` in the Controllers folder; make it a private method instead of a separate class.

### AI response (summary)
- Plan: put `ToStatusCode` back as a private method in `EnrollmentController`, move the PUT action into it with an absolute route (`~/api/learners/...`) so the URL stays the same, and delete `LearnersController.cs` and `OutcomeStatusCodes.cs`.
- `EnrollmentController.cs` was edited as planned. The command that deleted the two files was interrupted and did not run, so as of Entry 17 both files still exist, along with a stray temporary file I created by mistake (`docs\_tmp_placeholder.txt`).
- **Expected consequence, not yet verified:** `LearnersController` and `EnrollmentController` now both map `PUT api/learners/{learnerId}/completed-courses`, which should make that request fail with an ambiguous-route error until the two files are deleted. The build and tests were not re-run after the edit. (Outcome: moot. Entry 18 removed the PUT endpoint, so the duplicate route no longer exists.)

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Private mapping method in the controller instead of a separate class | **Accepted (candidate-directed)**, **not finished** | `EnrollmentController.cs` edited; two files still to delete |
| Create a placeholder file | **Rejected by me as a mistake**; deletion also interrupted | `docs\_tmp_placeholder.txt` |

### Verification
- Read-only directory listing in Entry 17 shows the leftover files. Tests not run since the edit.

---

## Entry 17 – Can records and `sealed` be replaced with normal classes?

### Activity
Explanation only (no code changes, as requested).

### Prompt (rephrased)
> Don't make any code changes. Instead of `record` and `sealed`, can we use normal classes?

### AI response (summary)
- Yes. `sealed` only prevents inheritance, and nothing in the code inherits from these types. `record` gives value equality, `ToString`, `with` copies and short syntax; nothing relies on those, because the repository matches by `LearnerId` and the tests assert on individual properties.
- Records are used for the 4 response DTOs, the 2 request DTOs, `Enrollment` and `WaitlistEntry`. `sealed` is on every class.
- Noted a detail: the tests read responses back from JSON, so classes should keep a parameterless constructor with public setters (or a single matching constructor).
- Reported the half-finished state described in Entry 16.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Replace `record` / `sealed` with normal classes | **Historical only; no current action:** current source uses ordinary classes and no `sealed` modifiers | At the time, no change was made; Entry 20 records the current-source check |
| Finish the cleanup from Entry 16 | **Superseded in Entry 18**: the PUT endpoint was removed entirely, which deleted those files | `LearnersController.cs`, `OutcomeStatusCodes.cs`, `docs\_tmp_placeholder.txt` |

### Verification
- Search of the source and tests for `record` and `sealed`, plus a directory listing. No build or test run.

---

## Entry 18 – Remove the PUT endpoint and its logic

### Activity
Code removal and documentation update.

### Prompt (rephrased)
> I don't want that PUT endpoint; remove the related logic.

### AI response (summary)
- Removed everything that existed only for the PUT: `LearnersController.cs`, `OutcomeStatusCodes.cs`, `UpdateCompletedCoursesRequest.cs`, `LearnerResponse.cs`, `LearnerUpdateTests.cs`, the stray `docs\_tmp_placeholder.txt`, the `UpdateCompletedCourses` method in the service and interface, `Learner.ReplaceCompletedCourses`, the `LearnerUpdated` outcome, the PUT action and the `LearnerUpdated` case in `EnrollmentController`, and the PUT tests and helpers (including the HTTP skip-and-retain test) in `EnrollmentApiTests.cs`.
- `EnrollmentController` keeps the status mapping as a private `ToStatusCode` method, which also settles Entry 16.
- Updated the README (status line, API table, seed-data note, the PUT walkthrough and the folder tree), the design spec (§4, §6, §8, §9, §10) and the limitations (L1, L5).
- **Consequence to know about:** nothing in the running app can now change a learner's eligibility, so skip-and-retain cannot be shown over HTTP. It is covered by `WaitlistTests`. The README says so.

### Decision log
| Suggestion | Outcome | Notes / files informed |
|---|---|---|
| Remove the PUT endpoint and all related code | **Accepted (candidate-directed)**, reverses Entry 14 | Files listed above |
| Keep the A–E walkthrough over HTTP | **Dropped**, it depended on the PUT | `README.md` |
| Keep the `TestHarness.Restore` helper | **Kept**, still used by `EnrollmentTests` and `WaitlistTests` | `tests\EnrollCourse.Tests\Helpers\TestHarness.cs` |

### Verification
| Check | Result |
|---|---|
| Search of all `.cs` files for `UpdateCompletedCourses`, `ReplaceCompletedCourses`, `LearnerUpdated`, `LearnerResponse`, `SetCompletedCourses`, `OutcomeStatusCodes`, `LearnersController`, `api/learners` | No matches |
| Search of the README, design spec and limitations for PUT-related terms and `68` | No matches |
| `dotnet build` | 0 warnings, 0 errors |
| `dotnet test` | **49 passed, 0 failed** |
| Coverage | **99.0% lines, 94.1% branches** (identical to before the PUT was added) |
| Live run | **Not run** in this step. The PUT route should now return 404, which was not checked by hand or by a test. |

---

## Verification evidence (running section)

| Area | Evidence | Status |
|---|---|---|
| Requirement coverage vs. PDF | AI audit in Entry 8: all 5 deliverables and 7 acceptance criteria mapped to files and tests | AI review done; candidate should complete the independent checks in the README before submission |
| Promotion examples (2 seats / 3 seats) | Hand trace in `Design-Specification.md` §5 | Done (manual) |
| Design vs. acceptance criteria | Mapping to spec sections and test IDs | Done (manual) |
| Folder tree matches README | Directory listing | Done |
| Red phase (tests before implementation) | `dotnet build` compile errors for missing types | Done |
| Unit tests (capacity, full, duplicates, FIFO, prerequisite, drops) | `dotnet test`: 49 passed, 0 failed (final state after Entry 18; it was 68 while the PUT endpoint existed) | Done |
| Concurrency tests | 4 parallel tests, 50 rounds each; mutation check shows they detect a missing lock | Done |
| Coverage ≥ 80% on changed code | 99.0% lines, 94.1% branches (after Entry 18) | Done |
| Tests can fail (mutation checks) | 4 mutations, each caught, each reverted. The fourth (Entry 14, update merges instead of replaces) tested code that was removed in Entry 18. | Done |
| End-to-end over HTTP | Live runs; found and fixed one DTO naming defect. The skip-and-retain run in Entry 14 used the removed PUT endpoint and can no longer be reproduced. | Done (see note) |

## Candid summary of AI contribution
- AI contributed requirement analysis, a proposed invariant set, the promotion algorithm formalisation, the first design, the implementation, the tests and the documentation drafts.
- The candidate chose the skip-and-retain policy and supplied the worked examples, chose the Web API shape and folder structure, simplified the model to four classes with a `List` and no timestamps, and chose .NET 10.
- AI-generated code was not trusted on first pass: a first-run green was challenged with mutation checks, which exposed weak concurrency tests (strengthened), and a live run exposed a naming defect (fixed).
- Known weaknesses are listed in `Limitations.md`. The concurrency tests are probabilistic; they were shown to catch a missing lock, but cannot prove races are absent.

---

## Entry 19 – Clarify run location and record documentation follow-ups

### Activity
Documentation updates and validation during a follow-up review.

### Prompts (rephrased)
1. > Align the design-spec README with the actual folder structure, review the repository for related documentation updates, then build, test and verify the APIs.
2. > Use one .NET target version for both the API and tests instead of mixing .NET 8 and .NET 10.
3. > Show the README smoke test as step-by-step commands, with each expected response immediately after its command.
4. > Add a note that the API can also be verified through Swagger UI.
5. > Explain whether `CourseRegistration.http` and `CourseRegistration.csproj.user` are required.
6. > Delete those optional files.
7. > State the exact directory from which to run build, API and smoke-test commands, and record this session's prompts and responses in this log.

### AI responses (summary)
1. Updated the root README and design spec to match the actual repository, routes, test names and project layout; ran the build, all tests and an HTTP smoke test.
2. Changed the API target to `net10.0` to match the test project. The solution built with no warnings or errors, all 49 tests passed, and the .NET 10 API smoke test confirmed enrollment and FIFO promotion.
3. Reworked the README smoke test into seven ordered steps with expected output beside each command.
4. Documented that Swagger UI is available at `http://localhost:5224/swagger` in Development for interactive API requests.
5. Explained that the `.http` file is a convenience for manual API requests and `.csproj.user` files hold local IDE settings; neither is needed to build or run the application.
6. Deleted `CourseRegistration.http` and both project `.csproj.user` files, then removed the deleted HTTP file from the design spec's tree.
7. Added `Set-Location 'C:\CharlesSchwab_AI_Assessment'` as the working-directory instruction for build/run and the separate smoke-test terminal; this entry records the prompts and responses.

### Decision log
| Request | Outcome | Notes / files informed |
|---|---|---|
| Align documentation with the repository and verify the API | **Accepted** | `README.md`, `Design-Specification.md` |
| Use one target framework | **Accepted**: API and tests both target `net10.0` | Both project files; README and design spec |
| Provide sequential smoke-test output and a Swagger UI note | **Accepted** | `README.md` |
| Remove optional HTTP and per-user IDE files | **Accepted** | `CourseRegistration.http`, both `.csproj.user` files; design-spec tree |
| Document the exact working directory and this session's AI use | **Accepted** | `README.md`, this log |

### Verification
| Check | Result |
|---|---|
| `dotnet build CourseRegistration.slnx` | Succeeded; 0 warnings, 0 errors |
| `dotnet test CourseRegistration.slnx --no-build` | 49 passed, 0 failed |
| Live API smoke test on .NET 10 | GET, enrollment, waitlisting and FIFO promotion succeeded |
| Documentation paths and removed files | The root path is stated in the README; removed files and their documentation references were checked |

---

## Entry 20 – Final README completeness and candidate verification status

### Activity
Review all current README-style documents for accuracy, stale statements and outstanding candidate actions.

### Prompt (rephrased)
> The assessment requires me to verify the work myself. Make sure the README files are complete and clarify whether anything remains for me to do.

### AI response (summary)
- Reviewed the root README, design specification, AI prompt log and limitations.
- Corrected the limitations list: Swagger/OpenAPI is enabled in Development, so it is not listed as an out-of-scope feature.
- Clarified that the limitations are intentional scope boundaries and optional future enhancements, not tasks required for the current assessment.
- The source, build and test work is complete. The only remaining candidate-owned action is the independent verification required by the assessment: follow the root README's build/test and API smoke-test steps, and review the AI prompt log for accuracy.
- The existing source check found ordinary classes and no `sealed` modifiers; the old pending decision in Entry 17 is historical, not an outstanding change.

### Decision log
| Request | Outcome | Notes / files informed |
|---|---|---|
| Complete the README set and identify outstanding actions | **Reviewed and clarified** | `README.md`, `Design-Specification.md`, `AI-Prompt-Log.md`, `Limitations.md` |
| Remove contradictions about OpenAPI and pending work | **Corrected** | `Limitations.md`; historical statuses clarified in this log |

### Verification
| Check | Result |
|---|---|
| Documentation set | All four Markdown files reviewed |
| Source scan for `record` declarations / `sealed` modifiers | None found; occurrences of “record” are ordinary text |
| Build and tests | Latest source verification: build succeeded with 0 warnings/errors; 49 tests passed |
| Candidate action | Run the README checks independently and confirm the AI-use record reflects your decisions before submission |
