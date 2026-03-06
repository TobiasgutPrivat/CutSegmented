using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.Scene;
using TmEssentials;

static class SegmentedRun {
    static string templateName = "_Template.Replay.Gbx";
    public static CGameCtnMediaClip CreateSegmentedRun(string folder, List<int> skipIndexes)
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

            List<CSceneVehicleVis.EntRecordDelta> segment = record.RecordData.EntList.First(x => //First usually has last respawn
                x.Samples.Count > 20 && x.Samples.First() is CSceneVehicleVis.EntRecordDelta).Samples.OfType<CSceneVehicleVis.EntRecordDelta>().ToList(); //20 is an aproximate to not select incomplete entries
            //Theres usually one EntList with a full replay (longest one) but not needed in this case

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
                int closestIndex = 0; // offset relative to previous segment
                double closestDistance = double.MaxValue;
                int index = 0;
                foreach (var split in lastSegment) {
                    var distance = getDistance(firstElement, split);
                    if (distance < closestDistance) {
                        closestDistance = distance;  
                        //TODO maybe align more precisely according to where in between 2 closest keys the new segment starts
                        closestIndex = index;
                    }
                    index++;
                }
                // calc
                int nextIndex = Math.Min(closestIndex + 1, lastSegment.Count - 1);
                int prevIndex = Math.Max(closestIndex-1,0);
                int secondClosest = getDistance(firstElement, lastSegment[nextIndex]) < getDistance(firstElement, lastSegment[prevIndex]) ? nextIndex : prevIndex;
                float firstDistance = getDistance(firstElement, lastSegment[closestIndex]);
                float secondDistance = getDistance(firstElement, lastSegment[secondClosest]);
                float firstWeight = 1 - (firstDistance / (firstDistance + secondDistance));
                float secondWeight = 1 - firstWeight;
                TimeSingle closestTime = lastSegment[closestIndex].Time * firstWeight + lastSegment[secondClosest].Time * secondWeight; // aproximate to the middle of the 2 closest keys, adjust as needed

                block.Keys[0].Time = lastBlock.Keys[0].Time - lastBlock.StartOffset + closestTime; // set start key to matching time in previous segment
                lastBlock.Keys[1].Time = block.Keys[0].Time; // set end key of previous segment to the new start time
            } else {
                block.Keys[0].Time = new TimeSingle(0); // for the first segment, just set it to the offset
            }

            block.Keys[1].Time = block.Keys[0].Time - block.StartOffset + (TimeSingle)EndTime;
            
            lastSegment = segment;
            lastBlock = block;
            track.Name = Path.GetFileName(file);

            if (!skipIndexes.Contains(segmentedRun.Tracks.Count)){
                segmentedRun.Tracks.Add(track);
            }
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

    static float getDistance(CSceneVehicleVis.EntRecordDelta a, CSceneVehicleVis.EntRecordDelta b) {
        Vec3 diffrence = b.Position - a.Position;
        return (float)Math.Sqrt(diffrence.X * diffrence.X + diffrence.Y * diffrence.Y + diffrence.Z * diffrence.Z);
    }
}