using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;
using GBX.NET.ZLib;

Gbx.LZO = new MiniLZO();
Gbx.ZLib = new ZLib();

// // remove Password
// string mapPath = @"C:\Users\Tobias\Documents\Trackmania2020\Maps\240 Tower\_ 240Tower.Map.Gbx";
// Gbx<CGameCtnChallenge> gbx = Gbx.Parse<CGameCtnChallenge>(mapPath);
// CGameCtnChallenge map = gbx.Node;
// map.RemovePassword();
// map.Save(mapPath);

string path = @"C:\Users\Tobias\Documents\Trackmania2020\Replays\240 Tower Segmented";

// // add cam track to a run
// Gbx<CGameCtnMediaClip> gbxTemplate = Gbx.Parse<CGameCtnMediaClip>(path + @"\Skin fix.Clip.Gbx");
// CGameCtnMediaClip segmentedRun = gbxTemplate.Node;
// SegmentedRun.addCamTrack(segmentedRun);

// create segmented run
List<int> skipIndexes = []; //indexes of segments to skip (0-based)
CGameCtnMediaClip segmentedRun = SegmentedRun.CreateSegmentedRun(path, skipIndexes);
segmentedRun.Save(path + @"\SegmentedRun.Clip.Gbx");
Console.WriteLine($"Segmented run saved to {path}\\SegmentedRun.Clip.Gbx");