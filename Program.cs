using GBX.NET;
using GBX.NET.LZO;

Gbx.LZO = new MiniLZO();
// Gbx.ZLib = new ZLib();

string path = @"C:\Users\Tobias\Documents\Trackmania2020\Maps\Altered Process\Winter 2026\Winter 2026 WRTrace"; // set to null for command line .exe

Directory.GetFiles(path, "*.Replay.Gbx", SearchOption.AllDirectories);