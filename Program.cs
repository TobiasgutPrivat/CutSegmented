using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;
using GBX.NET.ZLib;

Gbx.LZO = new MiniLZO();
Gbx.ZLib = new ZLib();

string path = @"C:\Users\Tobias\Documents\Trackmania2020\Replays\STR8";
List<int> skipIndexes = [4]; //indexes of segments to skip (0-based)
CGameCtnMediaClip segmentedRun = SegmentedRun.CreateSegmentedRun(path, skipIndexes);
segmentedRun.Save(path + @"\SegmentedRun.Clip.Gbx");
Console.WriteLine($"Segmented run saved to {path}\\SegmentedRun.Clip.Gbx");