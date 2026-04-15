# Go Do It

A free, privacy-first desktop to-do list and calendar app built with C# and Avalonia UI. Go Do It has a very simple design
made to fit on a singular screen as to not overwhelm the user with too many text boxes or features. It contains a feed at
the top with all of the tasks the user creates in chronological order (based off due date). Directly underneath is the
calendar that displays each of the tasks that are due that day. To the right of the calendar, there is the task maker. The
user can input all the information they need in it, and the task will be displayed in both the feed and the calendar. The
specific fields the user can input are the title, description, due date (chosen via date picker), how often it repeats 
(daily, weekly, etc.), the category (via dropdown menu, categories give different colors to tasks), and subtasks.

The way we focus on privacy is by storing all the data locally in a JSON file. Storing information this way eliminates the 
threat of a database's cloud shutting down, and reduces the chances of a malicious actor getting access to the user's 
information. It is important to note that because all the data is stored in a JSON file, information can still be lost in the event that the file is deleted or lost in some way. Fortunately, it is easy to export the data and import it to another 
device. Also, because the JSON is in plaintext, the user can edit the information in the JSON as they please.

## Quick Start

1. **Prerequisites**
   - .NET 9.0 SDK (download from https://dotnet.microsoft.com/download)
   - Git

2. **Clone and build**
   ```bash
   git clone https://github.com/omartheone104/Go-Do-It.git
   cd Go-Do-It
   dotnet restore
   dotnet build
   ```

3. **Run the app**
    ```bash
    dotnet run --project GoDoIt/GoDoIt.csproj
    ```

4. **Run unit tests**
    ```bash
    dotnet test
    ```

## Results

To reproduce the results presented in the paper, follow the steps outlined in Results/README.md

## Project Structure

- GoDoIt/ - Main Avalonia desktop application
- GoDoIt.Tests/ - Unit tests
- GoDoIt.sln - Solution file
- Results/ - Data and Instructions to replicate figures
