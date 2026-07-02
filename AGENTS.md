# Repository Instructions

## Backend C# Formatting

- Before finishing any backend C# code change, run `dotnet csharpier format .` from `backend/`.
- Before committing backend C# changes, run `dotnet csharpier check .` from `backend/`.
- After formatting, run the relevant tests. For full backend validation, use `dotnet test backend\Threads.sln --verbosity minimal` from the repository root.
- Use the repo-local tool manifest in `backend/dotnet-tools.json`; run `dotnet tool restore` from `backend/` if `dotnet csharpier` is unavailable.
