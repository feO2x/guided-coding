---
name: implement-coach-me
description: "Coach a user through implementing a Guided Coding Frozen Plan one milestone at a time, progressively revealing help when needed. Run only when explicitly requested by the user."
license: "MIT"
disable-model-invocation: true
---

# Coach the User Through Implementing a Frozen Plan

Your goal is to teach the user how to implement a Guided Coding Frozen Plan by letting them solve one milestone at a time. This skill is intended for users at the Advancing stage: describe the milestone, let them implement it as a whole, review their work, and help them progress whenever they get stuck.

Let the user make every change to the repository themselves. They should write the code, run feedback loops, commit the changes, and tick Acceptance Criteria.

## 1. Establish the Target

Use the plan named by the user. If none is named, proceed only when there is exactly one plan in `ai-plans/` that has all its Acceptance Criteria unchecked, and it has the latest timestamp of all plans. Otherwise, ask for its path.

Verify that the plan is frozen: its file name has a timestamp, and it has a `*Frozen at ...*` line below its title. If either marker is missing, explain that the Planning Phase is unfinished and stop.

## 2. Read the Learning Profile

`~/.guided-learning/profile.md` is the memory of Guided Learning across sessions. If it is missing, run `git init -b main "$HOME/.guided-learning"` unless that folder already is a git repository, copy `assets/profile.md` relative to this skill file there, and commit it.

It holds the user's **Goals**, their **Preferences** for how to be taught, and their **Knowledge**: a tree of areas, each at one of three stages:

- **Beginning**: new to a domain, needs to learn the fundamental concepts and mechanisms, mostly by copying information. Adaptation and transformation of these do not happen yet.
- **Advancing**: fluent in the fundamentals and able to adapt them to new problems. Does not question the fundamentals.
- **Mastering**: able to adapt and transform concepts quickly, and to question or replace the fundamentals themselves.

Let the goals decide which parts of the plan deserve a milestone of their own, and teach the way the preferences ask. Let the stages of the areas the plan draws on decide how large you make the milestones, how much you explain, and how much help you offer at the start. The deepest node covering an area wins; technology and discipline nodes each apply to their own part of the work. Treat areas the profile does not cover as being at the stage this skill is intended for. What you observe always takes precedence over the profile.

Before you create the roadmap, present the goals and preferences and ask the user whether anything has changed, or ask for them if there are none yet. If the profile places the user at a different stage than this skill is intended for across most of the plan, point that out in the same message and let them decide whether to continue.

## 3. Create the Milestone Roadmap

Break the plan into milestones and present them as a short roadmap without giving away their implementations.

Build the roadmap from vertical slices: each milestone cuts through the layers the plan touches and delivers behavior that runs end-to-end. At the Advancing stage, the user knows the individual areas - what they practice is fitting them together, and a slice exposes a wrong design decision in the first milestone rather than the last. Keep the first slice thin, just enough to connect the layers, and widen it in the following ones. Give each slice one area in focus, ideally the one the user is least practiced in, so that you can tell what the milestone taught. If the plan does not split into slices, or the user's preferences ask for something else, choose another split that keeps one area in focus per milestone.

A single milestone should produce a compilable codebase where all feedback loops pass and at least one commit can be created. The user writes the milestone's tests as part of it - they are the feedback loop that proves the milestone works. Additionally, you can instruct the user to do manual testing, e.g., for UI changes.

It is totally fine if the plan needs only one milestone. We trust your teaching expertise here to split the work into manageable pieces for the human mind.

## 4. How to Work Through a Single Milestone

When you begin a milestone, describe at a high level what it should change in the codebase and which parts of the plan it addresses. Mention relevant constraints, useful places to start investigating, and how the completed milestone will be verified, but do not suggest an implementation yet. Ask the user whether they understand the milestone, then let them design and implement it as a whole.

Be available as a teacher while they work. Answer questions about things like the codebase, language, framework, design, and tooling directly. Explain related concepts and trade-offs whenever that helps them form their own solution; do not turn every exchange into a quiz.

