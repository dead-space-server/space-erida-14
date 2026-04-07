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
const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9а-я_\-,& ]+)?/img;
// const HeaderRegex = /^\s*(?::cl:|🆑) *([a-z0-9_\-, ]+)?/img; // :cl: or 🆑 [0] followed by optional author name [1]
const EntryRegex = /^ *[*-]? *(add|remove|tweak|fix): *([^\n\r]+)\r?$/img; // * or - followed by change type [0] and change message [1]
const CommentRegex = /<!--.*?-->/gs; // HTML comments

// Main function
async function main() {
    const pr = await axios.get(`https://api.github.com/repos/${process.env.GITHUB_REPOSITORY}/pulls/${process.env.PR_NUMBER}`);
    const { merged_at, body, user, title } = pr.data;

    commentlessBody = body.replace(CommentRegex, '');

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

    writeChangelog(entry);

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
                type = "Add";
                break;
            case "remove":
                type = "Remove";
                break;
            case "tweak":
                type = "Tweak";
                break;
            case "fix":
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

    data.Entries.push(entry);

    fs.writeFileSync(
        ChangelogFilePath,
        "Entries:\n" +
        yaml.dump(data.Entries, { indent: 2 }).replace(/^---/, "")
    );
}

main();
