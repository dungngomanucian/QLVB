# QLVB Setup

## Clone and run

1. Clone repository.
2. Open `QLVB.sln` with Visual Studio 2022.
3. Restore NuGet packages in Visual Studio (`Restore NuGet Packages`).
4. Build solution.

## Notes

- Internal runtime libraries are stored in `VCAMart/libs`:
  - `LIB.dll`
  - `ric.db.dll`
  - `CKFinder.dll`
- Project references no longer depend on external machine paths such as `..\LIB` or `..\..\..\AIPlatform`.
- `packages` folder is ignored by Git; dependencies are restored from `packages.config`.
