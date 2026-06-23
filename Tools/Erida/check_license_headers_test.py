#!/usr/bin/env python3
# SPDX-FileCopyrightText: 2026 Yaroslav Yudaev <ydaevy10@gmail.com>
# SPDX-License-Identifier: MIT

import importlib.util
import subprocess
import tempfile
import unittest
from pathlib import Path


SCRIPT_PATH = Path(__file__).with_name("check_license_headers.py")
CANONICAL_COPYRIGHT = "Yaroslav Yudaev <ydaevy10@gmail.com>"


def load_checker():
    if not SCRIPT_PATH.exists():
        raise AssertionError(f"Missing checker script: {SCRIPT_PATH}")

    spec = importlib.util.spec_from_file_location("check_license_headers", SCRIPT_PATH)
    if spec is None or spec.loader is None:
        raise AssertionError(f"Unable to load checker script: {SCRIPT_PATH}")

    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def run_git(repo: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", *args],
        cwd=repo,
        check=True,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )
    return result.stdout.strip()


class LicenseHeaderCheckTest(unittest.TestCase):
    def setUp(self):
        self.checker = load_checker()
        self.tempdir = tempfile.TemporaryDirectory()
        self.repo = Path(self.tempdir.name)
        run_git(self.repo, "init", "-q")
        run_git(self.repo, "config", "user.email", "tests@example.invalid")
        run_git(self.repo, "config", "user.name", "License Header Tests")
        self.write_file("README.md", "# Test repo\n")
        run_git(self.repo, "add", "README.md")
        run_git(self.repo, "commit", "-q", "-m", "initial")
        self.base_ref = run_git(self.repo, "rev-parse", "HEAD")

    def tearDown(self):
        self.tempdir.cleanup()

    def write_file(self, relative_path: str, content, mode: str = "w"):
        path = self.repo / relative_path
        path.parent.mkdir(parents=True, exist_ok=True)
        if "b" in mode:
            path.write_bytes(content)
        else:
            path.write_text(content, encoding="utf-8")

    def commit_added_files(self):
        run_git(self.repo, "add", ".")
        run_git(self.repo, "commit", "-q", "-m", "add files")
        return run_git(self.repo, "rev-parse", "HEAD")

    def commit_all(self, message: str):
        run_git(self.repo, "add", ".")
        run_git(self.repo, "commit", "-q", "-m", message)
        return run_git(self.repo, "rev-parse", "HEAD")

    def check_added_files(self):
        head_ref = self.commit_added_files()
        return self.checker.check_repository(self.repo, self.base_ref, head_ref)

    def test_accepts_mit_spdx_metadata_for_new_file(self):
        self.write_file(
            "Content.Shared/_Erida/PortableThing.cs",
            "// SPDX-FileCopyrightText: 2026 Test Author\n"
            "// SPDX-License-Identifier: MIT\n\n"
            "namespace Content.Shared._Erida;\n",
        )

        result = self.check_added_files()

        self.assertEqual(result.errors, [])
        self.assertEqual(result.warnings, [])
        self.assertIn("Content.Shared/_Erida/PortableThing.cs", result.checked)

    def test_accepts_agpl_spdx_metadata_for_ported_file(self):
        self.write_file(
            "Content.Shared/_Goobstation/PortedThing.cs",
            "// SPDX-FileCopyrightText: 2025 Source Project Authors\n"
            "// SPDX-License-Identifier: AGPL-3.0-or-later\n\n"
            "namespace Content.Shared._Goobstation;\n",
        )

        result = self.check_added_files()

        self.assertEqual(result.errors, [])
        self.assertEqual(result.warnings, [])
        self.assertIn("Content.Shared/_Goobstation/PortedThing.cs", result.checked)

    def test_accepts_adjacent_license_file(self):
        self.write_file("Resources/Prototypes/_Erida/example.json", "{\"id\":\"Example\"}\n")
        self.write_file(
            "Resources/Prototypes/_Erida/example.json.license",
            "SPDX-FileCopyrightText: 2026 Test Author\n"
            "SPDX-License-Identifier: MIT\n",
        )

        result = self.check_added_files()

        self.assertEqual(result.errors, [])
        self.assertEqual(result.warnings, [])
        self.assertIn("Resources/Prototypes/_Erida/example.json", result.checked)

    def test_reports_missing_metadata_with_mit_copyright_and_port_guidance(self):
        self.write_file("Content.Server/_Erida/MissingHeader.cs", "namespace Content.Server._Erida;\n")

        result = self.check_added_files()
        report = self.checker.format_report(result)

        self.assertEqual([issue.path for issue in result.errors], ["Content.Server/_Erida/MissingHeader.cs"])
        self.assertIn("SPDX-FileCopyrightText: 2026 <copyright holder>", report)
        self.assertIn("SPDX-License-Identifier: MIT", report)
        self.assertIn("ported code", report)
        self.assertIn("preserve the source project's license", report)

    def test_tool_files_use_canonical_copyright_holder(self):
        for path in (SCRIPT_PATH, Path(__file__)):
            header = path.read_text(encoding="utf-8").splitlines()[:5]

            self.assertIn(f"# SPDX-FileCopyrightText: 2026 {CANONICAL_COPYRIGHT}", header)

    def test_reports_new_file_with_license_but_without_copyright_as_error(self):
        self.write_file(
            "Content.Server/_Erida/MissingCopyright.cs",
            "// SPDX-License-Identifier: MIT\n\nnamespace Content.Server._Erida;\n",
        )

        result = self.check_added_files()
        report = self.checker.format_report(result)

        self.assertEqual([issue.path for issue in result.errors], ["Content.Server/_Erida/MissingCopyright.cs"])
        self.assertIn("missing SPDX-FileCopyrightText", report)
        self.assertEqual(result.warnings, [])

    def test_formats_sticky_pr_comment_for_error_report(self):
        self.write_file("Content.Server/_Erida/MissingHeader.cs", "namespace Content.Server._Erida;\n")

        result = self.check_added_files()
        report = self.checker.format_report(result)
        comment = self.checker.format_comment(result, report)

        self.assertIn("<!-- erida-license-metadata-check -->", comment)
        self.assertIn("## License metadata check", comment)
        self.assertIn("❌ License metadata check failed.", comment)
        self.assertIn("Content.Server/_Erida/MissingHeader.cs", comment)

    def test_skips_modified_file_without_spdx_metadata(self):
        self.write_file("Content.Server/Existing.cs", "namespace Content.Server;\n")
        self.commit_all("add existing file")
        self.base_ref = run_git(self.repo, "rev-parse", "HEAD")
        self.write_file("Content.Server/Existing.cs", "namespace Content.Server;\n// changed\n")
        head_ref = self.commit_all("modify existing file")

        result = self.checker.check_repository(self.repo, self.base_ref, head_ref)

        self.assertEqual(result.errors, [])
        self.assertEqual(result.warnings, [])

    def test_warns_for_modified_spdx_file_without_copyright_without_failing(self):
        self.write_file(
            "Content.Server/ExistingSpdx.cs",
            "// SPDX-License-Identifier: MIT\n\nnamespace Content.Server;\n",
        )
        self.commit_all("add existing SPDX file")
        self.base_ref = run_git(self.repo, "rev-parse", "HEAD")
        self.write_file(
            "Content.Server/ExistingSpdx.cs",
            "// SPDX-License-Identifier: MIT\n\nnamespace Content.Server;\n// changed\n",
        )
        head_ref = self.commit_all("modify SPDX file")

        result = self.checker.check_repository(self.repo, self.base_ref, head_ref)
        report = self.checker.format_report(result)

        self.assertEqual(result.errors, [])
        self.assertEqual([issue.path for issue in result.warnings], ["Content.Server/ExistingSpdx.cs"])
        self.assertIn("advisory", report)
        self.assertIn("SPDX-FileCopyrightText", report)

    def test_warns_when_modified_spdx_file_lacks_commit_author(self):
        self.write_file(
            "Content.Server/ExistingCopyright.cs",
            "// SPDX-FileCopyrightText: 2025 Existing Holder <holder@example.invalid>\n"
            "// SPDX-License-Identifier: MIT\n\n"
            "namespace Content.Server;\n",
        )
        self.commit_all("add existing copyright file")
        self.base_ref = run_git(self.repo, "rev-parse", "HEAD")
        self.write_file(
            "Content.Server/ExistingCopyright.cs",
            "// SPDX-FileCopyrightText: 2025 Existing Holder <holder@example.invalid>\n"
            "// SPDX-License-Identifier: MIT\n\n"
            "namespace Content.Server;\n// changed\n",
        )
        head_ref = self.commit_all("modify copyright file")

        result = self.checker.check_repository(self.repo, self.base_ref, head_ref)
        report = self.checker.format_report(result)

        self.assertEqual(result.errors, [])
        self.assertEqual([issue.path for issue in result.warnings], ["Content.Server/ExistingCopyright.cs"])
        self.assertIn("License Header Tests", report)
        self.assertIn("maintainers may ask for a correction during review", report)

    def test_skips_binary_files_without_requiring_text_header(self):
        self.write_file("Resources/Textures/_Erida/example.rsi/icon.png", b"\x89PNG\r\n\x1a\n\x00", "wb")

        result = self.check_added_files()

        self.assertEqual(result.errors, [])
        self.assertEqual(result.warnings, [])
        self.assertIn("Resources/Textures/_Erida/example.rsi/icon.png", result.skipped_binary)


if __name__ == "__main__":
    unittest.main()
