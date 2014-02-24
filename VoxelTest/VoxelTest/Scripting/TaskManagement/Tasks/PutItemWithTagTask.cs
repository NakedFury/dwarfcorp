﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DwarfCorp
{
    /// <summary>
    /// Tells a creature that it should find an item with the specified
    /// tags and put it in a given zone.
    /// </summary>
    [Newtonsoft.Json.JsonObject(IsReference = true)]
    internal class PutItemWithTagTask : Task
    {
        public Zone Zone;
        public TagList Tags;

        public PutItemWithTagTask()
        {

        }

        public PutItemWithTagTask(TagList tags, Zone zone)
        {
            Name = "Put Item with tag: " + tags + " in zone " + zone.ID;
            Tags = tags;
            Zone = zone;
        }

        public override Act CreateScript(Creature creature)
        {
            Room room = Zone as Room;
            if(room == null)
            {
                return null;
            }

            if(!creature.Faction.RoomDesignator.IsBuildDesignation(room))
            {
                return null;
            }

            VoxelBuildDesignation voxDesignation = creature.Faction.RoomDesignator.GetBuildDesignation(room);

            if(voxDesignation == null)
            {
                return null;
            }

            RoomBuildDesignation designation = voxDesignation.BuildDesignation;
            return new PutTaggedRoomItemAct(creature.AI, designation, Tags);
        }

        public override float ComputeCost(Creature agent)
        {
            return (Zone == null) ? 1000 : 1.0f;
        }
    }

}