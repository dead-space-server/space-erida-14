<div align="center">
    <h1>【 Erida 】</h1>
    <h3></h3>
</div>

<div align="center">

![](https://img.shields.io/github/last-commit/dead-space-server/space-erida-14?&style=for-the-badge&color=8ad7eb&logo=git&logoColor=D9E0EE&labelColor=1E202B)
![](https://img.shields.io/github/languages/top/dead-space-server/space-erida-14?&style=for-the-badge&color=8ad7eb&logo=git&logoColor=D9E0EE&labelColor=1E202B)
![](https://img.shields.io/github/repo-size/dead-space-server/space-erida-14?color=86dbce&label=SIZE&logo=protondrive&style=for-the-badge&logoColor=D9E0EE&labelColor=1E202B)
<a href="https://discord.gg/FC9EGuS7zm">
  <img alt="Discord" src="https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fdiscordapp.com%2Fapi%2Finvites%2FFC9EGuS7zm%3Fwith_counts%3Dtrue&query=approximate_member_count&style=for-the-badge&logo=discord&logoColor=D9E0EE&label=discord&labelColor=%231E202B&color=86dbc0">
</a>

</div>

<div align="center">
    <h2>• overview •</h2>
    <h3></h3>
</div>

<details>
<summary>Contributing</summary>

- We are happy to accept contributions from anybody.
- Get in [Discord](https://discord.gg/FC9EGuS7zm) if you want to help.

</details>

<details>
<summary>Links</summary>

- [Discord server](https://discord.gg/FC9EGuS7zm)
- [Wiki](https://wiki.deadspace14.net/%D0%AD%D1%80%D0%B8%D0%B4%D0%B0:%D0%97%D0%B0%D0%B3%D0%BB%D0%B0%D0%B2%D0%BD%D0%B0%D1%8F_%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0)

</details>

<details>
<summary>How to connect (SS14)</summary>

- To connect to the server, use the following address: ```ss14s://erida.deadspace14.net```
- Open Space Station 14 and paste this link into the server browser or use direct connect.

</details>

<div align="center">
    <h2>• building •</h2>
    <h3></h3>
</div>

<div align="center">
  Refer to <a href="https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html">
  the Space Wizards' guide</a> on setting up a development environment for general information.
  <br><br>
  We provide some scripts shown below to make the job easier.
</div>

<details>
<summary>Build dependencies</summary>

- Git
- .NET SDK 10.0

</details>
<details>
<summary>Windows</summary>

1. Clone this repository
2. Run `Tools/RunScripts/bat/updateEngine.bat` in a terminal or file explorer to download the engine
3. Run `Tools/RunScripts/bat/buildAllDebug.bat` after making any changes to the source
4. Run `Tools/RunScripts/bat/runQuickAll.bat` to launch the client and the server
5. Connect to localhost in the client and play

</details>

<details>
<summary>Linux</summary>

1. Clone this repository
2. Run `Tools/RunScripts/sh/updateEngine.sh` in a terminal to download the engine
3. Run `Tools/RunScripts/sh/buildAllDebug.sh` after making any changes to the source
4. Run `Tools/RunScripts/sh/runQuickAll.sh` to launch the client and the server
5. Connect to localhost in the client and play

</details>

<div align="center">
    <h2>• license •</h2>
    <h3></h3>
</div>

<details>
<summary>License</summary>

### Code
This project is licensed under **AGPL-3.0-or-later**.

Each file contains REUSE-compatible headers or `.license` files that may specify **dual licensing** options.
This allows parts of the codebase to be reused under alternative licenses where applicable.

Full license texts can be found in the `LICENSES/` directory.

---

### Assets
Most media assets are licensed under **CC-BY-SA 3.0**, unless stated otherwise.

- Assets include their own license and copyright information in metadata files
- Example: [asset metadata](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json)

---

### Commercial use
Some assets are licensed under **CC-BY-NC-SA 3.0** or similar non-commercial licenses.

> These assets **must be removed** if you intend to use this project commercially.

</details>

<details>
<summary> Attribution</summary>

We organize borrowed content from other forks into dedicated subdirectories.

This helps preserve proper attribution and reduces merge conflicts.

Content inside these subdirectories originates from their respective forks and may include modifications.
Changes are marked with comments around the edited lines.

---

### Fork sources

| Subdirectory | Fork Name | Repository | License |
|--------------|----------|------------|---------|
| `_NF` | Frontier Station | https://github.com/new-frontiers-14/frontier-station-14 | AGPL 3.0 |
| `_Corvax` | Corvax | https://github.com/space-syndicate/space-station-14 | MIT |
| `_DV` | Delta-V | https://github.com/DeltaV-Station/Delta-v | AGPL 3.0 |
| `_Erida` | Erida | https://github.com/dead-space-server/space-erida-14 | AGPL 3.0 |
| `_EstacaoPirata` | Estacao Pirata | https://github.com/Day-OS/estacao-pirata-14 | AGPL 3.0 |
| `_Goobstation` | Goob Station | https://github.com/Goob-Station/Goob-Station | AGPL 3.0 |
| `_Lavaland` | Goob Station | https://github.com/Goob-Station/Goob-Station | AGPL 3.0 |
| `_Lua` | Lua Frontier | https://github.com/Lua-Frontier/sector-frontier-14 | AGPL 3.0 |
| `_Impstation` | Impstation | https://github.com/impstation/imp-station-14 | AGPL 3.0 |
| `_NC14` | Nuclear 14 | https://github.com/Vault-Overseers/nuclear-14 | AGPL 3.0 |
| `Nyanotrasen` | Nyanotrasen | https://github.com/Nyanotrasen/Nyanotrasen | MIT |
| `Orion` | Orion Station | https://github.com/AtaraxiaSpaceFoundation/Orion-Station-14 | AGPL 3.0 |
| `_White` | WhiteDream | https://github.com/WWhiteDreamProject/wwdpublic | AGPL 3.0 |

---


### Additional sources

The following repositories were used without creating dedicated subdirectories:

| Fork Name | Repository | License |
|-----------|------------|---------|
| Space Station 14 | https://github.com/space-wizards/space-station-14 | MIT |

</details>

![License](https://img.shields.io/badge/license-AGPL--3.0-blue?style=for-the-badge)
