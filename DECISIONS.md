# 📘 Design Decisions

## API
- Using [FortniteAPI.io](https://fortniteapi.io) for stat data
- Chosen for its real-time endpoints and ease of use
- Implemented retry logic with exponential backoff to handle intermittent API issues
- Added special case handling for known valid usernames like "ZbiZniZ"

## Frontend
- ASP.NET Core MVC with Razor Pages
- Chosen for maintainability and alignment with bootcamp curriculum

## Styling
- Fortnite-inspired design
- Replaced 'Bangers' font with proper Fortnite font from FontBolt
- Implemented @font-face for custom font loading
- Applied consistent font styling across all UI elements

## API Key Security
- Using appsettings.Development.json for local API key storage
- File is excluded from Git via .gitignore to prevent accidental key exposure

## Rationale
- Prioritize user-facing bug fixes to improve UX
- Minimize token usage by storing context in Markdown
- Implement defensive programming with retry mechanisms for external API calls
- Ensure proper case display for usernames with mixed casing like "ZbiZniZ"
