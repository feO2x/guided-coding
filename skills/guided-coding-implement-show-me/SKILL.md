---
name: guided-coding-implement-show-me
description: Instruct a user how to implement a Guided Coding Frozen Plan by presenting and explaining complete code. Run only when explicitly requested by the user.
license: MIT
---

# Show the User How to Implement a Frozen Plan

Your goal is to teach the user how to implement a Guided Coding Frozen Plan by breaking it up into useful teachable milestones. You output one code fragment at a time for one milestone and explain it; they enter it, execute feedback loops and manual tests, and ask about whatever is unclear. This skill is intended for users at the Beginning stage, so please provide the complete code of a fragment, do not leave any parts out. The user should not figure out parts of the implementation by themselves.

Let the user make every change to the repository themselves. Typing the code by hand is where a good part of the learning happens, so encourage that over copying and pasting, and leave committing and publishing to them.

## 1. Establish the Target

Use the plan named by the user. If none is named, proceed only when there is exactly one plan in `ai-plans/` that has all its Acceptance Criteria unchecked, and it has the latest timestamp of all plans. Otherwise, ask for its path.

Verify that the plan is frozen: its file name has a timestamp, and it has a `*Frozen at ...*` line below its title. If either marker is missing, explain that the Planning Phase is unfinished and stop.

## 2. Create the Milestone Roadmap

Break the plan into milestones and present them as a short roadmap, not showing any code yet.

A good milestone depends on the size of the plan: you could use file-by-file for smaller plans, or break the work into vertical slices if the plan is larger. A single milestone should produce a compilable codebase where all feedback loops pass and at least one commit can be created. Additionally, you can instruct the user to do manual testing, e.g., for UI changes.

It is totally fine if you break up a plan into a single milestone. We trust your teaching expertise here, look at the extent of the plan and consider how you can teach users the corresponding concepts effectively through one or several milestones.

## 3. How to Work Through a Single Milestone

When you begin working with the user on a new milestone, first output a description of the changes it introduces to the codebase from a high-level perspective, and which parts of the plan it addresses. Ask the user whether they understood this. 

Then continue by presenting code to the user. Please do not output all code at once, but fragment-by-fragment so that the user can comprehend the changes step-by-step and build up their mental model of the codebase over time. Verify that each fragment was entered correctly once the user signals completion.

After all code fragments are in place, instruct the user how to run feedback loop commands to verify the changes, or how to execute manual tests. Before they run these, it is worth asking what they expect to happen and why - one question, not a quiz. This tells you whether the explanation actually landed.

The user might ask questions about details of the code at any point - be helpful here. If something does not compile or a test fails, let the user read the error first, explain what to focus on in the error message (for example, exceptions carry a lot of information).

Once the user signals readiness, you verified the milestone behaves as described, and a commit was created by the user, let the user tick the corresponding Acceptance Criteria in the plan from `- [ ]` to `- [x]` and move to the next milestone or finish the Implementing Phase.

## 4. Handle Plan Issues

If a plan decision is wrong or an Acceptance Criterion cannot be met as written, try to solve it or find a workaround. If a problem genuinely cannot be solved, that's totally fine - simply report it to the user.

Ideally, you can catch this directly while you are creating the milestones for the plan, but you might also encounter an issue while the user is working through a milestone. It is up to you to decide whether the Implementation Phase should be interrupted or aborted if you need external input to solve the plan problem.

In the Guiding Phase, the reviewer can decide how to proceed with your findings.

## 5. After the Last Milestone

Summarize everything you and the user have accomplished and tell them to go over to the Guiding Phase. If you didn't face any plan issues, all Acceptance Criteria should be ticked.
