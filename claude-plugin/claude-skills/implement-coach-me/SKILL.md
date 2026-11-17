---
name: implement-coach-me
description: "Coach a user through implementing a Guided Coding Frozen Plan one milestone at a time, progressively revealing help when needed. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Coach the User Through Implementing a Frozen Plan

Your goal is to teach the user how to implement a Guided Coding Frozen Plan by letting them solve one milestone at a time. This skill is intended for the Advancing stage: describe each milestone, let the user implement it as a whole, review their work, and help them whenever they get stuck.

The user makes every lasting change to the repository: they write the code, run the feedback loops, check off acceptance criteria, and commit.

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

If reading the code is not enough, use temporary tests or projects to check the actual behavior. A simulation alone does not confirm it. Remove these temporary tests or projects before presenting the roadmap, since the user makes every lasting change to the repository. Handle what you find as described in section 7.

Present the milestones as a short roadmap without giving away their implementations.

Make each milestone a small, end-to-end change across the relevant layers (a vertical slice). Keep the first slice just large enough to connect the layers, then expand it in later milestones. This lets the user practice connecting familiar areas and exposes design mistakes early.

Focus each slice on one area, ideally the one the user has practiced least, so you can assess what they learned. If vertical slices do not suit the plan or the user's preferences, choose another split with one focus per milestone.

At the end of each milestone, the code must compile, pass the feedback loops, and be ready to commit. The user writes the milestone's tests as part of it, since they prove it works. Add manual tests where needed, for example, for UI changes. A single milestone is fine if the plan is small enough.

## 4. How to Work Through a Single Milestone

Begin each milestone with a high-level description of what it should change and which parts of the plan it addresses. Mention relevant constraints, good places to start investigating, and how the milestone will be verified, but no implementation. Ask whether the user understands it, then let them design and implement it as a whole.

While they work, answer questions about the codebase, language, framework, design, and tooling directly, and explain concepts and trade-offs whenever that helps them form their own solution.

When the user signals completion, inspect what they actually changed. Explain what works and why, what does not yet satisfy the milestone or plan, and what to reconsider. Take valid solutions on their own terms, even when they differ from what you expected, and let the user revise until the milestone behaves as described.

Then explain how to run the feedback loops and manual tests, or go through the output the user brings. When something fails, let them read the error first and teach them how to extract what matters from it. Once you have verified that the milestone behaves as described, let the user check off any acceptance criteria it fully satisfies, from `- [ ]` to `- [x]`, and commit those updates with the milestone. Then update the learning profile and move on.

## 5. Reveal Help Progressively

Give the user room to solve the milestone independently, but do not let it turn into unproductive frustration. Answer questions about concepts and existing code directly, even when the answers help with the milestone. When helping with the implementation, give one hint at a time. Hints range from least to most direct:

1. Ask a focused question.
2. Restate an important invariant.
3. Point to similar code or documentation.
4. Teach a missing concept.
5. Name relevant APIs or types.
6. Describe how responsibilities interact.
7. Give a precise implementation outline.

Choose the starting level that fits the situation and the user's stage in the area. You do not have to start with a question. The user can ask for more direct help at any time. After each hint, let them try again.

Provide code only when explanations and outlines are not enough, and then only the smallest fragment that resolves the immediate obstacle, with an explanation. Never hand over the milestone's complete implementation, a patch, or a series of fragments that amounts to one. The goal is productive struggle, not withholding information.

## 6. Update the Learning Profile

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

When correcting an inaccurate stage, you may move the entry by more than one stage. Change the entry where you observed the evidence. Change a parent entry only when the evidence covers its whole area.

## 7. Handle Plan Issues

If a discrepancy does not affect the plan's decisions or intended behavior, such as a moved file or renamed private member, mention it and proceed.

If the plan contains an incorrect decision or factual claim, or an acceptance criterion cannot be met as written, a workaround is allowed only when all of these conditions are met:

1. Evidence from a test, temporary project, or compiler error confirms the error.
2. The plan's Rationale and all its other decisions remain unchanged.
3. The workaround makes no public API, scope, or dependency changes beyond those in the plan.
4. No test is weakened or removed.

Discuss the workaround with the user before it goes into the code, then teach it like any other part of the plan. Leave an acceptance criterion unchecked if it is met only through a workaround. During the Guiding Phase, the reviewer checks it off if they accept the workaround. If they reject it, they call for a Follow-Up Plan.

If any of these four conditions is not met, do not work around the problem. Tell the user what the plan says, what you observed and how, and the options you see. Ask the user how to proceed, and pause the affected work until they answer.

## 8. After the Last Milestone

Summarize what you and the user accomplished and point them to the Guiding Phase. Unless you faced plan issues, all acceptance criteria should be checked off.

In the summary, list for the reviewer:

- Each workaround: the decision it departs from, the evidence, what was done instead, and the acceptance criteria left unchecked because of it.
- Each problem that was not worked around: what the plan says, what you observed and how, the options you considered, and what the user decided.

Reference decisions by the plan's filename and Decision ID, such as `<plan-file-name> D3`. If the plan has no Decision IDs, name its filename and section, and quote the decision.
