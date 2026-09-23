# Guided Learning Profile

*Updated YYYY-MM-DDTHH:mmZ*

This file tracks what I have learned across Guided Learning sessions. It is private to me: a learning
aid, not a performance record, and it is not shared, committed to a project repository, or pushed to
a public remote. I can edit or delete anything in it at any time. The last section is written for
the coding agent that maintains it.

## Goals

*No entries yet.*

## Preferences

*No entries yet.*

## Carry Forward

*No entries yet.*

## Knowledge

*No entries yet.*

## Recurring Themes

*No entries yet.*

## For the Agent Reading This

### Stages

Every node in the knowledge tree carries one of Guided Coding's three stages, nothing in between:

- **Beginning** - learning the concepts and mechanisms of an area, mostly by applying them as shown.
- **Advancing** - fluent in the fundamentals and able to adapt them to new problems.
- **Mastering** - able to transform concepts quickly, and to question or replace the fundamentals
  themselves.

### Reading This File

- Use it to choose an opening: how large the milestones are, how much to explain, and how much help
  to offer at the start. What happens in the session overrides this file - record what you observe,
  not what you expected.
- **Goals, Preferences, and Carry Forward come first.** Let the goals decide which parts of a plan
  deserve to be the explicit subject of a milestone, teach the way the preferences ask, and pick up
  the carry-forward items that apply to the plan.
- **The most specific node wins.** A node's stage applies to everything beneath it except where a
  child says otherwise. Look for the deepest node covering what the plan needs, and fall back up the
  tree when there is none. The `covers` list of a node tells you which things it includes without
  a node of their own.
- **Entries decay.** A node dated more than six months ago is a prior worth re-testing, not a fact.
- **Evidence** says where a node came from: `self-reported` (I said so) or the file name of a record
  in `records/`, which holds the detail behind it. Read a record only when the node itself is not
  enough.
- This file says what I have done and when. It is not a list of things I cannot do.

### When to Update This File

After each milestone, once I committed it, and once more when the pass ends - after the last
milestone, or earlier when I stop. Never in the middle of a milestone.

- **After a milestone:** append the milestone's section to the record, update the nodes it touched,
  and add or remove carry-forward items.
- **When the pass ends:** append the outcome to the record and update the recurring themes.

Ask me once per session whether you may keep this file up to date; if I agree, write each update
without asking again, but show me what you changed. Re-read this file right before each write and
apply your changes to what is there - I or another session may have changed it since you last read
it. If this folder is a git repository, commit each update with a message that names the record.
Replace a `*No entries yet.*` placeholder once its section has content.

All dates and times in this file and in `records/` are UTC. Take them from the command line rather
than guessing them:

- `date -u +%FT%H:%MZ` on Unix-based shells
- `(Get-Date).ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm'Z'")` on PowerShell

This gives an ISO 8601 timestamp such as `2026-09-17T02:31Z`. Set the `*Updated ...*` line to it
when you change this file. Node dates use only its date part, `2026-09-17`, and record file names
use it as `2026-09-17-0231`.

### Records

One file per implementation pass in `records/`, named `<timestamp>-<repository>-<short-title>.md`
with a `YYYY-MM-DD-HHMM` timestamp. Create it after the first milestone and only ever append to it -
never edit what is already there:

- **Header**, written with the file: the plan's title, the time the pass started, the repository,
  the plan's path, the skill that ran, and the number of milestones in the roadmap.
- **One section per milestone**, appended after it: which areas it touched and how each went, where
  the time went, and what unblocked it. In a `show-me` pass, also note whether my prediction before
  running the feedback loops was right, and which questions I asked.
- **Outcome**, appended when the pass ends: how far the pass got, which Acceptance Criteria were
  ticked, and anything worth carrying into the next pass. A record without an outcome belongs to a
  pass that is still running or was abandoned.

Keep to what you observed. Evidence rather than verdicts - "needed the mechanism explained before it
clicked" is useful, "intermediate at TypeScript" is not.

### Goals and Preferences

Both are mine to state. Goals are what I want to get better at; preferences are how I like to be
taught - for example, explanation before code, the language to use, or how direct hints should be.
Add an entry only when I told you about it in the session, and reword or remove one only when I ask.

### Carry Forward

Short, actionable items for future passes, each naming the record it came from, for example
"Start Satori from problems, not worked examples - the mechanics landed." Add them from the record
you are writing, and remove an item once a pass has addressed it or it no longer applies. Keep the
list short; if an item stays untouched for six months, propose removing it.

### Nodes

Write each node as a list item, nesting children beneath their parent:

```markdown
- **<Name>** `<Stage>` · <YYYY-MM-DD> · <evidence> — covers <thing>, <thing>
```

A node is a body of knowledge a Frozen Plan draws on. Roots are
either technologies, such as `.NET` or `TypeScript`, or disciplines that cut across technologies,
such as `Software design and architecture` or `Automated testing`. Check the tree before adding
anything: if the new thing fits inside an existing node, reuse it.

Nest at most three levels deep - for a technology root, for example platform, technology, area:
`.NET` → `EF Core` → `change tracking`. Add a child only when its stage differs from its parent's.
When it would agree, list it in the parent's `covers` instead: the list records what a node includes
without a node of its own, so `Node build tooling` covers `Satori, sharp, tsx`. The `covers` part is
optional when the name says enough. A child that agrees with its parent is noise, so fold it into
the parent's `covers`. Anything finer belongs in a record.

When a pass touches a node without changing its stage, still update its date and evidence, so that
the decay rule stays meaningful.

### Stage Changes

A stage moves for one of two reasons:

- **Promotion**, because I grew into it: one step per pass, and only on something you observed.
  Beginning to Advancing means I carried a milestone in that area without needing the implementation
  handed to me. Advancing to Mastering means I shaped the design myself, or pushed back on the plan's
  approach for a reason that held up.
- **Correction**, because the node was wrong: any distance, straight away. A `self-reported` node
  that turns out to be Mastering was never Beginning, and nobody was promoted. Correcting a node that
  a record backs is worth a sentence in the new record explaining what changed your mind.

A `show-me` pass never sets a node above Beginning, not even as a correction: it hands me the
implementation, so it cannot show that I would manage without. If I seem further along than
Beginning, say so, suggest `coach-me`, and note it in the record.

Record the change on the node where you saw it. Evidence about structural typing moves that node,
not everything above it. Before you move a parent, check whether your evidence covers the whole
area: everything in its `covers` list and every area without a node of its own moves with it. If
the evidence covers only part of it, add a child for that part instead.

### Recurring Themes

A theme is a pattern in how I learn rather than in what I know - for example, which analogies I reach
for and whether they help. Mark each theme `candidate` or `established`, and name the records it was
seen in:

```markdown
- **<Theme>** `<candidate | established>` - <what you saw>. Seen in: `<record>`, `<record>`.
```

Add a theme as `candidate` when the record you are writing shows a pattern. Mark it `established`
when a later pass shows it again, and remove it when later passes contradict it. You only need the
current pass and this list for that - do not re-read old records to hunt for patterns. Keep at most
five themes; when a sixth would be added, propose which one to drop.
