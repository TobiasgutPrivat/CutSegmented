using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Plug;
using GBX.NET.Engines.Scene;
using TmEssentials;

static class SegmentedRun {
    static string templateName = "_Template.Replay.Gbx";
    public static CGameCtnMediaClip CreateSegmentedRun(string folder)
    {
        TimeInt32 totalTime = TimeInt32.Zero;

        string path = folder;

        string[] files = Directory.GetFiles(path, "*.Replay.Gbx", SearchOption.AllDirectories);

        Gbx<CGameCtnReplayRecord> gbxTemplate = Gbx.Parse<CGameCtnReplayRecord>(path + @"\" + templateName);
        CGameCtnMediaClip segmentedRun = gbxTemplate.Node.Clip!;

        foreach (string file in files)
        {
            // read replay record
            Gbx<CGameCtnReplayRecord> gbx = Gbx.Parse<CGameCtnReplayRecord>(file);
            CGameCtnReplayRecord record = gbx.Node;
            if (record.RecordData == null) continue;

            CGameCtnMediaTrack track = Utils.CreateTrackFromReplay(record);
            CGameCtnMediaBlockEntity block = (CGameCtnMediaBlockEntity)track.Blocks[0];
            CPlugEntRecordData.EntRecordListElem run = record.RecordData.EntList.First(x => 
                x.Samples.Count > 20 && x.Samples.First() is CSceneVehicleVis.EntRecordDelta); //20 is an aproximate to not select incomplete entries

            TimeInt32 EndTime = run.Samples.Last().Time;

            // check for big jumps in pos, indicator for respawn, remove all previous entries
            int? lastRespawnIndex = null;
            for (int i = 1; i < run.Samples.Count; i++)
            {
                CSceneVehicleVis.EntRecordDelta prev = run.Samples[i-1] as CSceneVehicleVis.EntRecordDelta;
                CSceneVehicleVis.EntRecordDelta current = run.Samples[i] as CSceneVehicleVis.EntRecordDelta;
                if (prev != null && current != null) {
                    Vec3 diffrence = current.Position - prev.Position;
                    double distance = Math.Sqrt(diffrence.X * diffrence.X + diffrence.Y * diffrence.Y + diffrence.Z * diffrence.Z);
                    if (distance > 100) { //100 is an aproximate threshold, adjust as needed
                        lastRespawnIndex = i;
                        break;
                    }
                }
            }

            // time
            // block.StartOffset = -totalTime; // relative to first key
            foreach (var key in block.Keys!) {
                key.Time += (TimeSingle)totalTime;
            }
            totalTime += EndTime;


            segmentedRun.Tracks.Add(track);
        }
        return segmentedRun;
    }
}