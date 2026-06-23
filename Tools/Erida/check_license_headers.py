#!/usr/bin/env python3
# SPDX-FileCopyrightText: 2026 Yaroslav Yudaev <ydaevy10@gmail.com>
# SPDX-License-Identifier: MIT

import argparse
from dataclasses import dataclass
import json
import os
import re
import subprocess
import sys
from pathlib import Path


HEADER_LINE_LIMIT = 40
COMMENT_MARKER = "<!-- erida-license-metadata-check -->"
SPDX_LICENSE_RE = re.compile(r"\bSPDX-License-Identifier\s*:\s*(.+)")
SPDX_COPYRIGHT_RE = re.compile(r"\bSPDX-FileCopyrightText\s*:\s*(.+)")


@dataclass(frozen=True)
class Author:
    name: str
    email: str

    @property
    def display(self) -> str:
        if self.email:
            return f"{self.name} <{self.email}>"
        return self.name


@dataclass(frozen=True)
class Issue:
    path: str
    message: str


@dataclass(frozen=True)
class Metadata:
    has_license: bool
    copyrights: list[str]

    @property
    def has_copyright(self) -> bool:
        return bool(self.copyrights)

    @property
    def has_any_spdx(self) -> bool:
        return self.has_license or self.has_copyright


class CheckResult:
    def __init__(self):
        self.checked = []
        self.errors = []
        self.warnings = []
        self.skipped_binary = []


def run_git(repo: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", *args],
        cwd=repo,
        check=True,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )
    return result.stdout


def collect_changed_files(repo: Path, base_ref: str, head_ref: str) -> list[tuple[str, str]]:
    output = run_git(
        repo,
        "diff",
        "--name-status",
        "--diff-filter=AM",
        base_ref,
        head_ref,
        "--",
    )
    changed = []
    for line in output.splitlines():
        if not line.strip():
            continue

        status, path = line.split("\t", 1)
        changed.append((status, path.replace("\\", "/")))

    return changed


def collect_file_authors(repo: Path, base_ref: str, head_ref: str, relative_path: str) -> list[Author]:
    output = run_git(
        repo,
        "log",
        "--format=%an%x00%ae",
        f"{base_ref}..{head_ref}",
        "--",
        relative_path,
    )
    authors = []
    seen = set()
    for line in output.splitlines():
        if not line:
            continue

        name, _, email = line.partition("\0")
        author = Author(name.strip(), email.strip())
        key = (author.name.casefold(), author.email.casefold())
        if author.name and key not in seen:
            authors.append(author)
            seen.add(key)

    return authors


def is_license_sidecar(relative_path: str) -> bool:
    return relative_path.endswith(".license")


def is_license_text(relative_path: str) -> bool:
    name = Path(relative_path).name.upper()
    return name.startswith("LICENSE") or name.startswith("COPYING")


def is_fluent_locale(relative_path: str) -> bool:
    return relative_path.endswith(".ftl")


def is_rsi_metadata(relative_path: str) -> bool:
    return relative_path.endswith(".rsi/meta.json")


def read_text_prefix(path: Path) -> tuple[bool, str]:
    data = path.read_bytes()
    if b"\0" in data:
        return False, ""

    try:
        text = data.decode("utf-8")
    except UnicodeDecodeError:
        return False, ""

    lines = text.splitlines()[:HEADER_LINE_LIMIT]
    return True, "\n".join(lines)


def parse_metadata(text: str) -> Metadata:
    copyrights = []
    has_license = False
    for line in text.splitlines():
        license_match = SPDX_LICENSE_RE.search(line)
        if license_match:
            has_license = True

        copyright_match = SPDX_COPYRIGHT_RE.search(line)
        if copyright_match:
            copyrights.append(copyright_match.group(1).strip())

    return Metadata(has_license=has_license, copyrights=copyrights)


def read_sidecar_metadata(path: Path) -> Metadata | None:
    sidecar = Path(str(path) + ".license")
    if not sidecar.exists():
        return None

    is_text, text = read_text_prefix(sidecar)
    if not is_text:
        return None

    return parse_metadata(text)


def has_complete_metadata(metadata: Metadata) -> bool:
    return metadata.has_license and metadata.has_copyright


