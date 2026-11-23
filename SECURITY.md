# Security policy

## Supported versions

Security fixes are applied to the latest released version of PRISM.

| Version | Supported |
|---|---|
| 1.1.x | Yes |
| Older | No |

## Reporting a vulnerability

Please do not open a public issue for a security problem.

Report it privately through GitHub's **Security → Report a vulnerability** flow for this repository, or contact the maintainers directly. Include:

- A short description of the issue
- Steps to reproduce
- The affected version
- Any relevant logs, without personal data

You should receive an acknowledgement within a few days. Please allow time for a fix and a coordinated disclosure before publishing details.

## Scope

PRISM is a local Windows overlay. It does not use a backend, telemetry, accounts or network services. Relevant security areas include:

- Unsafe handling of local settings and imported JSON
- Privilege boundaries (the app runs per user, without elevation)
- Installer and uninstaller behavior
- Supply-chain risk in build/release tooling

## Not a game cheat

PRISM does not read game memory, inject code into game processes, bypass anti-cheat systems or modify game files. Reports asking for such functionality are out of scope.
