# WR to Author

A tool to set the current WR to be the Author and Authortime of the map

This will overwrite your mapfiles and directly reupload to Nadeo Services

## Usage

1. Download .exe [here](https://1drv.ms/u/c/bf971998d3da6c52/EaWeEMXzxK5CqqvKDWRsPTUB195KQDIlk-OYmBHHPMHbag?e=anxYOD)
2. Drag File or Folder onto the .exe (or use cli)
3. Follow the instructions (you will need to enter your ubisoft login)

For more flexibility you can pull the Repo and modify/build it yourself

## map process
probably best to:

1. create/generate map
2. calc lightmaps
3. upload via club campaign
4. let validate
5. WRtoWuthor
5. upload via club campaign

## publishing

dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false /p:EnableCompressionInSingleFile=true -o .\publish