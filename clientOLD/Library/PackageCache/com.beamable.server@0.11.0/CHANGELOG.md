# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.11.0]
### Changed
- Changed response serialization from Microservice. Use UseLegacySerialization to maintain old behaviour.

### Added
- Added a cache for content fetches. 
- Added Doc Url to package.json.
- Added Open API 3.0 auto-generative documentation.
- Added AdminOnlyCallable version of ClientCallable that requires caller to have full scopes.
- Added requiredScopes field to ClientCallable that requires caller to have required scopes.

### Fixed
- Fixes leaderboard and stat fetching cache errors.
- Fixes null reference bug when passing null to [ClientCallable] arrays.