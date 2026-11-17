---
name: guided-coding-implement-show-me
description: Instruct a user how to implement a Guided Coding Frozen Plan by presenting and explaining complete code. Run only when explicitly requested by the user.
license: MIT
---

# Show the User How to Implement a Frozen Plan

Your goal is to teach the user how to implement a Guided Coding Frozen Plan by splitting it into teachable milestones. For each milestone, you present and explain the code one fragment at a time. The user enters it, runs the feedback loops and manual tests, and asks about whatever is unclear. This skill is intended for the Beginning stage, so always present complete fragments: the user should not have to figure out any part of the implementation.

The user makes every lasting change to the repository, including commits. Typing the code by hand is where much of the learning happens, so encourage it over copying and pasting.

## 1. Establish the Target

Use the plan named by the user. If none is named, proceed only if exactly one plan in `ai-plans/` has all acceptance criteria unchecked and that plan also has the latest timestamp. Otherwise, ask for its path.

Verify that the plan is frozen: its filename has a timestamp, and it has a `*Frozen at ...*` line below its title. If either marker is missing, explain that the Planning Phase is unfinished and stop.

## 2. Read the Learning Profile

`~/.guided-learning/profile.md` stores the user's learning preferences and progress across sessions. If it is missing, run `git init -b main "$HOME/.guided-learning"` unless that folder is already a Git repository. Copy this skill's `assets/profile.md` into that folder and commit it.

The profile holds the user's **Preferences**, such as the language to teach in and whether to explain concepts before showing code. It also records their **Knowledge** as a tree of areas, each at one of three stages:

- **Beginning**: new to a domain. Learns fundamental concepts and mechanisms mainly by following examples and cannot yet adapt them independently.
- **Advancing**: fluent in the fundamentals and able to adapt them to new problems. Does not question the fundamentals.
- **Mastering**: able to adapt and transform concepts quickly, and to question or replace the fundamentals themselves.

Follow the user's teaching preferences. Use their stage in each relevant area to choose milestone size, explanation depth, and initial support. Use the most specific profile entry that covers the area. Technology and discipline entries each apply to their own part of the work. For areas the profile does not cover, use this skill's intended stage. Your observations take precedence over the profile.

If you created the profile in this session, ask the user how they like to be taught before you create the roadmap. Otherwise, restate the preferences in one line when you present the roadmap, so that the user can object.

## 3. Create the Milestone Roadmap

Before you create the roadmap, verify that the types, members, and files the plan describes as existing are present and that its claims about existing behavior match what the code currently does. Do not treat planned additions or behavior changes as missing prerequisites.

If reading the code is not enough, use temporary tests or projects to check the actual behavior. A simulation alone does not confirm it. Remove these temporary tests or projects before presenting the roadmap, since the user makes every lasting change to the repository. Handle what you find as described in section 6.

Present the milestones as a short roadmap without any code.

Build it layer by layer, each milestone putting one area in focus and building on the previous ones. For a backend feature, this could be the domain model, then database access, a service, and finally the endpoint. Since nothing runs end-to-end before the last layer, explain in the roadmap how the layers will connect, and remind the user where the current layer sits whenever a milestone begins. If the plan has no layers, or the preferences ask for something else, choose another split that keeps one area in focus per milestone.

At the end of each milestone, the code must compile, pass the feedback loops, and be ready to commit. Its tests are part of the code you present, since they prove the milestone works. Add manual tests where needed, for example, for UI changes. A single milestone is fine if that teaches the plan best.

## 4. How to Work Through a Single Milestone

Begin each milestone with a high-level description of the changes it introduces and which parts of the plan it addresses, and ask whether the user understands it.

Then present the code fragment by fragment, never all at once, so that the user builds their mental model of the codebase step by step. Verify each fragment once the user has entered it.

When all fragments are in place, explain how to run the feedback loops or manual tests. Before the user runs them, ask what they expect to happen and why. Their answer tells you whether your explanation landed.