def missing_metadata_message(metadata: Metadata) -> str:
    missing = []
    if not metadata.has_copyright:
        missing.append("SPDX-FileCopyrightText")
    if not metadata.has_license:
        missing.append("SPDX-License-Identifier")

    return "missing " + " and ".join(missing)


def author_matches_copyright(author: Author, copyrights: list[str]) -> bool:
    joined = "\n".join(copyrights).casefold()
    if author.email and author.email.casefold() in joined:
        return True

    return bool(author.name and author.name.casefold() in joined)


def check_added_file(repo: Path, relative_path: str, result: CheckResult):
    if is_rsi_metadata(relative_path):
        check_added_rsi_metadata(repo / relative_path, relative_path, result)
        return

    if is_license_sidecar(relative_path) or is_license_text(relative_path) or is_fluent_locale(relative_path):
        return

    path = repo / relative_path
    sidecar_metadata = read_sidecar_metadata(path)
    if sidecar_metadata is not None:
        if has_complete_metadata(sidecar_metadata):
            result.checked.append(relative_path)
        else:
            result.errors.append(Issue(relative_path, missing_metadata_message(sidecar_metadata)))
        return

    is_text, text = read_text_prefix(path)
    if not is_text:
        result.skipped_binary.append(relative_path)
        return

    metadata = parse_metadata(text)
    if has_complete_metadata(metadata):
        result.checked.append(relative_path)
    else:
        result.errors.append(Issue(relative_path, missing_metadata_message(metadata)))


def check_added_rsi_metadata(path: Path, relative_path: str, result: CheckResult):
    try:
        metadata = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        result.errors.append(Issue(relative_path, "invalid RSI meta.json"))
        return

    missing = []
    if not str(metadata.get("copyright", "")).strip():
        missing.append("copyright")
    if not str(metadata.get("license", "")).strip():
        missing.append("license")

    if missing:
        result.errors.append(Issue(relative_path, "missing RSI " + " and ".join(missing)))
    else:
        result.checked.append(relative_path)


def check_modified_file(repo: Path, base_ref: str, head_ref: str, relative_path: str, result: CheckResult):
    if is_license_sidecar(relative_path) or is_license_text(relative_path):
        return

    path = repo / relative_path
    sidecar_metadata = read_sidecar_metadata(path)
    if sidecar_metadata is not None and sidecar_metadata.has_any_spdx:
        metadata = sidecar_metadata
    else:
        is_text, text = read_text_prefix(path)
        if not is_text:
            result.skipped_binary.append(relative_path)
            return

        metadata = parse_metadata(text)

    if not metadata.has_any_spdx:
        return

    if not metadata.has_copyright:
        result.warnings.append(
            Issue(
                relative_path,
                "advisory: existing SPDX metadata is missing SPDX-FileCopyrightText",
            )
        )
        return

    missing_authors = [
        author.display
        for author in collect_file_authors(repo, base_ref, head_ref, relative_path)
        if not author_matches_copyright(author, metadata.copyrights)
    ]
    if missing_authors:
        result.warnings.append(
            Issue(
                relative_path,
                "advisory: commit author not found in SPDX-FileCopyrightText: "
                + ", ".join(missing_authors),
            )
        )


def check_repository(repo: Path, base_ref: str, head_ref: str) -> CheckResult:
    result = CheckResult()
    for status, relative_path in collect_changed_files(repo, base_ref, head_ref):
        if status == "A":
            check_added_file(repo, relative_path, result)
        elif status == "M":
            check_modified_file(repo, base_ref, head_ref, relative_path, result)
    return result