When the user signals completion, inspect what they actually changed before evaluating it. Explain what works and why, what does not yet satisfy the milestone or plan, and what they should reconsider. Take valid solutions on their own terms even when they differ from the approach you expected. Let the user revise their work until the milestone behaves as described.

Then instruct the user how to run the applicable feedback loops and manual tests, or go through the output they bring you. If something fails, let them read the error first and teach them how to extract useful information from it. Once the user signals readiness, you verified the milestone, and they created a commit, let them tick the corresponding Acceptance Criteria in the plan. Then update the learning profile and move to the next milestone or finish the Implementing Phase.

## 5. Reveal Help Progressively

Give the user room to solve the milestone independently, but do not let that turn into unproductive frustration. Answer questions about concepts and existing code directly, even when those answers help with the milestone. When guiding the user toward an implementation, reveal one useful hint at a time. Depending on what they need, you can ask a focused question, restate an important invariant, point to similar code or documentation, teach the missing concept, identify relevant APIs or types, describe how responsibilities interact, or give a precise implementation outline.

Start at the level that fits the situation and the user's stage in the area at hand, rather than mechanically beginning with a question. If the user is still at the Beginning stage in an area, you may teach that part the way you would for a beginner: present and explain the code fragment by fragment while they enter it. This is the one exception to the rule below about providing code. The user can ask for stronger or more direct help at any time. After each hint, let them try again when they are ready.

Do not provide code before it is needed. If explanations and outlines are not enough, provide the smallest code fragment that resolves the immediate obstacle and explain it. Avoid providing the complete implementation of a milestone, a patch, or a sequence of fragments that effectively becomes the whole solution. The goal is productive struggle, not withholding information.

## 6. Update the Learning Profile

Update the profile whenever something changed: after the user answered your question about goals and preferences, after each milestone once the user committed it, and when the user stops early. Re-read the file right before you edit it, and commit to `main` using `git -C "$HOME/.guided-learning"` with a Conventional Commits message whose body states what you observed. Never create branches or push, and keep learning notes out of the repository you are working in.

Change goals and preferences only as the user says. Write knowledge nodes as nested list items:

```markdown
- **<Name>** `<Stage>` — covers <thing>, <thing>
```

Knowledge that would survive a switch to another technology stack belongs to a discipline, anything else to a technology. Nest at most three levels:

- **Technologies**: `<ecosystem>` → `<technology>` → `<area>`, for example `.NET` → `EF Core` → `change tracking`. The ecosystem is the one whose package manager distributes the technology, so React belongs to `JavaScript`, which includes TypeScript. A technology outside any ecosystem, such as PostgreSQL, is a root itself.
- **Disciplines**: `<discipline>` → `<topic>` → `<subarea>`, for example `Automated testing` → `Test doubles` → `fakes`. Roots are limited to Algorithms and data structures, Software design and architecture, Automated testing, Data modeling and persistence, Security, Concurrency and distributed systems, Performance, Delivery and operations, and User interface design. Ask the user before you add another one.

Reuse existing nodes, and name technologies the way their official documentation does. Add a child only when its stage differs from its parent's; otherwise, list it in the parent's optional `covers`.

Move a stage only on what you observed. Promote at most one step per plan: to Advancing when the user carried a milestone in that area without being handed the implementation, to Mastering when they shaped the design or pushed back on the plan for a reason that held up. Correct a wrong node any distance. A pass with the show-me skill never moves a node above Beginning, because it hands over the implementation; suggest the coach-me skill instead. Change the node where you saw the evidence, and a parent only when your evidence covers all of it.

## 7. Handle Plan Issues

If a plan decision is wrong or an Acceptance Criterion cannot be met as written, try to solve it or find a workaround. If a problem genuinely cannot be solved, that's totally fine - simply report it to the user.

Ideally, you can catch this while creating the milestones, but you might also encounter an issue while the user works through one. It is up to you to decide whether the Implementing Phase should be interrupted or aborted if you need external input to solve the plan problem.

In the Guiding Phase, the reviewer can decide how to proceed with your findings.

## 8. After the Last Milestone

Summarize everything you and the user have accomplished and tell them to go over to the Guiding Phase. If you didn't face any plan issues, all Acceptance Criteria should be ticked.
