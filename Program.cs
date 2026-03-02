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

CGameCtnMediaClip segmentedRun = new CGameCtnMediaClip();
segmentedRun.CreateChunk<CGameCtnMediaClip.Chunk0307900D>().Version = 1;
segmentedRun.CreateChunk<CGameCtnMediaClip.Chunk0307900E>().Data = new byte[8];
CGameCtnMediaTrack newTrack = new CGameCtnMediaTrack();
newTrack.CreateChunk<CGameCtnMediaTrack.Chunk03078001>();
newTrack.CreateChunk<CGameCtnMediaTrack.Chunk03078005>().Version = 1;
CGameCtnMediaBlockEntity newBlock = new CGameCtnMediaBlockEntity();
newBlock.CreateChunk<CGameCtnMediaBlockEntity.Chunk0329F000>();
newBlock.CreateChunk<CGameCtnMediaBlockEntity.Chunk0329F002>();
segmentedRun.Tracks.Add(newTrack);
newTrack.Blocks.Add(newBlock);
CPlugEntRecordData.EntRecordListElem? entListElem = null;

foreach (string file in files)
{
    Gbx<CGameCtnReplayRecord> gbx = Gbx.Parse<CGameCtnReplayRecord>(file);
    CGameCtnReplayRecord record = gbx.Node;
    if (record.RecordData == null) continue;

    // add clips
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
        if (newBlock.RecordData == null) {
            newBlock.RecordData = record.RecordData;
            entListElem = entry;
        } else {
            entListElem.Samples.AddRange(entry.Samples);
        }
    }

    if (newBlock.GhostName == null) {
        newBlock.GhostName = record.Ghosts[0].GhostNickname;
        newBlock.PlayerModel = record.Ghosts[0].PlayerModel;
    }
}
segmentedRun.Save(path + @"\SegmentedRun.Clip.Gbx");