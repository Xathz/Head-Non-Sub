# Head Non-Sub

Discord bot for the PaymoneyWubby server. C# / .NET 8, single project in `Head Non-Sub/`, no tests.
Build: `dotnet build "Head Non-Sub.sln"`

## Working preferences

- Keep code comments simple: short, plain, one line where possible.
- Branch names describe the change (e.g. `fix/cooldown-dm-check`, `feat/members-command`), never random names.
- Follow the existing code style below rather than introducing new patterns.

## Code style

- Namespaces: block-scoped, root `HeadNonSub`, with a blank line after the opening brace and before the closing brace of namespace and class.
- Braces: K&R (opening brace on the same line) for namespaces, classes, methods, and control flow.
- Indent with 4 spaces.
- `using` directives at the top, `System.*` first, then the rest alphabetically.
- Explicit types over `var` (e.g. `MemoryStream memoryStream = new MemoryStream();`).
- Private fields: `_PascalCase` (e.g. `_Seconds`, `_CacheKeys`), `readonly` where possible.
- Public members, properties, and methods: PascalCase. Async methods end in `Async`.
- XML `/// <summary>` docs on public methods and classes, with `<param>` tags where there are parameters.
- Inline comments are short sentences without a trailing period (e.g. `// Download and parse top level domains`).
- Logging goes through `LoggingManager.Log.Debug/Info/Error(...)` with interpolated strings.
- Expression-bodied members for simple one-liners.
- `if/else` chains with `} else {` on one line.
- Discord commands live in `Clients/Discord/Commands`, inherit `BetterModuleBase`, and reply with `BetterReplyAsync`.
- Line endings are normalized by `.gitattributes` (`* text=auto`); older files have a UTF-8 BOM, so keep a file's existing encoding.

## Commits

- Conventional Commits (`feat:`, `fix:`, `refactor:`, `chore:`, `misc:`), short imperative subject, sentence case after the prefix
  (e.g. `fix: Hosting monitor crash`, `chore: Update dependencies`).
- See `CONTRIBUTING.md`: the maintainer does not accept PRs that add commands.
- No Claude Code links or footers in commits, PR descriptions, or GitHub comments
  (no `Claude-Session:` URL, no "Generated with Claude Code" lines).
