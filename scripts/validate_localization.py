#!/usr/bin/env python3
import json
import re
import sys
from collections import Counter
from pathlib import Path

OFFICIAL = ("en", "fr", "de", "zh_cn", "es", "pt_br", "ko", "ja", "ru", "it", "pl")
LITERAL_CONTROL = re.compile(r"\\[nrt]")
PLACEHOLDER = re.compile(r"(?<!\{)\{(\d+)(?::[^{}]+)?\}(?!\})")


def fail(message):
    print(f"localization validation: {message}", file=sys.stderr)
    raise SystemExit(1)


def signature(text):
    return Counter(PLACEHOLDER.findall(text))


roots = [Path(arg) for arg in sys.argv[1:]]
if not roots:
    roots = [Path("lang"), Path("lang_rebalanced")]

for root in roots:
    files = sorted(root.glob("*.json"))
    actual = tuple(path.stem for path in files)
    if set(actual) != set(OFFICIAL) or len(files) != len(OFFICIAL):
        fail(f"{root}: expected exactly {OFFICIAL}, found {actual}")

    parsed = {}
    for path in files:
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except Exception as exc:
            fail(f"{path}: invalid JSON: {exc}")
        if not isinstance(data, dict) or any(not isinstance(v, str) for v in data.values()):
            fail(f"{path}: localization file must be a flat string-to-string object")
        parsed[path.stem] = data

    english = parsed["en"]
    english_keys = set(english)
    for locale in OFFICIAL:
        data = parsed[locale]
        keys = set(data)
        if keys != english_keys:
            missing = sorted(english_keys - keys)
            extra = sorted(keys - english_keys)
            fail(f"{root}/{locale}.json: key mismatch; missing={missing}, extra={extra}")

        for key, value in data.items():
            if LITERAL_CONTROL.search(value):
                fail(
                    f"{root}/{locale}.json:{key}: contains a literal escaped control sequence "
                    r"(\\n/\\r/\\t); use the actual JSON escape so the parser returns the control character"
                )
            if signature(value) != signature(english[key]):
                fail(
                    f"{root}/{locale}.json:{key}: format placeholders {signature(value)} "
                    f"do not match English {signature(english[key])}"
                )

print("localization validation: OK " + ", ".join(str(root) for root in roots))
