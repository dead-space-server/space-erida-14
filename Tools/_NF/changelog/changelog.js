const fs = require("fs");
const yaml = require("js-yaml");
const axios = require("axios");

// Use GitHub token if available
if (process.env.GITHUB_TOKEN) axios.defaults.headers.common["Authorization"] = `Bearer ${process.env.GITHUB_TOKEN}`;

// Check changelog directory.
if (!process.env.CHANGELOG_DIR) {
    console.log("CHANGELOG_DIR not defined, exiting.");
    return process.exit(1);
}

const ChangelogFilePath = `../../../${process.env.CHANGELOG_DIR}`

// Regexes
const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9а-яё_\-,& ]+)?/img;
const EntryRegex = /^ *[*-]? *(add|remove|tweak|fix|добавлено|удалено|изменено|исправлено): *([^\n\r]+)\r?$/img;
const CommentRegex = /<!--.*?-->/gs; // HTML comments

// Main function
async function main() {
    const pr = await axios.get(`https://api.github.com/repos/${process.env.GITHUB_REPOSITORY}/pulls/${process.env.PR_NUMBER}`);
    const { merged_at, body, user, title } = pr.data;

    let commentlessBody = (body || "").replace(CommentRegex, '');

    const headerMatch = HeaderRegex.exec(commentlessBody);
    if (!headerMatch) {
        console.log("No changelog entry found, skipping");
        return;
    }

    let author = headerMatch[1];
    if (!author) {
        console.log("No author found, setting it to author of the PR\n");
        author = user.login;
    } else {
        author = author.trim()
    }

    commentlessBody = commentlessBody.slice(HeaderRegex.lastIndex);

    const entries = getChanges(commentlessBody);
    if (entries.length <= 0) {
        console.log("PR has a changelog header but no valid entries. Either remove the changelog completely, or use entries like '- Добавлено: текст' / '- add: text'.");
        return process.exit(1);
    }

    let time = merged_at;
    if (time) {
        time = time.replace("z", ".0000000+00:00").replace("Z", ".0000000+00:00");
    }
    else {
        console.log("Pull request was not merged, skipping");
        return;
    }

    const entry = {
        author: author,
        changes: entries,
        id: getHighestCLNumber() + 1,
        time: time,
        url: `https://github.com/${process.env.GITHUB_REPOSITORY}/pull/${process.env.PR_NUMBER}`,
        avatar_url: user.avatar_url,
        title: title
    };

    // Erida start
    if (!writeChangelog(entry)) {
        return;
    }
    // Erida end

    console.log(`Changelog updated with changes from PR #${process.env.PR_NUMBER}`);
}

function getChanges(body) {
    const matches = [];
    const entries = [];

    for (const match of body.matchAll(EntryRegex)) {
        matches.push([match[1], match[2]]);
    }

    if (!matches) {
        console.log("No changes found, skipping");
        return;
    }


    matches.forEach((entry) => {
        let type;

        switch (entry[0].toLowerCase()) {
            case "add":
            case "добавлено":
                type = "Add";
                break;
            case "remove":
            case "удалено":
                type = "Remove";
                break;
            case "tweak":
            case "изменено":
                type = "Tweak";
                break;
            case "fix":
            case "исправлено":
                type = "Fix";
                break;
            default:
                break;
        }

        if (type) {
            entries.push({
                type: type,
                message: entry[1],
            });
        }
    });

    return entries;
}

function getHighestCLNumber() {
    const file = fs.readFileSync(ChangelogFilePath, "utf8");

    const data = yaml.load(file);
    const entries = data && data.Entries ? Array.from(data.Entries) : [];
    const clNumbers = entries.map((entry) => entry.id);

    return Math.max(...clNumbers, 0);
}

function writeChangelog(entry) {
    let data = { Entries: [] };

    if (fs.existsSync(ChangelogFilePath)) {
        const file = fs.readFileSync(ChangelogFilePath, "utf8");
        data = yaml.load(file);
    }

    data ??= { Entries: [] };
    data.Entries ??= [];

    // Erida start
    if (data.Entries.some((existing) => existing.url === entry.url)) {
        console.log(`Changelog already contains changes from PR #${process.env.PR_NUMBER}, skipping`);
        return false;
    }
    // Erida end

    data.Entries.push(entry);

    const metadata = { ...data };
    delete metadata.Entries;
    const metadataYaml = Object.keys(metadata).length > 0
        ? yaml.dump(metadata, { indent: 2 }).replace(/^---\n?/, "")
        : "";

    fs.writeFileSync(
        ChangelogFilePath,
        metadataYaml +
        "Entries:\n" +
        yaml.dump(data.Entries, { indent: 2 }).replace(/^---\n?/, "")
    );

    return true;
}

main();
