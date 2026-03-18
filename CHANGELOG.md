# Changelog

All notable changes to the **StatEngine** project will be documented in this file.

## [2.0.0] - 2026-03-17
### Added
- **100% Free AI (Groq)**: Integrated Groq (Llama 3.3) as the default LLM provider for zero-cost operation.
- **Automated Image Generation**: Integrated Pollinations.ai to generate visuals for every statistic.
- **Manual Draft Workflow**: Added `FileDraftBroadcaster` to save tweets and images locally, bypassing Twitter Free API limits.
- **Enhanced Provider Scope**: Expanded to include Migration, Religion, and specialized domain pods.
- **Robust Testing**: Added comprehensive **xUnit** test suite with **Moq** for ensuring core logic reliability.

### Changed
- Refactored `Worker.cs` to include the Image Generation and Local Draft step.
- Optimized image prompts for better reliability and faster generation.

## [1.1.0] - 2026-03-17
### Added
- **Multi-Domain Support**: Expand beyond finance and sports to include Education, Fashion, Lifestyle, Religion, and more.
- **Environment Variable Integration**: Secrets are now prioritized via environment variables for enhanced security.
- **Premium Documentation**: Added rich-aesthetic README, Detailed Setup Guide, and Changelog.
- **Free LLM Guidance**: Included instructions for local execution via Ollama.

### Fixed
- WorldBank JSON parsing for complex nested array responses.
- Missing using directives and file locking issues during execution.

## [1.0.0] - 2026-03-16
### Added
- Initial Hexagonal Architecture (Core, Infrastructure, Worker).
- `WorldBankProvider` integration.
- `LiteDbCache` for NoSQL deduplication.
- `LlmSocialFormatter` with Semantic Kernel.
- `TwitterBroadcaster` via TweetinviAPI.
- Polly-based resilience layer.
