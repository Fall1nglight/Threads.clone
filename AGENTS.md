# Repository Instructions

## Overall

- In planning mode, keep explanatory text minimal as possible. Represent the proposed changes primarily through concrete code snippets, declarations, signatures; explain only what cannot be made clear from the code itself. For example, write `interface User { ... }` instead of “create a new `User` interface.”

## Backend

- Use the repo-local tool manifest in `backend/dotnet-tools.json`; run `dotnet tool restore` from `backend/` if `dotnet csharpier` is unavailable.
- Before finishing any backend C# code change, run `dotnet csharpier format .` from `backend/`.
- Before committing backend C# changes, run `dotnet csharpier check .` from `backend/`.
- After formatting, run the relevant tests ONLY if asked explicitly. For full backend validation, use `dotnet test backend\Threads.sln --verbosity minimal` from the repository root.

## Frontend
