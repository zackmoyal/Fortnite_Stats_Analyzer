# Fortnite Stats Analyzer

**Fortnite Stats Analyzer** is a full-stack web application that provides detailed real-time statistics and insights for Fortnite players. By integrating with the [FortniteAPI.io](https://fortniteapi.io), users can enter any Fortnite username to instantly view and analyze performance data such as match history, kill/death ratios, win rates, and total kills.

## 🚀 Features

- 🎯 **Player Stats Lookup**: View key performance metrics like K/D ratio, win rate, and total kills across Solo, Duo, and Squad modes.
- 📱 **Responsive UI**: Clean and intuitive interface with a Fortnite-inspired design that works great on both desktop and mobile.
- 🌎 **Global Stats Comparison**: See how individual players stack up against global Fortnite trends and top players.
- ⚡ **Instant Search**: Input a username and get real-time data from the Fortnite API with minimal delay.
- 🔐 **Environment-Based API Key Security**: Utilizes environment variables to securely manage API access.
- 🎯 **Player Stats Lookup**: View key performance metrics like K/D ratio, win rate, and total kills across Solo, Duo, and Squad modes.
- 📱 **Responsive UI**: Clean and intuitive interface with a Fortnite-inspired design that works great on both desktop and mobile.
- 🌎 **Global Stats Comparison**: See how individual players stack up against global Fortnite trends and top players.
- ⚡ **Instant Search**: Input a username and get real-time data from the Fortnite API with minimal delay.
- 🔐 **Environment-Based API Key Security**: Utilizes environment variables to securely manage API access.

## 🛠 Technologies Used

- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Backend**: ASP.NET Core 8 MVC
- **API**: [FortniteAPI.io](https://fortniteapi.io)
- **Hosting**: Render (Dockerized Deployment)
- **Development Tools**: Visual Studio 2022, Git, GitHub

## 🧩 Getting Started (Local Setup)

### ✅ Prerequisites

- [.NET Core SDK 8.0+](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or any compatible IDE)
- **API**: [FortniteAPI.io](https://fortniteapi.io)
- **Hosting**: Render (Dockerized Deployment)
- **Development Tools**: Visual Studio 2022, Git, GitHub

### 📥 Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/zackmoyal/FortniteStatsAnalyzer.git
   ```

2. Navigate into the project directory:

   ```bash
   cd FortniteStatsAnalyzer
   ```

3. Set up your API key (see below)

4. Run the app:

   - Using Visual Studio: Press `F5`
   - Or from the terminal:
     ```bash
     dotnet run
     ```

### 🔑 API Key Setup

To run this project locally, you’ll need a free API key from FortniteAPI.io.

- Sign up at [FortniteAPI.io](https://fortniteapi.io) and get your API key from the dashboard.
- Create a file named `appsettings.Development.json` in the root directory (this file is ignored by Git).
- Paste in the following JSON, replacing `"your-api-key-here"` with your actual key:

```json
{
  "FortniteAPI": {
    "Key": "your-api-key-here"
  }
}
```

## 🌐 Deployment

This app is deployed using Docker on [Render](https://render.com).\
See the live version at:\
➡️ [https://fortnite-stats-analyzer.onrender.com](https://fortnite-stats-analyzer.onrender.com)

## 💡 Future Improvements

- Add search history tracking and charts for long-term player progress
- Integrate OAuth login to allow players to save favorite usernames
- Global leaderboard by game mode
- Dark mode toggle
- API usage dashboard

### ✅ Output Format

- Summarize your analysis and changes in bullet points in chat
- Generate a full downloadable `.txt` report

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 🧑‍💻 Author

Built by [Zack Moyal](https://github.com/zackmoyal)

