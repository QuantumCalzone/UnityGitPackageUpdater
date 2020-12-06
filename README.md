# Unity Git Package Updater
Easily update Unity packages hosted via git

## How to install
### Via 2019.4 and later
- In Unity, open Window/Package Manager
- Select the 	**+** button at the top left
- Select 	**Add package from git URL...**
- Paste in ```https://github.com/QuantumCalzone/UnityGitPackageUpdater.git#upm```
- Click the **Add** button

### Via 2019.3 and eariler
- In your Unity Project, open up the manifest.json file located in your Packages folder
- below ```"dependencies": {``` add the line ```"com.quantumcalzone.unityframework": "https://github.com/QuantumCalzone/UnityGitPackageUpdater.git#upm",```
