using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using GBX.NET.LZO;
using GBX.NET.ZLib;

Gbx.LZO = new MiniLZO();
Gbx.ZLib = new ZLib();

string path = @"C:\Users\Tobias\Documents\Trackmania2020\Replays\CutTest"; // set to null for command line .exe

string[] files = Directory.GetFiles(path, "*.Replay.Gbx", SearchOption.AllDirectories);

CGameCtnReplayRecord? segmentedRun = null;

foreach (string file in files)
{
    Gbx<CGameCtnReplayRecord> gbx = Gbx.Parse<CGameCtnReplayRecord>(file);
    CGameCtnReplayRecord record = gbx.Node;
    if (record.RecordData == null) continue;

    if (segmentedRun == null) {
        segmentedRun = record;
        // maybe clear other stuff
        continue;
    }

    // add clips
    CGameCtnMediaTrack track = new CGameCtnMediaTrack();
    CGameCtnMediaBlockEntity trackEntity = new CGameCtnMediaBlockEntity();
    track.Blocks.Add(trackEntity);
    trackEntity.GhostName = record.Ghosts[0].GhostNickname;
    trackEntity.PlayerModel = new Ident("CarSport","Common","Nadeo");
    List<CPlugEntRecordData.EntRecordListElem> entries = record.RecordData.EntList
            .Where(x => x.Samples.Count > 20 && x.Samples.First() is CSceneVehicleVis.EntRecordDelta).ToList(); //20 is an aproximate to not select incomplete entries

    //check for big jumps in pos, indicator for respawn, remove all previous entries
    foreach (var entry in entries) {
    int? lastRespawnIndex = null;
        for (int i = 1; i < entry.Samples.Count; i++)
        {
            CSceneVehicleVis.EntRecordDelta prev = entry.Samples[i-1] as CSceneVehicleVis.EntRecordDelta;
            CSceneVehicleVis.EntRecordDelta current = entry.Samples[i] as CSceneVehicleVis.EntRecordDelta;
            if (prev != null && current != null) {
                Vec3 diffrence = current.Position - prev.Position;
                double distance = Math.Sqrt(diffrence.X * diffrence.X + diffrence.Y * diffrence.Y + diffrence.Z * diffrence.Z);
                if (distance > 100) { //100 is an aproximate threshold, adjust as needed
                    lastRespawnIndex = i;
                    break;
                }
            }
        }
        if (lastRespawnIndex != null) {
            entry.Samples.RemoveRange(0, lastRespawnIndex.Value-1);
        }
    }

    trackEntity.RecordData = new CPlugEntRecordData();
    trackEntity.RecordData.EntList.AddRange(entries);

}

segmentedRun?.Save(path + @"\SegmentedRun.Replay.Gbx");