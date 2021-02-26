using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferencePoint
{

    public List<float> Position { get; set; }
    public int MemberCount { get; set; }
    public List<Tuple<GameObject, float>> LastFrontMembers;

    public ReferencePoint(List<float> position)
    {
        Position = position;
        MemberCount = 0;
        LastFrontMembers = new List<Tuple<GameObject, float>>();
    }

    public void AddMember()
    {
        MemberCount++;
    }

    public void AddLastFrontMember(GameObject machine, float distance)
    {
        LastFrontMembers.Add(new Tuple<GameObject, float>(machine, distance));
    }
}
