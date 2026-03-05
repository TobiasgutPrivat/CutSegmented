using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using TmEssentials;

static class Utils
{
    public static CGameCtnMediaTrack CreateTrackFromReplay(CGameCtnReplayRecord replay) {
        List<CPlugEntRecordData.EntRecordListElem> entries = replay.RecordData.EntList
            .Where(x => x.Samples.Count > 20 && x.Samples.First() is CSceneVehicleVis.EntRecordDelta).ToList(); //20 is an aproximate to not select incomplete entries

        TimeInt32 EndTime = entries.First().Samples.Last().Time;

        var block = new CGameCtnMediaBlockEntity();
        block.GhostName = replay.Ghosts[0].GhostNickname;
        block.PlayerModel = replay.Ghosts[0].PlayerModel;
        block.SkinNames = replay.Ghosts[0].SkinPackDescs;
        block.RecordData = replay.RecordData;
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
        block.NoticeRecords = [2,0]; // important
        var blockC1 = block.CreateChunk<CGameCtnMediaBlockEntity.Chunk0329F000>();
        blockC1.Version = 11;
        blockC1.U02 = 1;
        block.CreateChunk<CGameCtnMediaBlockEntity.Chunk0329F002>();
        // block.CreateChunk<CGameCtnMediaBlockEntity.Chunk0329F003>(); // doesnt matter

        var blocks = new List<CGameCtnMediaBlock>();
        blocks.Add(block);

        var track = new CGameCtnMediaTrack();
        track.Name = $"{replay.Ghosts[0].GhostNickname}";
        track.Blocks = blocks;
        track.CreateChunk<CGameCtnMediaTrack.Chunk03078001>();
        track.CreateChunk<CGameCtnMediaTrack.Chunk03078005>().Version = 1;

        return track;
    }
}