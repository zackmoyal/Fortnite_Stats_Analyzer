# Fortnite Stats Analyzer

**Fortnite Stats Analyzer** is a full-stack web application that provides detailed real-time statistics and insights for Fortnite players. By integrating with the [FortniteAPI.io](https://fortniteapi.io), users can enter any Fortnite username to instantly view and analyze performance data such as match history, kill/death ratios, win rates, and total kills.

🟢 **Live Demo**:  
👉 [https://fortnite-stats-analyzer.onrender.com](https://fortnite-stats-analyzer.onrender.com)

## 🚀 Features

- 🎯 **Player Stats Lookup**: View key performance metrics like K/D ratio, win rate, and total kills across Solo, Duo, and Squad modes.
- 📱 **Responsive UI**: Clean and intuitive interface with a Fortnite-inspired design that works great on both desktop and mobile.
- 🌎 **Global Stats Comparison**: See how individual players stack up against global Fortnite trends and top players.
- ⚡ **Instant Search**: Input a username and get real-time data from the Fortnite API with minimal delay.
- 🔐 **Environment-Based API Key Security**: Utilizes environment variables to securely manage API access.

## 🛠 Technologies Used

- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Backend**: ASP.NET Core 8 MVC
- **API**: [FortniteAPI.io](https://fortniteapi.io)
- **Hosting**: Render (Dockerized Deployment)
- **Development Tools**: Visual Studio 2022, Git, GitHub

## 🧩 Getting Started (Local Setup)

### ✅ Prerequisites

- [.NET Core SDK 8.0+](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or any compatible IDE)

### 📥 Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/zackmoyal/FortniteStatsAnalyzer.git
   ```
2. Navigate into the project directory:
   ```bash
   cd FortniteStatsAnalyzer
   ```
3. Add your API key to `appsettings.json` or set it as an environment variable:
   ```json
   {
     "FortniteApi": {
       "ApiKey": "your-api-key-here"
     }
   }
   ```
4. Run the app:
   - Using Visual Studio: Press `F5`
   - Or from the terminal:
     ```bash
     dotnet run
     ```

## 🌐 Deployment

This app is deployed using Docker on [Render](https://render.com).  
See the live version at:  
➡️ [https://fortnite-stats-analyzer.onrender.com](https://fortnite-stats-analyzer.onrender.com)

## 💡 Future Improvements

- Add search history tracking and charts for long-term player progress
- Integrate OAuth login to allow players to save favorite usernames
- Global leaderboard by game mode
- Dark mode toggle
- API usage dashboard

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 🧑‍💻 Author

Built by [Zack Moyal](https://github.com/zackmoyal)
