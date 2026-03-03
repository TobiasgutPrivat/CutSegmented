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
//  = new CGameCtnMediaClip();
// segmentedRun.CreateChunk<CGameCtnMediaClip.Chunk0307900D>().Version = 1;
// segmentedRun.CreateChunk<CGameCtnMediaClip.Chunk0307900E>().Data = new byte[8];
// CGameCtnMediaTrack newTrack = new CGameCtnMediaTrack();
// newTrack.CreateChunk<CGameCtnMediaTrack.Chunk03078001>();
// newTrack.CreateChunk<CGameCtnMediaTrack.Chunk03078005>().Version = 1;
// segmentedRun.Tracks.Add(newTrack);
// CGameCtnMediaTrack playerCamera = segmentedRun.Tracks[0];
// CGameCtnMediaTrack playerTrack = segmentedRun.Tracks[1];
// CGameCtnMediaBlockEntity playerTemplateBlock = (CGameCtnMediaBlockEntity)playerTrack.Blocks[0];
// playerTemplateBlock.GhostName = "Test";

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

    // create Block
    var block = new CGameCtnMediaBlockEntity();
    block.GhostName = record.Ghosts[0].GhostNickname;
    block.PlayerModel = record.Ghosts[0].PlayerModel;
    block.SkinNames = record.Ghosts[0].SkinPackDescs;
    block.RecordData = record.Ghosts[0].RecordData;
    block.CreateChunk<CGameCtnMediaBlockEntity.Chunk0329F000>();
    block.CreateChunk<CGameCtnMediaBlockEntity.Chunk0329F002>();
    // playerTrack.Blocks.Add(block);
    
    block.Keys = new List<CGameCtnMediaBlockEntity.Key>();
    var key1 = new CGameCtnMediaBlockEntity.Key();
    key1.Time = TimeSingle.Zero;
    key1.TrailIntensity = 1;
    key1.SelfIllumIntensity = 1;
    key1.U01 = 1;
    var key2 = new CGameCtnMediaBlockEntity.Key();
    key2.Time = EndTime;
    key2.TrailIntensity = 1;
    key2.SelfIllumIntensity = 1;
    key2.U01 = 1;
    block.Keys.Add(key1);
    block.Keys.Add(key2);
    block.Keys.Add(key1);
    block.Keys.Add(key2);
    block.NoticeRecords = [0];

    var blocks = new List<CGameCtnMediaBlock>();
    blocks.Add(block);

    var track = new CGameCtnMediaTrack();
    track.Name = $"Ghost:{record.Ghosts[0].GhostNickname}";
    track.Blocks = blocks;
    track.CreateChunk<CGameCtnMediaTrack.Chunk03078001>();
    track.CreateChunk<CGameCtnMediaTrack.Chunk03078005>().Version = 1;
    segmentedRun.Tracks.Add(track);


    block.RecordData = record.RecordData;

    if (block.GhostName == null) {
        block.GhostName = record.Ghosts[0].GhostNickname;
        block.PlayerModel = record.Ghosts[0].PlayerModel;
    }
}
segmentedRun.Save(path + @"\SegmentedRun.Clip.Gbx");