def format_report(result: CheckResult) -> str:
    if not result.errors and not result.warnings:
        return (
            "License metadata check passed.\n"
            f"Checked new text files: {len(result.checked)}.\n"
            f"Skipped binary files: {len(result.skipped_binary)}."
        )

    sections = []
    if result.errors:
        errors = "\n".join(f"- {issue.path}: {issue.message}" for issue in result.errors)
        sections.append(
            "License metadata check failed.\n\n"
            "Every new text file must declare copyright and license metadata near the top "
            "of the file, or with an adjacent `.license` sidecar when the format cannot "
            "contain comments.\n\n"
            "Recommended for new original Erida code:\n"
            "  SPDX-FileCopyrightText: 2026 <copyright holder>\n"
            "  SPDX-License-Identifier: MIT\n\n"
            "For ported code, preserve the source project's license and attribution instead "
            "of rewriting it to MIT.\n\n"
            "Files with blocking metadata issues:\n"
            f"{errors}"
        )

    if result.warnings:
        warnings = "\n".join(f"- {issue.path}: {issue.message}" for issue in result.warnings)
        sections.append(
            "Advisory metadata warnings. These do not fail the job, but maintainers may ask "
            "for a correction during review.\n\n"
            "If this change is a copyrightable contribution and you or your organization are "
            "the copyright holder, add a correct SPDX-FileCopyrightText line. For mechanical "
            "changes, ports, or work owned by someone else, keep the correct holder instead.\n\n"
            f"{warnings}"
        )

    return "\n\n".join(sections)


def format_comment(result: CheckResult, report: str) -> str:
    if result.errors:
        status = "❌ License metadata check failed."
    elif result.warnings:
        status = "⚠️ License metadata check passed with advisory warnings."
    else:
        status = "✅ License metadata check passed."

    return (
        f"{COMMENT_MARKER}\n"
        "## License metadata check\n\n"
        f"{status}\n\n"
        "<details>\n"
        "<summary>Details</summary>\n\n"
        "```text\n"
        f"{report.rstrip()}\n"
        "```\n\n"
        "</details>\n"
    )


def escape_github_command(value: str, property_value: bool = False) -> str:
    value = value.replace("%", "%25").replace("\r", "%0D").replace("\n", "%0A")
    if property_value:
        value = value.replace(":", "%3A").replace(",", "%2C")
    return value


def print_github_annotations(result: CheckResult):
    error_message = (
        "Missing required SPDX metadata. Recommended for new original Erida code: "
        "SPDX-FileCopyrightText: 2026 <copyright holder>; SPDX-License-Identifier: MIT. "
        "For ported code, preserve the source project's license and attribution."
    )
    for issue in result.errors:
        print(
            "::error "
            f"file={escape_github_command(issue.path, True)},"
            "line=1,"
            "title=Missing license metadata::"
            f"{escape_github_command(error_message)}"
        )

    warning_message = (
        "Advisory SPDX metadata warning. If this change is a copyrightable contribution "
        "and you or your organization are the copyright holder, add a correct "
        "SPDX-FileCopyrightText line."
    )
    for issue in result.warnings:
        print(
            "::warning "
            f"file={escape_github_command(issue.path, True)},"
            "line=1,"
            "title=Review SPDX copyright metadata::"
            f"{escape_github_command(warning_message)}"
        )


def write_github_summary(report: str):
    summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if not summary_path:
        return

    with open(summary_path, "a", encoding="utf-8") as summary:
        summary.write("## License metadata check\n\n")
        summary.write(report)
        summary.write("\n")


def write_comment(path: str, result: CheckResult, report: str):
    comment_path = Path(path)
    comment_path.parent.mkdir(parents=True, exist_ok=True)
    comment_path.write_text(format_comment(result, report), encoding="utf-8")


def parse_args(argv: list[str]):
    parser = argparse.ArgumentParser(description="Check REUSE/SPDX metadata for changed files.")
    parser.add_argument("--repo", default=".", help="Repository root. Defaults to current directory.")
    parser.add_argument("--base-ref", required=True, help="Base git ref or SHA for the PR diff.")
    parser.add_argument("--head-ref", required=True, help="Head git ref or SHA for the PR diff.")
    parser.add_argument(
        "--format",
        choices=("text", "github"),
        default="text",
        help="Use github to emit workflow annotations and a step summary.",
    )
    parser.add_argument(
        "--comment-path",
        help="Write a sticky PR comment body to this path.",
    )
    return parser.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    repo = Path(args.repo).resolve()
    result = check_repository(repo, args.base_ref, args.head_ref)
    report = format_report(result)

    if args.format == "github":
        print_github_annotations(result)
        write_github_summary(report)

    if args.comment_path:
        write_comment(args.comment_path, result, report)

    print(report)
    return 1 if result.errors else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
