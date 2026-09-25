#!/usr/bin/env python3
import json
import os
import re
import subprocess
import sys
from pathlib import Path

GATE_PATH = Path("docs/CANDIDATE_GATE.json")
PLACEHOLDERS = {"", "unknown", "tbd", "todo", "n/a", "na", "?"}


def fail(message):
    print(f"candidate gate: {message}", file=sys.stderr)
    raise SystemExit(1)


def git(*args):
    return subprocess.check_output(["git", *args], text=True).strip()


def changed_paths(commit):
    raw = git("diff-tree", "--no-commit-id", "--name-only", "-r", commit)
    return [line for line in raw.splitlines() if line]


def is_production_path(path):
    return (
        path.startswith("src/")
        or path.startswith("lang/")
        or path.startswith("lang_rebalanced/")
        or path.endswith(".csproj")
        or path in {
            ".github/workflows/build-clarity.yml",
            ".github/workflows/build-rebalanced.yml",
        }
    )


if not GATE_PATH.is_file():
    fail(f"missing {GATE_PATH}; create it from docs/CANDIDATE_GATE_TEMPLATE.json before production edits")

try:
    data = json.loads(GATE_PATH.read_text(encoding="utf-8"))
except Exception as exc:
    fail(f"{GATE_PATH} is not valid JSON: {exc}")

if data.get("schema_version") != 1:
    fail("schema_version must be 1")

branch = os.environ.get("GITHUB_REF_NAME", "").strip()
declared_branch = str(data.get("candidate_branch", "")).strip()
if not declared_branch:
    fail("candidate_branch is required")
if branch and declared_branch != branch:
    fail(f"candidate_branch {declared_branch!r} does not match workflow branch {branch!r}")

baseline = str(data.get("baseline_sha", "")).strip()
if not re.fullmatch(r"[0-9a-fA-F]{40}", baseline):
    fail("baseline_sha must be the exact 40-character pre-change source SHA")

try:
    git("cat-file", "-e", baseline + "^{commit}")
except subprocess.CalledProcessError:
    fail(f"baseline_sha {baseline} is not a commit available in checkout history")

if subprocess.call(["git", "merge-base", "--is-ancestor", baseline, "HEAD"]) != 0:
    fail("baseline_sha must be an ancestor of HEAD")

changes = data.get("changes")
if not isinstance(changes, list) or not changes:
    fail("changes must be a non-empty list")

scalar_fields = (
    "id",
    "observable_property",
    "canonical_owner",
    "final_writer_or_commit_point",
    "acceptance_evidence",
)
list_fields = ("blast_radius", "preserved_invariants", "evidence_refs")

for index, change in enumerate(changes, 1):
    if not isinstance(change, dict):
        fail(f"changes[{index}] must be an object")
    for field in scalar_fields:
        value = str(change.get(field, "")).strip()
        if value.lower() in PLACEHOLDERS:
            fail(f"changes[{index}].{field} is unresolved")
    for field in list_fields:
        value = change.get(field)
        if not isinstance(value, list) or not value or any(not str(item).strip() for item in value):
            fail(f"changes[{index}].{field} must be a non-empty list of concrete entries")
    if change.get("state") != "READY":
        fail(f"changes[{index}].state must be READY for a production candidate")

range_spec = f"{baseline}..HEAD"
gate_commits = git("rev-list", "--reverse", range_spec, "--", str(GATE_PATH)).splitlines()
if not gate_commits:
    fail("the active gate record must be committed after baseline_sha")

gate_commit = gate_commits[0]
gate_changed = changed_paths(gate_commit)
if gate_changed != [str(GATE_PATH)]:
    fail(
        "the first active gate commit must be gate-only; it changed: "
        + ", ".join(gate_changed)
    )

commits = git("rev-list", "--reverse", range_spec).splitlines()
production_commits = [
    commit for commit in commits
    if any(is_production_path(path) for path in changed_paths(commit))
]
if not production_commits:
    fail("no production-source change exists after the declared baseline; no production candidate build is needed")

first_production = production_commits[0]
if gate_commit == first_production or subprocess.call(
    ["git", "merge-base", "--is-ancestor", gate_commit, first_production]
) != 0:
    fail("the READY gate commit must precede the first production-source mutation")

print(
    "candidate gate: READY "
    f"branch={declared_branch} baseline={baseline} "
    f"gate_commit={gate_commit} changes={len(changes)}"
)