Answer questions about the code at any point. When something does not compile or a test fails, let the user read the error first, then point out what to focus on, for example, the information an exception carries.

Once you have verified that the milestone behaves as described, let the user check off any acceptance criteria it fully satisfies, from `- [ ]` to `- [x]`, and commit those updates with the milestone. Then update the learning profile and move on.

## 5. Update the Learning Profile

Update the profile whenever something changed: when the user expresses a preference, after each milestone once the user committed it, and when the user stops early. Re-read the file right before you edit it, and commit to `main` using `git -C "$HOME/.guided-learning"` with a Conventional Commits message whose body states what you observed. Never create branches or push, and keep learning notes out of the repository you are working in.

Change preferences only as the user says. Write knowledge entries as nested list items:

```markdown
- **<Name>** `<Stage>`: covers <thing>, <thing>
```

Record knowledge that applies across technology stacks under a discipline. Record other knowledge under a technology. Nest at most three levels:

- **Technologies**: `<ecosystem>` → `<technology>` → `<area>`, for example `.NET` → `EF Core` → `change tracking`. The ecosystem is the one whose package manager distributes the technology, so React belongs to `JavaScript`, which includes TypeScript. A technology outside any ecosystem, such as PostgreSQL, is a root itself.
- **Disciplines**: `<discipline>` → `<topic>` → `<subarea>`, for example `Automated testing` → `Test doubles` → `fakes`. Roots are limited to Algorithms and data structures, Software design and architecture, Automated testing, Data modeling and persistence, Security, Concurrency and distributed systems, Performance, Delivery and operations, and User interface design. Ask the user before you add another one.

After each milestone, add an entry for every area it drew on that the profile does not cover yet. Reuse existing entries, and name technologies the way their official documentation does. Add a child entry only when its stage differs from its parent's. Otherwise, list it in the parent's optional `covers`.

Change a stage only when supported by your observations. Advance it by at most one stage per plan:

- To Advancing when the user completed a milestone in that area without being given the implementation.
- To Mastering when they shaped the design or challenged the plan with a valid reason.

When correcting an inaccurate stage, you may move the entry by more than one stage. This skill never moves an entry above Beginning, because it provides the implementation. Change the entry where you observed the evidence. Change a parent entry only when the evidence covers its whole area.

## 6. Handle Plan Issues

If a discrepancy does not affect the plan's decisions or intended behavior, such as a moved file or renamed private member, mention it and proceed.

If the plan contains an incorrect decision or factual claim, or an acceptance criterion cannot be met as written, a workaround is allowed only when all of these conditions are met:

1. Evidence from a test, temporary project, or compiler error confirms the error.
2. The plan's Rationale and all its other decisions remain unchanged.
3. The workaround makes no public API, scope, or dependency changes beyond those in the plan.
4. No test is weakened or removed.

Discuss the workaround with the user before it goes into the code, then teach it like any other part of the plan. Leave an acceptance criterion unchecked if it is met only through a workaround. During the Guiding Phase, the reviewer checks it off if they accept the workaround. If they reject it, they call for a Follow-Up Plan.

If any of these four conditions is not met, do not work around the problem. Tell the user what the plan says, what you observed and how, and the options you see. Ask the user how to proceed, and pause the affected work until they answer.

## 7. After the Last Milestone

Summarize what you and the user accomplished and point them to the Guiding Phase. Unless you faced plan issues, all acceptance criteria should be checked off.

In the summary, list for the reviewer:

- Each workaround: the decision it departs from, the evidence, what was done instead, and the acceptance criteria left unchecked because of it.
- Each problem that was not worked around: what the plan says, what you observed and how, the options you considered, and what the user decided.

Reference decisions by the plan's filename and Decision ID, such as `<plan-file-name> D3`. If the plan has no Decision IDs, name its filename and section, and quote the decision.
