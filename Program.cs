using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using GBX.NET.LZO;
using GBX.NET.ZLib;
using TmEssentials;

Gbx.LZO = new MiniLZO();
Gbx.ZLib = new ZLib();

string path = @"C:\Users\Tobias\Documents\Trackmania2020\Replays\CutTest"; // set to null for command line .exe
string templateName = "_Template.Replay.Gbx";

string[] files = Directory.GetFiles(path, "*.Replay.Gbx", SearchOption.AllDirectories);


Gbx<CGameCtnReplayRecord> gbxTemplate = Gbx.Parse<CGameCtnReplayRecord>(path + @"\" + templateName);
CGameCtnMediaClip segmentedRun = gbxTemplate.Node.Clip;
// segmentedRun.LocalPlayerClipEntIndex = -1; //not sure if matters

foreach (string file in files)
{
    // read replay record
    Gbx<CGameCtnReplayRecord> gbx = Gbx.Parse<CGameCtnReplayRecord>(file);
    CGameCtnReplayRecord record = gbx.Node;
    if (record.RecordData == null) continue;
    
    List<CPlugEntRecordData.EntRecordListElem> entries = record.RecordData.EntList
            .Where(x => x.Samples.Count > 20 && x.Samples.First() is CSceneVehicleVis.EntRecordDelta).ToList(); //20 is an aproximate to not select incomplete entries

    TimeInt32 EndTime = entries.First().Samples.Last().Time;
    
    //check for big jumps in pos, indicator for respawn, remove all previous entries
    // foreach (var entry in entries) {
    //     int? lastRespawnIndex = null;
    //     for (int i = 1; i < entry.Samples.Count; i++)
    //     {
    //         CSceneVehicleVis.EntRecordDelta prev = entry.Samples[i-1] as CSceneVehicleVis.EntRecordDelta;
    //         CSceneVehicleVis.EntRecordDelta current = entry.Samples[i] as CSceneVehicleVis.EntRecordDelta;
    //         if (prev != null && current != null) {
    //             Vec3 diffrence = current.Position - prev.Position;
    //             double distance = Math.Sqrt(diffrence.X * diffrence.X + diffrence.Y * diffrence.Y + diffrence.Z * diffrence.Z);
    //             if (distance > 100) { //100 is an aproximate threshold, adjust as needed
    //                 lastRespawnIndex = i;
    //                 break;
    //             }
    //         }
    //     }
    //     if (lastRespawnIndex != null) {
    //         entry.Samples.RemoveRange(0, lastRespawnIndex.Value-1);
    //     }
    // }

    segmentedRun.Tracks.Add(Utils.CreateTrackFromReplay(record));
}
segmentedRun.Save(path + @"\SegmentedRun.Clip.Gbx");