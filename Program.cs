using GBX.NET;
using GBX.NET.LZO;
using GBX.NET.ZLib;

Gbx.LZO = new MiniLZO();
Gbx.ZLib = new ZLib();

string path = @"C:\Users\Tobias\Documents\Trackmania2020\Replays\CutTest";

SegmentedRun.CreateSegmentedRun(path).Save(path + @"\SegmentedRun.Clip.Gbx");