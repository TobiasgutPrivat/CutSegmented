using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Scene;
using TmEssentials;

static class SegmentedRun {
    static string templateName = "_Template.Replay.Gbx";
    public static CGameCtnMediaClip CreateSegmentedRun(string folder)
    {
        // TimeInt32 totalTime = TimeInt32.Zero;

        string path = folder;

        string[] files = Directory.GetFiles(path, "*.Replay.Gbx", SearchOption.AllDirectories);

        Gbx<CGameCtnReplayRecord> gbxTemplate = Gbx.Parse<CGameCtnReplayRecord>(path + @"\" + templateName);
        CGameCtnMediaClip segmentedRun = gbxTemplate.Node.Clip!;
        List<CSceneVehicleVis.EntRecordDelta>? lastSegment = null;
        CGameCtnMediaBlockEntity? lastBlock = null;

        foreach (string file in files)
        {
            // read replay record
            Gbx<CGameCtnReplayRecord> gbx = Gbx.Parse<CGameCtnReplayRecord>(file);
            CGameCtnReplayRecord record = gbx.Node;
            if (record.RecordData == null) continue;

            CGameCtnMediaTrack track = Utils.CreateTrackFromReplay(record);
            CGameCtnMediaBlockEntity block = (CGameCtnMediaBlockEntity)track.Blocks[0];
            List<CSceneVehicleVis.EntRecordDelta> segment = record.RecordData.EntList.First(x => 
                x.Samples.Count > 20 && x.Samples.First() is CSceneVehicleVis.EntRecordDelta).Samples.OfType<CSceneVehicleVis.EntRecordDelta>().ToList(); //20 is an aproximate to not select incomplete entries

            TimeInt32 EndTime = segment.Last().Time;

            // check for big jumps in pos, indicator for respawn, remove all previous entries
            int? lastRespawnIndex = null;
            for (int i = 1; i < segment.Count; i++)
            {
                CSceneVehicleVis.EntRecordDelta prev = segment[i-1];
                CSceneVehicleVis.EntRecordDelta current = segment[i];
                if (prev != null && current != null) {
                    Vec3 diffrence = current.Position - prev.Position;
                    double distance = Math.Sqrt(diffrence.X * diffrence.X + diffrence.Y * diffrence.Y + diffrence.Z * diffrence.Z);
                    if (distance > 100) { //100 is an aproximate threshold, adjust as needed
                        lastRespawnIndex = i;
                        break;
                    }
                }
            }

            CSceneVehicleVis.EntRecordDelta firstElement = lastRespawnIndex == null ? segment[0] : segment[lastRespawnIndex.Value]; //element where the wanted Part
            block.StartOffset = firstElement.Time; // relative to first key

            if (lastSegment != null) {
                //find closest key in lastSegment for the start of the new segment
                TimeSingle closestTime = TimeSingle.Zero; // offset relative to previous segment
                double closestDistance = double.MaxValue;
                foreach (var split in lastSegment) {
                    var distance = Math.Sqrt(
                        Math.Pow(split.Position.X - firstElement.Position.X, 2) +
                        Math.Pow(split.Position.Y - firstElement.Position.Y, 2) +
                        Math.Pow(split.Position.Z - firstElement.Position.Z, 2)
                    );
                    if (distance < closestDistance) {
                        closestDistance = distance;  
                        //TODO maybe align more precisely according to where in between 2 closest keys the new segment starts
                        closestTime = split.Time;
                    }
                }
                block.Keys[0].Time = lastBlock.Keys[0].Time - lastBlock.StartOffset + closestTime; // set start key to matching time in previous segment
                lastBlock.Keys[1].Time = block.Keys[0].Time; // set end key of previous segment to the new start time
            } else {
                block.Keys[0].Time = new TimeSingle(0); // for the first segment, just set it to the offset
            }

            block.Keys[1].Time = block.Keys[0].Time - block.StartOffset + (TimeSingle)EndTime;
            
            lastSegment = segment;
            lastBlock = block;
            track.Name = $"Segment {segmentedRun.Tracks.Count + 1}";

            segmentedRun.Tracks.Add(track);
        }

        // add camera track
        var cameraTrack = new CGameCtnMediaTrack();
        cameraTrack.CreateChunk<CGameCtnMediaTrack.Chunk03078001>();
        cameraTrack.CreateChunk<CGameCtnMediaTrack.Chunk03078005>().Version = 1;
        cameraTrack.Name = $"Player camera";
        var blocks = new List<CGameCtnMediaBlock>();
        cameraTrack.Blocks = blocks;

        for (int i = 0; i < segmentedRun.Tracks.Count; i++) {
            var track = segmentedRun.Tracks[i];
            CGameCtnMediaBlockEntity entityBlock = (CGameCtnMediaBlockEntity)track.Blocks[0];
            var block = new CGameCtnMediaBlockCameraGame();
            block.CreateChunk<CGameCtnMediaBlockCameraGame.Chunk03084007>();
            block.Start = entityBlock.Keys[0].Time;
            block.End = entityBlock.Keys[1].Time;
            block.ClipEntId = i+1;
            blocks.Add(block);
        }

        segmentedRun.Tracks.Add(cameraTrack);

        return segmentedRun;
    }
}