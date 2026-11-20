# Limitations and Follow-up Risks

These are intentional scope limitations and optional future enhancements, not outstanding steps required to build, test, or verify the current assessment implementation.

| ID | Limitation / risk | Follow-up |
|---|---|---|
| L1 | **No re-check trigger.** A retained (skipped) learner who becomes eligible is promoted only at the next drop, so a free seat can sit unused meanwhile. | Add a "re-evaluate waitlist" operation, or run the promotion pass when eligibility data changes. |
| L2 | **A newcomer can take a free seat.** If a seat is free and only ineligible learners wait, a new eligible learner enrolls directly. A retained learner who then becomes eligible has missed it. FIFO among eligible learners still holds. | Policy decision: hold the seat, or promote on re-check. |
| L3 | **In-memory, single process.** State is lost on restart, and the lock protects one process only, so multiple instances would oversubscribe. | Use a persistent store with transactions or constraints. |
| L4 | **Simple prerequisite rule.** One optional required course per offering; no multiple prerequisites, grades, co-requisites or waivers. | Extend `CourseOffering` and `EnrollmentService.IsEligible`. |
| L5 | **Seed data only.** The offering and learners are hardcoded, and nothing in the running app can change a learner's completed courses (an endpoint for this was built and then removed at the candidate's request). Skip-and-retain therefore cannot be shown over HTTP; it is demonstrated by `WaitlistTests` only. | Add a learner endpoint that changes data inside the service lock, with authentication. |
| L6 | **Concurrency tests are probabilistic.** They repeat 50 rounds each and were shown to fail when the lock is removed, but cannot prove races are absent. | Review the lock scope; run the tests repeatedly in CI. |
| L7 | **Deliberately left out of scope:** identity and authorization, audit timestamps, waitlist size limit, promotion notifications, capacity changes. Swagger/OpenAPI documentation is available in Development. | Add when the feature grows beyond this slice. |

Behaviours that are by design, not defects (see `Design-Specification.md` §4 and §8): a learner rejected as ineligible is not remembered and must enroll again; a learner who drops and re-enrolls goes to the back of the waitlist.
